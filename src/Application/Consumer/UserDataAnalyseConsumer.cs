using System.Net;
using Application.Common.Kafka;
using Application.Services.CacheService.Intefaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
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
        // var _redis = serviceProvider.GetRequiredService<IOrdinaryDistributedCache>();
        var context = serviceProvider.GetRequiredService<AnalyseDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<UserDataAnalyseConsumer>>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();


        UserDataAnalyseModel userModel = JsonSerializer.Deserialize<UserDataAnalyseModel>(message);
        try
        {
            UserAnalyseEntity entity = mapper.Map<UserAnalyseEntity>(userModel);
            entity.Id = ObjectId.GenerateNewId().ToString();
            await context.UserAnalyseEntity.InsertOneAsync(entity);
            logger.LogInformation($"UserDataAnalyse entity added: {entity.Id}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing cache operations for key {ex}.", ex.Message);
        }
    }
}
