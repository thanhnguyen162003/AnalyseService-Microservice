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

public class UserRoadmapGenConsumer : KafkaConsumerBase<UserDataAnalyseModel>
{
    public UserRoadmapGenConsumer(IConfiguration configuration, ILogger<UserDataAnalyseConsumer> logger, IServiceProvider serviceProvider)
        : base(configuration, logger, serviceProvider, "recommend_onboarding", "user_data_analyze_roadmap_group")
    {
    }

    protected override async Task ProcessMessage(string message, IServiceProvider serviceProvider)
    {
        // var _redis = serviceProvider.GetRequiredService<IOrdinaryDistributedCache>();
        var context = serviceProvider.GetRequiredService<AnalyseDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<UserDataAnalyseConsumer>>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var producer = serviceProvider.GetRequiredService<IProducerService>();
        
        try
        {
            var userModel = JsonConvert.DeserializeObject<UserDataAnalyseModel>(message);
            UserAnalyseEntity entity = mapper.Map<UserAnalyseEntity>(userModel);
            //base on user subjectIds, find all the roadmapSubjectIds that most match with user subjectIds
            //then, send back to user 1
            string mongoId = ObjectId.GenerateNewId().ToString();
            List<Guid> subjectIds = entity.Subjects;
            if (subjectIds.Count >= 3)
            {
                var roadmaps = await context.Roadmap
                    .Find(_ => true)
                    .ToListAsync();

                var matchingRoadmaps = roadmaps
                    .Select(roadmap => new
                    {
                        Roadmap = roadmap,
                        //intersect 2 list to get the number of matching subjectIds
                        MatchingSubjects = roadmap.RoadmapSubjectIds.Intersect(subjectIds).Count()
                    })
                    .OrderByDescending(x => x.MatchingSubjects)
                    .Take(4)
                    .Select(x => x.Roadmap)
                    .ToList();
                //send back to user 1
                var random = new Random();
                var selectedRoadmap = matchingRoadmaps[random.Next(matchingRoadmaps.Count)];
                await producer.ProduceObjectWithKeyAsync(TopicKafkaConstaints.UserRoadmapGenCreated, entity.UserId.ToString(), selectedRoadmap);
            }
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing cache operations for key {ex}.", ex.Message);
        }
    }
}
