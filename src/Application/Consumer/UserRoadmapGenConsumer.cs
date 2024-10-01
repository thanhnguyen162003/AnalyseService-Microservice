// using Application.Common.Interfaces.KafkaInterface;
// using Application.Common.Kafka;
// using Application.Constants;
// using Domain.Entities;
// using Infrastructure.Data;
// using MongoDB.Bson;
// using MongoDB.Driver;
// using Newtonsoft.Json;
// using SharedProject.Models;
//
// namespace Application.Consumer;
//
// public class UserRoadmapGenConsumer : KafkaConsumerBase<UserDataAnalyseModel>
// {
//     public UserRoadmapGenConsumer(IConfiguration configuration, ILogger<UserDataAnalyseConsumer> logger, IServiceProvider serviceProvider)
//         : base(configuration, logger, serviceProvider, "recommend_onboarding", "user_data_analyze_roadmap_group")
//     {
//     }
//
//     protected override async Task ProcessMessage(string message, IServiceProvider serviceProvider)
//     {
//         // var _redis = serviceProvider.GetRequiredService<IOrdinaryDistributedCache>();
//         var context = serviceProvider.GetRequiredService<AnalyseDbContext>();
//         var logger = serviceProvider.GetRequiredService<ILogger<UserDataAnalyseConsumer>>();
//         var mapper = serviceProvider.GetRequiredService<IMapper>();
//         var producer = serviceProvider.GetRequiredService<IProducerService>();
//         
//         try
//         {
//             var userModel = JsonConvert.DeserializeObject<UserDataAnalyseModel>(message);
//             UserAnalyseEntity entity = mapper.Map<UserAnalyseEntity>(userModel);
//             // Check if an entity with the same UserId already exists
//             var existingEntity = await context.UserAnalyseEntity
//                 .Find(e => e.UserId.Equals(entity.UserId))
//                 .FirstOrDefaultAsync();
//             string mongoId = ObjectId.GenerateNewId().ToString();
//             List<Guid> subjectIds = entity.Subjects;
//             if (subjectIds.Count >= 3) // must be 3 subject at least to generate roadmap
//             {
//                 
//             }
//             
//             // // if (existingEntity is null)
//             // // {
//             // //     string mongoId = ObjectId.GenerateNewId().ToString();
//             // //     List<Guid> subjectIds = entity.Subjects;
//             // //     
//             // // }
//             // if (existingEntity is not null && userModel.Address is not null && userModel.TypeExam is not null)
//             // {
//             //     //no need re gen roadmap
//             //     logger.LogInformation($"User {userModel.UserId} is already examined");
//             // }
//         }
//         catch (Exception ex)
//         {
//             logger.LogError(ex, "An error occurred while processing cache operations for key {ex}.", ex.Message);
//         }
//     }
// }
