using Application.Common.Kafka;
using Application.Constants;
using Infrastructure.Data;
using Newtonsoft.Json;
using SharedProject.Models;
using MongoDB.Driver;
using Domain.Entities;

namespace Application.Consumer;

public class RecentViewKafkaConsumer(
    IConfiguration configuration,
    ILogger<RecentViewKafkaConsumer> logger,
    IServiceProvider serviceProvider)
    : KafkaConsumerBase<RecentViewModel>(configuration, logger, serviceProvider,
        TopicKafkaConstaints.RecentViewCreated, "user_recent_view_group")
{
    protected override async Task ProcessMessage(string message, IServiceProvider serviceProvider)
    {
        int retryCount = 0;
        int maxRetries = 2; // Set max retry limit
        int delayBetweenRetriesMs = 2000; // Delay between retries in milliseconds

        var context = serviceProvider.GetRequiredService<AnalyseDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<RecentViewKafkaConsumer>>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var recentViewModel = JsonConvert.DeserializeObject<RecentViewModel>(message);
        var newEntity = mapper.Map<RecentView>(recentViewModel);

        while (retryCount < maxRetries)
        {
            try
            {
                // Check existing entries for the user
                var userRecentViews = context.RecentViews
                    .Find(rv => rv.UserId == newEntity.UserId)
                    .SortByDescending(rv => rv.Time)
                    .ToList();

                // Ensure idempotency (if the same document already exists, skip)
                if (userRecentViews.Any(rv => rv.IdDocument == newEntity.IdDocument))
                {
                    logger.LogInformation($"Document {newEntity.IdDocument} for UserId {newEntity.UserId} already exists. Skipping.");
                    return;
                }

                // Remove the oldest entry if the count exceeds 9
                if (userRecentViews.Count >= 10)
                {
                    var oldestEntry = userRecentViews.Last();
                    context.RecentViews.DeleteOne(rv => rv.Id == oldestEntry.Id);
                    logger.LogInformation($"Removed oldest entry for UserId {newEntity.UserId}: DocumentId {oldestEntry.IdDocument}");
                }

                // Add the new entry
                await context.RecentViews.InsertOneAsync(newEntity);

                logger.LogInformation($"Successfully saved recent view for UserId {newEntity.UserId}: DocumentId {newEntity.IdDocument}");

                // Exit the retry loop after success
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                logger.LogError(ex, $"Attempt {retryCount} failed while processing data for UserId {recentViewModel.UserId}. Retrying...");

                if (retryCount >= maxRetries)
                {
                    logger.LogError(ex, $"Maximum retries reached. Sending message to retry topic for UserId {recentViewModel.UserId}.");
                }
                else
                {
                    await Task.Delay(delayBetweenRetriesMs);
                }
            }
        }
    }
}
