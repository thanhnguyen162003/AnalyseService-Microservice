using Algolia.Search.Http;
using Application;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Application.UserServiceRpc;

public class DailyTaskService : BackgroundService
{
    private TimeSpan _targetTime = new TimeSpan(23, 59, 50);
    private readonly UserServiceRpc.UserServiceRpcClient _userServiceRpcClient;
    private readonly AnalyseDbContext _dbContext;
    public DailyTaskService(UserServiceRpc.UserServiceRpcClient userServiceRpcClient, AnalyseDbContext dbContext)
    {
        _userServiceRpcClient = userServiceRpcClient ?? throw new ArgumentNullException(nameof(userServiceRpcClient));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var utcPlus7Zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // UTC+7
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, utcPlus7Zone);
            var nextRun = now.Date + _targetTime;
            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await RunMe();
            }
        }
    }

    private async Task RunMe()
    {
        var response = await _userServiceRpcClient.GetCountUserAsync(new UserCountRequest()
        {
            Amount = 0,
            IsCount = false,
            Type = "Day"
        });

        var result = response.Activities.Select(x => new UserActivityModel
        {
            Date = DateTime.Parse(x.Date),
            Moderators = x.Moderators,
            Students = x.Students,
            Teachers = x.Teachers

        }).ToList();
        await _dbContext.UserActivityModel.InsertManyAsync(result);
    }
}

