using System.Net;
using Application.Common.Kafka;
using Application.Services.CacheService.Intefaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SharedProject.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Application.Consumer;

public class UserDataAnalyseConsumer : KafkaConsumerBase<UserDataAnalyseModel>
{
    public UserDataAnalyseConsumer(IConfiguration configuration, ILogger<UserDataAnalyseConsumer> logger, IServiceProvider serviceProvider)
        : base(configuration, logger, serviceProvider, "recommend_onboarding", "user_data_analyse_group")
    {
    }

    protected override async Task ProcessMessage(string message, IServiceProvider serviceProvider)
    {
        var _redis = serviceProvider.GetRequiredService<IOrdinaryDistributedCache>();
        var _logger = serviceProvider.GetRequiredService<ILogger<UserDataAnalyseConsumer>>();


        UserDataAnalyseModel subjectImageModel = JsonSerializer.Deserialize<UserDataAnalyseModel>(message);
        string key = $"{subjectImageModel.UserId}";
        string? userData = await _redis.GetStringAsync(key);
        
        try
        {
            // If the cached data exists, remove it and save the new data
            if (!string.IsNullOrEmpty(userData))
            {
                await _redis.RemoveAsync(key);
                _logger.LogInformation("Existing cache for key {Key} found and removed.", key);
            }

            // Serialize and save the new data in Redis
            var loopHandling = new JsonSerializerSettings 
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            await _redis.SetStringAsync(key, JsonConvert.SerializeObject(subjectImageModel, loopHandling));
            _logger.LogInformation("New data for key {Key} has been saved successfully.", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing cache operations for key {Key}.", key);
        }
    }
}
