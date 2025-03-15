using Confluent.Kafka;
using System.Diagnostics;

namespace Application.Common.Kafka
{
    public abstract class KafkaConsumerBase<T> : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KafkaConsumerBase<T>> _logger;
        private readonly string _topicName;
        private readonly SemaphoreSlim _throttler;
        
        // Metrics tracking
        private long _totalMessagesProcessed = 0;
        private long _failedMessages = 0;
        private readonly Stopwatch _processingStopwatch = new Stopwatch();
        
        // Health monitoring
        private volatile bool _isHealthy = true;
        private DateTime _lastSuccessfulConsume = DateTime.UtcNow;

        protected KafkaConsumerBase(
            IConfiguration configuration, 
            ILogger<KafkaConsumerBase<T>> logger, 
            IServiceProvider serviceProvider, 
            string topicName, 
            string groupId,
            int maxConcurrentProcessing = 10)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _topicName = topicName;
            _throttler = new SemaphoreSlim(maxConcurrentProcessing);
            
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                SaslUsername = configuration["Kafka:SaslUsername"],
                SaslPassword = configuration["Kafka:SaslPassword"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                
                // Performance optimizations
                MaxPollIntervalMs = 300000,     // 5 minutes
                SessionTimeoutMs = 30000,       // 30 seconds
                FetchMaxBytes = 1048576,        // 1MB
                FetchMinBytes = 10240,          // 10KB
                FetchWaitMaxMs = 500,           // 500ms
                QueuedMaxMessagesKbytes = 1048576, // 1MB
                StatisticsIntervalMs = 5000     // Enable statistics for monitoring
            };
            
            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, error) => 
                {
                    _logger.LogError($"Kafka error: {error.Reason}");
                    if (error.IsFatal)
                    {
                        _isHealthy = false;
                    }
                })
                .Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            _consumer.Subscribe(_topicName);
            _logger.LogInformation($"Subscribed to topic: {_topicName}");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (consumeResult != null)
                    {
                        _lastSuccessfulConsume = DateTime.UtcNow;
                        
                        // Wait for a processing slot using the throttler
                        await _throttler.WaitAsync(stoppingToken);
                        
                        // Process the message with controlled parallelism
                        var processTask = Task.Run(async () => 
                        {
                            try 
                            {
                                _processingStopwatch.Restart();
                                
                                // Process with retry logic
                                await ProcessMessageWithRetryAsync(consumeResult.Message.Value, scope.ServiceProvider);
                                
                                _processingStopwatch.Stop();
                                
                                // Track metrics
                                Interlocked.Increment(ref _totalMessagesProcessed);
                                
                                // Log processing completion
                                if (_totalMessagesProcessed % 100 == 0)
                                {
                                    _logger.LogInformation(
                                        $"Processed {_totalMessagesProcessed} messages total, " +
                                        $"last message processed in {_processingStopwatch.ElapsedMilliseconds}ms");
                                }
                                
                                // Commit the offset after successful processing
                                try
                                {
                                    _consumer.Commit(consumeResult);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error committing offset: {ex.Message}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Interlocked.Increment(ref _failedMessages);
                                _logger.LogError($"Error processing message: {ex.Message}");
                            }
                            finally 
                            {
                                _throttler.Release();
                            }
                        }, stoppingToken);
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Consume error: {e.Error.Reason}");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumption was canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in consumer loop: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            
            // Clean shutdown
            try
            {
                _consumer.Unsubscribe();
                _consumer.Close();
                _logger.LogInformation("Kafka consumer closed gracefully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during consumer shutdown: {ex.Message}");
            }
        }

        protected abstract Task ProcessMessage(string message, IServiceProvider serviceProvider);
        
        private async Task ProcessMessageWithRetryAsync(string message, IServiceProvider serviceProvider)
        {
            int retryCount = 0;
            const int maxRetries = 3;
            
            while (true)
            {
                try
                {
                    await ProcessMessage(message, serviceProvider);
                    return; // Success
                }
                catch (Exception ex)
                {
                    if (++retryCount > maxRetries)
                    {
                        _logger.LogError($"Failed to process message after {maxRetries} attempts. Error: {ex.Message}");
                        throw; // Rethrow after max retries
                    }
                    
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount)); // Exponential backoff
                    _logger.LogWarning($"Retry {retryCount}/{maxRetries} after {delay.TotalSeconds}s. Error: {ex.Message}");
                    await Task.Delay(delay);
                }
            }
        }
        
        // Health check method
        public bool IsHealthy() => 
            _isHealthy && DateTime.UtcNow - _lastSuccessfulConsume < TimeSpan.FromMinutes(5);
            
        // Get consumer metrics
        public ConsumerMetrics GetMetrics() => new ConsumerMetrics
        {
            MessagesProcessed = _totalMessagesProcessed,
            FailedMessages = _failedMessages,
            IsHealthy = IsHealthy()
        };
        
        public class ConsumerMetrics
        {
            public long MessagesProcessed { get; set; }
            public long FailedMessages { get; set; }
            public bool IsHealthy { get; set; }
        }
    }
}
