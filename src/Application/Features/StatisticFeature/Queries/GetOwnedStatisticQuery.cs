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
            var flashcardLearning = await dbContext.UserFlashcardLearningModel.Find(x => x.UserId == userId).ToListAsync();
            var response = new OwnedStatistic()
            {
                CurrentLoginStreak = retention.CurrentStreak,
                LongestLoginStreak = retention.MaxStreak,
                TotalFlashcardLearned = flashcardLearning.Select(x => x.FlashcardId).Distinct().Count(),
                TotalFlashcardContentLearned = flashcardLearning.Select(x => x.FlashcardContentId).Distinct().Count(),
                CurrentLearnStreak = CalculateCurrentStreak(flashcardLearning.SelectMany(x => x.LearningDates).ToList()),
                LongestLearnStreak = CalculateMaxStreak(flashcardLearning.SelectMany(x => x.LearningDates).ToList()),
                TotalFlashcardContentHours = flashcardLearning.SelectMany(x => x.TimeSpentHistory).Sum(),
                TotalFlashcardLearnDates = CalculateDateAmount(flashcardLearning.SelectMany(x => x.LearningDates).ToList())
            };
            return response;

        }
        private int CalculateDateAmount(List<DateTime> sortedLogins)
        {
            int totalUniqueDays = sortedLogins.Select(x => x.Date).Distinct().Count();

            return totalUniqueDays;
        }
        private int CalculateCurrentStreak(List<DateTime> sortedLogins)
        {
            int streak = 0;
            DateTime today = DateTime.Today;
            var uniqueDates = sortedLogins.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();

            for (int i = uniqueDates.Count - 1; i >= 0; i--)
            {
                if ((today.Date - uniqueDates[i].Date).TotalDays == streak)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private int CalculateMaxStreak(List<DateTime> sortedLogins)
        {
            if (sortedLogins == null || sortedLogins.Count == 0) return 0;

            int maxStreak = 1, currentStreak = 1;
            var uniqueDates = sortedLogins.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();
            for (int i = 1; i < uniqueDates.Count; i++)
            {
                if ((uniqueDates[i].Date - uniqueDates[i - 1].Date).TotalDays == 1)
                {
                    currentStreak++;
                }
                else
                {
                    maxStreak = Math.Max(maxStreak, currentStreak);
                    currentStreak = 1; // Reset streak
                }
            }

            return Math.Max(maxStreak, currentStreak);
        }
    }

}
