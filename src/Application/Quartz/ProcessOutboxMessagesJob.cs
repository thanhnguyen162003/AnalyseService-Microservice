using Application.Services.CacheService.Intefaces;
using Infrastructure.Data;
using Newtonsoft.Json;
using Quartz;

namespace Application.Quartz;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly AnalyseDbContext _dbContext;
    private readonly IPublisher _publisher;

    public ProcessOutboxMessagesJob(
        AnalyseDbContext dbContext,
        IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        
    }
}
