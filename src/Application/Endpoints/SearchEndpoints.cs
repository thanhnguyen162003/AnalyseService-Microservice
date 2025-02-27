using Application.Common.Models;
using Application.Common.Ultils;
using Application.Features.SearchFeature.Queries;
using Application.Services.Search;
using Carter;
using Domain.CustomModel;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Application.Endpoints;

public class SearchEndpoints : ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1");
        group.MapGet("search", SearchFlashCard).WithName(nameof(SearchFlashCard));
    }

    private static async Task<IResult> SearchFlashCard([AsParameters]SearchQuery searchQuery, ISender sender, CancellationToken cancellationToken, HttpContext context)
    {
        var result = await sender.Send(searchQuery, cancellationToken);

        if (searchQuery.Type == SearchType.All)
        {
            return JsonHelper.Json(result);
        } else
        {
            var title = searchQuery.Type switch
            {
                SearchType.Flashcard => "X-Flashcards-Pagination",
                SearchType.Subject => "X-Subjects-Pagination",
                SearchType.Document => "X-Documents-Pagination",
                SearchType.News => "X-Tips-Pagination",
                SearchType.Folder => "X-Folders-Pagination",
                SearchType.Name => "X-Names-Pagination",
                _ => throw new ArgumentOutOfRangeException()
            };

            context.Response.Headers.Append(title, JsonConvert.SerializeObject(new Metadata()
            {
                CurrentPage = searchQuery.PageNumber + 1,
                PageSize = searchQuery.PageSize,
                TotalPages = (int)result.GetType().GetProperty("TotalPages")?.GetValue(result)!,
                TotalCount = (int)result.GetType().GetProperty("TotalCount")?.GetValue(result)!
            }));

            return JsonHelper.Json(result);
        }

        
    }
}
