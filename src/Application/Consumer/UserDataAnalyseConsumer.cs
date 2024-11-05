using System.Net;
using Application.Common.Interfaces.KafkaInterface;
using Application.Common.Kafka;
using Application.Constants;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using SharedProject.Models;

namespace Application.Consumer;

public class UserDataAnalyseConsumer : KafkaConsumerBase<UserDataAnalyseModel>
{
    public UserDataAnalyseConsumer(IConfiguration configuration, ILogger<UserDataAnalyseConsumer> logger, IServiceProvider serviceProvider)
        : base(configuration, logger, serviceProvider, TopicKafkaConstaints.RecommendOnboarding, "user_data_analyze_group")
    {
    }

    protected override async Task ProcessMessage(string message, IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AnalyseDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<UserDataAnalyseConsumer>>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var producer = serviceProvider.GetRequiredService<IProducerService>();
        var userModel = JsonConvert.DeserializeObject<UserDataAnalyseModel>(message);
        UserAnalyseEntity entity = mapper.Map<UserAnalyseEntity>(userModel);
        try
        {
            // Check if an entity with the same UserId already exists
            var existingEntity = await context.UserAnalyseEntity
                .Find(e => e.UserId.Equals(entity.UserId))
                .FirstOrDefaultAsync();
            
            if (existingEntity is null && userModel!.Address is not null && userModel.TypeExam is not null)
            {
                string mongoId = ObjectId.GenerateNewId().ToString();
                RecommendedData recommendedData = new RecommendedData()
                {
                    Id = mongoId,
                    UserId = entity.UserId,
                    SubjectIds = entity.Subjects,
                    Grade = entity.Grade,
                    TypeExam = entity.TypeExam
                };
                UserAnalyseEntity userDataEntity = new UserAnalyseEntity()
                {
                    Id = mongoId,
                    Address = entity.Address,
                    Grade = entity.Grade,
                    UserId = entity.UserId,
                    SchoolName = entity.SchoolName,
                    Major = entity.Major,
                    TypeExam = entity.TypeExam,
                    Subjects = entity.Subjects
                };
                await producer.ProduceObjectWithKeyAsync(TopicKafkaConstaints.DataRecommended, entity.UserId.ToString(), recommendedData);
                await context.UserAnalyseEntity.InsertOneAsync(userDataEntity);
            }
            if (existingEntity is not null && userModel!.Address is not null && userModel.TypeExam is not null)
            {
                logger.LogInformation($"User with UserId {entity.UserId} already exists. Performing update...");
                existingEntity.Address = entity.Address;
                existingEntity.Grade = entity.Grade;
                existingEntity.UserId = entity.UserId;
                existingEntity.SchoolName = entity.SchoolName;
                existingEntity.Major = entity.Major;
                existingEntity.TypeExam = entity.TypeExam;
                existingEntity.Subjects = entity.Subjects;
                
                RecommendedData recommendedData = new RecommendedData()
                {
                    UserId = entity.UserId,
                    SubjectIds = entity.Subjects,
                    Grade = entity.Grade,
                    TypeExam = entity.TypeExam,
                    Id = existingEntity.Id
                };
                await producer.ProduceObjectWithKeyAsync(TopicKafkaConstaints.DataRecommended, entity.UserId.ToString(), recommendedData);
                await context.UserAnalyseEntity.ReplaceOneAsync(
                    e => e.UserId == existingEntity.UserId,
                    existingEntity
                );

                logger.LogInformation($"UserDataAnalyse entity updated: {existingEntity.Id}");
            }
        }
        catch (Exception ex)
        {
            await producer.ProduceObjectWithKeyAsync(TopicKafkaConstaints.RecommendOnboardingRetry, userModel.UserId.ToString(), userModel);
            logger.LogError(ex, "An error occurred while processing cache operations for key {ex}.", ex.Message);
        }
    }
}
