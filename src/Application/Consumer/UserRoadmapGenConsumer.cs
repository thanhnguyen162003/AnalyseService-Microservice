using Application.Common.Interfaces.KafkaInterface;
using Application.Common.Kafka;
using Application.Constants;
using Application.KafkaMessageModel;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Driver;
using Newtonsoft.Json;
using SharedProject.Models;

namespace Application.Consumer;

public class UserRoadmapGenConsumer(
    IConfiguration configuration,
    ILogger<UserDataAnalyseConsumer> logger,
    IServiceProvider serviceProvider)
    : KafkaConsumerBase<UserDataAnalyseModel>(configuration, logger, serviceProvider,
        TopicKafkaConstaints.RecommendOnboarding, "user_data_analyze_roadmap_group")
{
    protected override async Task ProcessMessage(string message, IServiceProvider serviceProvider)
    {
        int retryCount = 0;
        const int maxRetries = 2;
        bool success = false;
        var context = serviceProvider.GetRequiredService<AnalyseDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<UserDataAnalyseConsumer>>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var producer = serviceProvider.GetRequiredService<IProducerService>();
        var userModel = JsonConvert.DeserializeObject<UserDataAnalyseModel>(message);
        UserAnalyseEntity entity = mapper.Map<UserAnalyseEntity>(userModel);

        while (retryCount <= maxRetries && !success)
        {
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
                            // Intersect two lists to get the number of matching subjectIds
                            MatchingSubjects = roadmap.RoadmapSubjectIds.Intersect(subjectIds).Count(),
                            MatchingTypeExam = roadmap.TypeExam.Intersect(
                            entity.TypeExam?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()).Count(),
                        })
                        .OrderByDescending(x => x.MatchingSubjects)
                        .ThenByDescending(x => x.MatchingTypeExam)
                        .Take(4)
                        .Select(x => x.Roadmap)
                        .ToList();
                    
                    if (matchingRoadmaps.Count > 0)
                    {
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
                    else
                    {
                        logger.LogWarning($"No matching roadmaps found for user {entity.UserId}.");
                        // Handle the case where no matching roadmaps are found
                    }
                    success = true;
                }
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.LogError(ex, $"An error occurred during processing. Attempt {retryCount} of {maxRetries + 1}.");
                
                // If we've reached max retries, produce to retry topic
                if (retryCount > maxRetries)
                {
                    await producer.ProduceObjectWithKeyAsync(TopicKafkaConstaints.RecommendOnboardingRetryRoadmapGen,
                        userModel.UserId.ToString(), userModel);
                    logger.LogError(ex, "Max retries reached. Message will be sent to retry topic. Error: {ex}.", ex.Message);
                }
                else
                {
                    await Task.Delay(2000);
                }
            }
        }
    }
}
