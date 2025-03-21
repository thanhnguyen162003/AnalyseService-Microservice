using Application.Common.Models.StatisticModel;
using Domain.Enums;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Application.Features.StatisticFeature.Queries
{
    public class GetUserActivityCommandFromGRPC : IRequest<List<UserActivityResponseModel>>
    {
        public int Amount { get; set; }
        public string UserActivityType { get; set; }
        public bool IsCountFrom { get; set; }
    }

    public class GetUserActivityCommandFromGRPCHandler(AnalyseDbContext dbContext, IMapper _mapper, UserServiceRpc.UserServiceRpcClient userServiceRpcClient) : IRequestHandler<GetUserActivityCommandFromGRPC, List<UserActivityResponseModel>>
    {
        public async Task<List<UserActivityResponseModel>> Handle(GetUserActivityCommandFromGRPC request, CancellationToken cancellationToken)
        {
            var response = await userServiceRpcClient.GetCountUserAsync(new UserCountRequest()
            { 
                Amount = request.Amount,
                IsCount = request.IsCountFrom, 
                Type= request.UserActivityType
            });
            var result = response.Activities.Select(x => new UserActivityResponseModel
            {
                Date = DateTime.Parse(x.Date),
                Moderators = x.Moderators,
                Students = x.Students,
                Teachers = x.Teachers

            }).ToList();
            return result;
        }
    }
}
