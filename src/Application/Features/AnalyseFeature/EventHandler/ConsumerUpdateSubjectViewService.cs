using Application.Common.Interfaces.KafkaInterface;
using Application.Common.Kafka;
using Application.Constants;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedProject.Models;

namespace Application.Features.SubjectFeature.EventHandler;

public class ConsumerAnalyseService : KafkaConsumerAnalyseMethod
{
    private readonly ILogger<ConsumerAnalyseService> _logger;
    private readonly IProducerService _producerService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;
    private readonly AnalyseDbContext _dbContext;
    private readonly IKafkaConsumerMethod _consumerMethod;

    public ConsumerAnalyseService(IConfiguration configuration, ILogger<ConsumerAnalyseService> logger, IServiceProvider serviceProvider)
        : base(configuration, logger, serviceProvider, TopicKafkaConstaints.UserAnalyseData, "analyse_consumer_group")
    {
    }

    protected override async Task ProcessMessage(List<AnalyseDataDocumentModel> message, IServiceProvider serviceProvider)
    {
        if (message is not null)
        {
            List<UserAnalyseEntity> userData = await _dbContext.UserAnalyseEntity
            .Find(Builders<UserAnalyseEntity>.Filter.Empty)
            .ToListAsync();
            if (userData.Any())
            {
                List<RecommendedData> recommendedDatas = new List<RecommendedData>();

                // Process data
                foreach (UserAnalyseEntity user in userData)
                {
                    if (message is not null)
                    {
                        var topSubjectIds = message
                            .Where(d => d.SubjectId.HasValue && d.UserId.ToString() == user.Id)
                            .GroupBy(d => d.SubjectId!.Value)
                            .Select(group => new { SubjectId = group.Key })
                            .Take(4)
                            .Select(x => x.SubjectId)
                            .ToList();
                        topSubjectIds.AddRange(user.Subjects);

                        var topDocumentIds = message
                            .Where(d => d.DocumentId.HasValue && d.UserId.ToString() == user.Id)
                            .GroupBy(d => d.DocumentId!.Value)
                            .Select(group => new { DocumentId = group.Key, Count = group.Count() })
                            .OrderByDescending(x => x.Count)
                            .Take(8)
                            .Select(x => x.DocumentId)
                            .ToList();

                        var topFlashcardIds = message
                            .Where(d => d.FlashcardId.HasValue && d.UserId.ToString() == user.Id)
                            .GroupBy(d => d.FlashcardId!.Value)
                            .Select(group => new { FlashcardId = group.Key, Count = group.Count() })
                            .OrderByDescending(x => x.Count)
                            .Take(8)
                            .Select(x => x.FlashcardId)
                            .ToList();

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
                        recommendedDatas.Add(recommendedData);
                    }
                }
            }
        }
    }
}
//_logger.LogError("sdkjghflksjdfhglkjsdhjfgl;jsdl;kfgjl;ksdjfg;l before" + subject.View);
//                    subject.View += data.Count();
//                    var update = unitOfWork.SubjectRepository.Update(subject);
//var result = await unitOfWork.SaveChangesAsync();
//                    if (result > 0)
//                    {
//                        _logger.LogError($"Update Successfully!!!", subject.Id);
//                    }
//                    _logger.LogError($"Update Fail!!!");
//_logger.LogError("sdkjghflksjdfhglkjsdhjfgl;jsdl;kfgjl;ksdjfg;l after" + subject.View);
