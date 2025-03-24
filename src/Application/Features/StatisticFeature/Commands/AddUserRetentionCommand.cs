using System.Net;
using Application.Common.Models.RoadmapDataModel;
using Application.Common.Models.SearchModel;
using Application.Common.Models.StatisticModel;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using static Application.UserServiceRpc;

namespace Application.Features.StatisticFeature.Commands;

public record AddUserRetentionCommand : IRequest<ResponseModel>
{
}

public class AddUserRetentionCommanddHandler(
    IMapper mapper,
    AnalyseDbContext dbContext,
     UserServiceRpc.UserServiceRpcClient userServiceRpcClient,
    ILogger<AddUserActivityCommandHandler> logger)
    : IRequestHandler<AddUserRetentionCommand, ResponseModel>
{
    public async Task<ResponseModel> Handle(AddUserRetentionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await userServiceRpcClient.GetUserLoginCountAsync(new UserLoginCountRequest());

            var result = response.Retention
                .Select(x =>
                {
                    if (!Guid.TryParse(x.UserId, out var userId))
                    {
                        Console.WriteLine($"Invalid UserId: {x.UserId}");
                        return null; // Skip invalid UserId
                    }

                    if (!DateTime.TryParse(x.Date, out var loginDate))
                    {
                        Console.WriteLine($"Invalid Date format: {x.Date}");
                        return null; // Skip invalid Date
                    }
                    if (!int.TryParse(x.RoleId, out var roleId))
                    {
                        Console.WriteLine($"Invalid RoleId: {x.RoleId}");
                        return null; // Skip invalid Date
                    }
                    var check = userId;
                    return new UserRetentionRequestModel
                    {
                        UserId = userId,
                        LoginDate = loginDate,
                        RoleId = roleId
                    };
                })
                .Where(x => x != null) // Remove null values caused by invalid data
                .ToList();
            var userIds = result.Select(x => x.UserId).ToList();

            var filter = Builders<UserRetentionModel>.Filter.In(ur => ur.UserId, userIds);

            var userRetention = await dbContext.UserRetentionModel
                .Find(filter)
                .ToListAsync();

            // Step 2: Convert existing users to Dictionary for quick lookup
            var existingUserDict = userRetention.ToDictionary(ur => ur.UserId);

            // Step 3: Prepare bulk operations
            var bulkOps = new List<WriteModel<UserRetentionModel>>();

            foreach (var user in result)
            {
                if (existingUserDict.TryGetValue(user.UserId, out var existingUser))
                {
                    if (!existingUser.LoginDate.Any(d => d.Date == user.LoginDate.Date))
                    {
                        existingUser.LoginDate.Add(user.LoginDate);
                        existingUser.LoginDate = existingUser.LoginDate.OrderBy(d => d).Distinct().ToList();

                        int currentStreak = CalculateCurrentStreak(existingUser.LoginDate);
                        int maxStreak = CalculateMaxStreak(existingUser.LoginDate);

                        var update = Builders<UserRetentionModel>.Update
                            .AddToSet(ur => ur.LoginDate, user.LoginDate)
                            .Set(ur => ur.CurrentStreak, currentStreak)
                            .Set(ur => ur.MaxStreak, Math.Max(existingUser.MaxStreak, maxStreak));

                        var updateOneModel = new UpdateOneModel<UserRetentionModel>(
                            Builders<UserRetentionModel>.Filter.Eq(ur => ur.UserId, user.UserId),
                            update
                        );

                        bulkOps.Add(updateOneModel);
                    }
                }
                else
                {
                    var newUser = new UserRetentionModel
                    {
                        UserId = user.UserId,
                        LoginDate = new List<DateTime> { user.LoginDate },
                        RoleId = user.RoleId,
                        CurrentStreak = 1,
                        MaxStreak = 1
                    };

                    var insertOneModel = new InsertOneModel<UserRetentionModel>(newUser);
                    bulkOps.Add(insertOneModel);
                }
            }

            if (bulkOps.Count > 0)
            {
                await dbContext.UserRetentionModel.BulkWriteAsync(bulkOps);
            }

            return new ResponseModel(HttpStatusCode.Created, "thành công");
        }
        catch (Exception e)
        {
            return new ResponseModel(HttpStatusCode.BadRequest, e.Message);
        }
    }

    private int CalculateCurrentStreak(List<DateTime> sortedLogins)
    {
        int streak = 0;
        DateTime today = DateTime.Today;

        for (int i = sortedLogins.Count - 1; i >= 0; i--)
        {
            if ((today - sortedLogins[i]).TotalDays == streak)
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

        for (int i = 1; i < sortedLogins.Count; i++)
        {
            if ((sortedLogins[i] - sortedLogins[i - 1]).TotalDays == 1)
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
