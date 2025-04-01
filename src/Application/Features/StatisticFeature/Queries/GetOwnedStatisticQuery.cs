using System.Collections.Generic;
using System.Threading;
using Amazon.Runtime.Internal.Transform;
using Application.Common.Interfaces.ClaimInterface;
using Application.Common.Models.StatisticModel;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Application.UserServiceRpc;

namespace Application.Features.StatisticFeature.Queries
{
    public class GetOwnedStatisticQuery : IRequest<OwnedStatistic>
    {
    }

    public class GetOwnedStatisticQueryHandler(AnalyseDbContext dbContext, IMapper _mapper, UserServiceRpc.UserServiceRpcClient userServiceRpcClient, IClaimInterface claimInterface) : IRequestHandler<GetOwnedStatisticQuery, OwnedStatistic>
    {
        public async Task<OwnedStatistic> Handle(GetOwnedStatisticQuery request, CancellationToken cancellationToken)
        {
            var userId = claimInterface.GetCurrentUserId;
            var retention = await dbContext.UserRetentionModel.Find(x => x.UserId == userId).SingleOrDefaultAsync();
            var flashcardLearning = await dbContext.UserFlashcardLearningModel.Find(x => x.UserId == userId).Project(x => new { x.FlashcardId, x.FlashcardContentId }).ToListAsync();

            var response = new OwnedStatistic()
            {
                CurrentStreak = retention.CurrentStreak,
                LongestStreak = retention.MaxStreak,
                TotalFlashcard = flashcardLearning.Select(x => x.FlashcardId).Distinct().Count(),
                TotalFlashcardContent = flashcardLearning.Select(x => x.FlashcardContentId).Distinct().Count()
            };
            return response;

        }
    }
}
