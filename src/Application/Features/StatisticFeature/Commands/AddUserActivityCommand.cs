using System.Net;
using Application.Common.Models.RoadmapDataModel;
using Application.Common.Models.SearchModel;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Bson;
using static Application.UserServiceRpc;

namespace Application.Features.StatisticFeature.Commands;

public record AddUserActivityCommand : IRequest<ResponseModel>
{
    public int Amount { get; set; }
    public string UserActivityType { get; set; }
    public bool IsCountFrom { get; set; }
}

public class AddUserActivityCommandHandler(
    IMapper mapper,
    AnalyseDbContext dbContext,
     UserServiceRpc.UserServiceRpcClient userServiceRpcClient,
    ILogger<AddUserActivityCommandHandler> logger)
    : IRequestHandler<AddUserActivityCommand, ResponseModel>
{
    public async Task<ResponseModel> Handle(AddUserActivityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await userServiceRpcClient.GetCountUserAsync(new UserCountRequest()
            {
                Amount = request.Amount,
                IsCount = request.IsCountFrom,
                Type = request.UserActivityType
            });

            var result = response.Activities.Select(x => new UserActivityModel
            {
                Date = DateTime.Parse(x.Date),
                Moderators = x.Moderators,
                Students = x.Students,
                Teachers = x.Teachers

            }).ToList();
            await dbContext.UserActivityModel.InsertManyAsync(result);
            return new ResponseModel(HttpStatusCode.Created, "thành công");
        }
        catch (Exception e)
        {
            return new ResponseModel(HttpStatusCode.BadRequest, e.Message);
        }
    }
}
