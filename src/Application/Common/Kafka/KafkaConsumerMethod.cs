using Application.Common.Interfaces.KafkaInterface;
using Confluent.Kafka;

namespace Application.Common.Kafka;

public class KafkaConsumerMethod : IKafkaConsumerMethod
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumerMethod> _logger;
    public KafkaConsumerMethod(IConfiguration configuration, ILogger<KafkaConsumerMethod> logger)
    {
        _logger = logger;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            SaslUsername = configuration["Kafka:SaslUsername"],
            SaslPassword = configuration["Kafka:SaslPassword"],
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.Plain,
            GroupId = "Quartz",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }
    public async Task<string> ConsumeByKeyAsync(string topicName, string key, CancellationToken stoppingToken)
    {
        _consumer.Subscribe(topicName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(500));
                    if (consumeResult != null && consumeResult.Message.Key == key)
                    {
                        // Filter messages by the specified key
                        return consumeResult.Message.Value;
                    }
                    else if (consumeResult != null)
                    {
                        _logger.LogInformation($"Message with key {consumeResult.Message.Key} does not match the specified key.");
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Consume error: {e.Error.Reason}");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumption was canceled.");
                    break; // exit the loop when cancelled
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing Kafka message: {ex.Message}");
                }
            }
        }
        finally
        {
            // Ensure consumer is closed to free resources
            _consumer.Close();
            _consumer.Dispose();
        }

        return string.Empty;
    }
}
