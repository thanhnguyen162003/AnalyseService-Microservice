using Application.Common.Models;
using Application.Common.Ultils;
using Application.Features.RoadmapFeature.Commands;
using Carter;
using Microsoft.AspNetCore.Mvc;

namespace Application.Endpoints;

public class RoadmapEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1");
        group.MapPost("roadmap/section",CreateRoadmapSection).RequireAuthorization("moderatorPolicy").WithName(nameof(CreateRoadmapSection));
    }

    public static async Task<IResult> CreateRoadmapSection([FromBody] RoadMapSectionCreateRequestModel roadMapSectionCreateRequestModel, ISender sender,
        CancellationToken cancellationToken, ValidationHelper<RoadMapSectionCreateRequestModel> validator)
    {
        var (isValid, response) = await validator.ValidateAsync(roadMapSectionCreateRequestModel);
        if (!isValid)
        {
            return Results.BadRequest(response);
        }
        var command = new CreateRoadmapSectionCommand()
        {
            RoadMapSectionCreateCommand = roadMapSectionCreateRequestModel
        };
        var result = await sender.Send(command, cancellationToken);
        return JsonHelper.Json(result);
    }
}
