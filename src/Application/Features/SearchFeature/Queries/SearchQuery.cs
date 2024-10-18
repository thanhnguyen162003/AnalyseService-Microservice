using Application.Common.Models;
using Application.Common.Models.SearchModel;
using Application.Services.Search;

namespace Application.Features.SearchFeature.Queries;

public record SearchQuery : IRequest<object>
{
    public string? type { get; init; }
    public string Value { get; init; } = "";
}

public class SearchQueryHandler : IRequestHandler<SearchQuery, object>
{
    private readonly ISearchService _searchService;

    public SearchQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<object> Handle(SearchQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.type))
            return await _searchService.SearchAll(request.Value);
        else if (request.type == "flashcard")
            return await _searchService.SearchFlashCard(request.Value);
        else if (request.type == "subject")
            return await _searchService.SearchSubject(request.Value);
        else if (request.type == "document")
            return await _searchService.SearchDocument(request.Value);
        else
            throw new Exception("Type not found");
    }
}
