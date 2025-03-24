using Algolia.Search.Models.Abtesting;
using Application.Common.Ultils;
using Application.Features.AnalyseFeature.Queries;
using Application.Features.StatisticFeature.Commands;
using Application.Features.StatisticFeature.Queries;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace Application.Endpoints;

public class StatisticEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1");
        //group.MapGet("test", Test).WithName(nameof(Test));
        group.MapGet("userActivity", GetUserActivity).RequireAuthorization().WithName(nameof(GetUserActivity));
        group.MapGet("userRetention", GetUserRetention).RequireAuthorization().WithName(nameof(GetUserRetention));
        //group.MapPost("testAdd", AddTest).WithName(nameof(AddTest));
        //group.MapPost("userRetention", AddUserRetention).WithName(nameof(AddUserRetention));
    }

    //public static async Task<IResult> Test(string Type, int Amount, bool IsCount, ISender sender, CancellationToken cancellationToken)
    //{
    //    var command = new GetUserActivityCommandFromGRPC()
    //    {
    //        Amount = Amount,
    //        IsCountFrom = IsCount,
    //        UserActivityType = Type
    //    };
    //    var result = await sender.Send(command, cancellationToken);
    //    return JsonHelper.Json(result);
    //}
    //public static async Task<IResult> AddTest(string Type, int Amount, bool IsCount, ISender sender, CancellationToken cancellationToken)
    //{
    //    var command = new AddUserActivityCommand()
    //    {
    //        Amount = Amount,
    //        IsCountFrom = IsCount,
    //        UserActivityType = Type
    //    };
    //    var result = await sender.Send(command, cancellationToken);
    //    return JsonHelper.Json(result);
    //}
    //public static async Task<IResult> AddUserRetention(ISender sender, CancellationToken cancellationToken)
    //{
    //    var command = new AddUserRetentionCommand()
    //    {
    //    };
    //    var result = await sender.Send(command, cancellationToken);
    //    return JsonHelper.Json(result);
    //}
    public static async Task<IResult> GetUserActivity(string Type, int Amount, bool IsCount, ISender sender, CancellationToken cancellationToken)
    {
        var command = new GetUserActivityCommand()
        {
            Amount = Amount,
            IsCountFrom = IsCount,
            UserActivityType = Type
        };
        var result = await sender.Send(command, cancellationToken);
        return JsonHelper.Json(result);
    }
    public static async Task<IResult> GetUserRetention(string Type, ISender sender, CancellationToken cancellationToken)
    {
        var command = new GetUserRetentionCommand()
        {
            Type = Type
        };
        var result = await sender.Send(command, cancellationToken);
        return JsonHelper.Json(result);
    }


}
