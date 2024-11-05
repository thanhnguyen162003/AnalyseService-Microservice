using Application.Common.Interfaces.KafkaInterface;
using Application.Common.Kafka;
using Application.Constants;
using Application.Consumer;
using Application.KafkaMessageModel;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;
using Newtonsoft.Json;
using SharedProject.Models;

namespace Application.Services.MaintainService;

public class RoadmapMissedMaintainService : KafkaConsumerBase10Minutes<UserDataAnalyseModel>
{
    public RoadmapMissedMaintainService(
        IConfiguration configuration, ILogger<RoadmapMissedMaintainService> logger, IServiceProvider serviceProvider)
        : base(configuration, logger, serviceProvider, TopicKafkaConstaints.UserRecommnedRoadmapMissed, "user_data_analyze_roadmap_group")
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
                        MatchingSubjects = roadmap.RoadmapSubjectIds.Intersect(subjectIds).Count(),
                        MatchingTypeExam = roadmap.TypeExam.Intersect(
                        entity.TypeExam?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()).Count(),
                    })
                    .OrderByDescending(x => x.MatchingSubjects)
                    .ThenByDescending(x => x.MatchingTypeExam)
                    .Take(4)
                    .Select(x => x.Roadmap)
                    .ToList();
                //send back to user 1
                var random = new Random();
                var selectedRoadmap = matchingRoadmaps[random.Next(matchingRoadmaps.Count)];
                RoadmapUserKafkaMessageModel messageModel = new RoadmapUserKafkaMessageModel(){
                    RoadmapId = selectedRoadmap.Id,
                    RoadmapName = selectedRoadmap.RoadmapName,
                    RoadmapSubjectIds = selectedRoadmap.RoadmapSubjectIds,
                    RoadmapDescription = selectedRoadmap.RoadmapDescription,
                    TypeExam = selectedRoadmap.TypeExam,
                    ContentJson = selectedRoadmap.ContentJson,
                    RoadmapDocumentIds = selectedRoadmap.RoadmapDocumentIds,
                    UserId = entity.UserId
                };
                await producer.ProduceObjectWithKeyAsync(TopicKafkaConstaints.UserRoadmapGenCreated, entity.UserId.ToString(), messageModel);
            }
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing roadmap miss", ex.Message);
        }
    }
}
