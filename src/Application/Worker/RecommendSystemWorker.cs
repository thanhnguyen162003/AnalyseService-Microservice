namespace Application.Worker;

public class RecommendSystemWorker : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(20);
    private readonly ILogger<RecommendSystemWorker> _logger;
    private readonly IServiceScopeFactory _factory;
    private int _executionCount = 0;
    public bool IsEnabled { get; set; }

    public RecommendSystemWorker(ILogger<RecommendSystemWorker> logger, IServiceScopeFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_period);
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                if (!IsEnabled)
                {
                    await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                    BackgroundTaskService sampleService = asyncScope.ServiceProvider.GetRequiredService<BackgroundTaskService>();
                    await sampleService.ProcessDataAnalyzeOneDay(stoppingToken);
                    _executionCount++;
                    _logger.LogInformation(
                        $"Executed RecommendSystemWorker - Count: {_executionCount}");
                }
                else
                {
                    _logger.LogInformation(
                        "Skipped RecommendSystemWorker");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    $"Failed to execute RecommendSystemWorker with exception message {ex.Message}. Good luck next round!");
            }
        }
    }
}
