using System.Threading;
using Algolia.Search.Http;
using Algolia.Search.Models.Search;
using Application.Common.Interfaces.KafkaInterface;
using Application.Common.Kafka;
using Application.Constants;
using Confluent.Kafka;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedProject.Models;

namespace Application.Features.SubjectFeature.EventHandler;

public class ConsumerAnalyseService : KafkaConsumerAnalyseMethod
{
    private readonly SubjectServiceRpc.SubjectServiceRpcClient _client;
    public ConsumerAnalyseService(IConfiguration configuration, ILogger<ConsumerAnalyseService> logger, IServiceProvider serviceProvider, SubjectServiceRpc.SubjectServiceRpcClient client)
        : base(configuration, logger, serviceProvider, TopicKafkaConstaints.UserAnalyseData, "analyse_consumer_group")
    {
        _client = client;
    }

    protected override async Task ProcessMessage(List<AnalyseDataDocumentModel> message, IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        var _sender = serviceProvider.GetRequiredService<ISender>();
        var _logger = serviceProvider.GetRequiredService<ILogger<ConsumerAnalyseService>>();
        var producer = serviceProvider.GetRequiredService<IProducerService>();
        var _dbContext = serviceProvider.GetRequiredService<AnalyseDbContext>();
        if (message is not null)
        {
            List<UserAnalyseEntity> userData = await _dbContext.UserAnalyseEntity
            .Find(Builders<UserAnalyseEntity>.Filter.Empty)
            .ToListAsync();
            if (userData.Any())
            {
                List<RecommendedData> recommendedDatasInsert = new List<RecommendedData>();
                List<RecommendedData> recommendedDatasUpdate = new List<RecommendedData>();
                var subjectIds = message
                            .Where(d => d.SubjectId.HasValue)
                            .GroupBy(d => d.SubjectId!.Value)
                            .Select(group => new { SubjectId = group.Key })
                            .Select(x => x.SubjectId.ToString())
                            .AsEnumerable();
                var userIds = userData
                            .Select(x => x.UserId.ToString())
                            .AsEnumerable();
                SubjectGradeRequest grade = new() 
                { 
                    SubjectId = subjectIds.ToString()
                };
                var subjectGradeResponse = await _client.GetSubjectGradeAsync(grade);

                SubjectEnrollCheckRequest subjectEnrollCheckRequest = new() 
                {
                    SubjectId = subjectIds.ToString(),
                    UserId = userIds.ToString()
                };
                var subjectEnrollResponse = await _client.GetSubjectEnrollAsync(subjectEnrollCheckRequest);
                // Process 
                foreach (UserAnalyseEntity user in userData)
                {
                    if (message is not null)
                    {
                        var topSubjectIds = message
                            .Where(d => d.SubjectId.HasValue && d.UserId == user.UserId)
                            .GroupBy(d => d.SubjectId!.Value)
                            .Select(group => new { SubjectId = group.Key })
                            .Take(4)
                            .Select(x => x.SubjectId)
                            .ToList();
                       

                        var topDocumentIds = message
                            .Where(d => d.DocumentId.HasValue && d.UserId == user.UserId)
                            .GroupBy(d => d.DocumentId!.Value)
                            .Select(group => new { DocumentId = group.Key, Count = group.Count() })
                            .OrderByDescending(x => x.Count)
                            .Take(8)
                            .Select(x => x.DocumentId)
                            .ToList();

                        var topFlashcardIds = message
                            .Where(d => d.FlashcardId.HasValue && d.UserId == user.UserId)
                            .GroupBy(d => d.FlashcardId!.Value)
                            .Select(group => new { FlashcardId = group.Key, Count = group.Count() })
                            .OrderByDescending(x => x.Count)
                            .Take(8)
                            .Select(x => x.FlashcardId)
                            .ToList();
                        if (topSubjectIds.IsNullOrEmpty()
                            && topDocumentIds.IsNullOrEmpty()
                            && topFlashcardIds.IsNullOrEmpty())
                        {
                            continue;
                        }
                        else
                        {

                            var test = await _dbContext.RecommendedData.Find(x => x.UserId == user.UserId).FirstOrDefaultAsync();
                            if (test != null)
                            {
                                var recommendedData = new RecommendedData
                                {
                                    Id = test.Id,
                                    UserId = user.UserId,
                                    SubjectIds = topSubjectIds,
                                    DocumentIds = topDocumentIds,
                                    FlashcardIds = topFlashcardIds,
                                    Grade = user.Grade,
                                    TypeExam = user.TypeExam
                                };
                                recommendedDatasUpdate.Add(recommendedData);
                            }
                            else
                            {
                                // Create the recommended data object
                                var recommendedData = new RecommendedData
                                {
                                    Id = ObjectId.GenerateNewId().ToString(),
                                    UserId = user.UserId,
                                    SubjectIds = topSubjectIds,
                                    DocumentIds = topDocumentIds,
                                    FlashcardIds = topFlashcardIds,
                                    Grade = user.Grade,
                                    TypeExam = user.TypeExam
                                };
                                // Add to the batch list
                                recommendedDatasInsert.Add(recommendedData);
                            }
                        }
                    }
                }
                // Send batch messages to Kafka
                if (recommendedDatasInsert.Any())
                {
                    foreach (var recommendedData in recommendedDatasInsert)
                    {
                        // Send each recommended data in the batch
                        await producer.ProduceObjectWithKeyAsyncBatch(TopicKafkaConstaints.DataRecommended, recommendedData.UserId.ToString(), recommendedData);
                    }
                    await producer.FlushedData(TimeSpan.FromSeconds(10));
                    await _dbContext.RecommendedData.InsertManyAsync(recommendedDatasInsert, cancellationToken: stoppingToken);
                }
                if (recommendedDatasUpdate.Any())
                {
                    foreach (var recommendedData in recommendedDatasUpdate)
                    {
                        // Send each recommended data in the batch
                        var filter = Builders<RecommendedData>.Filter.Eq(n => n.Id, recommendedData.Id);
                        await _dbContext.RecommendedData.FindOneAndReplaceAsync(filter, recommendedData, cancellationToken: stoppingToken);
                        
                    } 
                }
            }
            _logger.LogInformation("Processing complete");
        }
    }
}
