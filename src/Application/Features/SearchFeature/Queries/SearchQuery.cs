using Application.Common.Models;
using Application.Common.Models.SearchModel;
using Application.Services.Search;
using Domain.Enums;

namespace Application.Features.SearchFeature.Queries;

public record SearchQuery : IRequest<object>
{
    public SearchType Type { get; set; }
    public string? Value { get; set; }
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
        request.Value = request.Value ?? string.Empty;
        if (request.Type == SearchType.All)
        {
            // Search all returns a common response
            return await _searchService.SearchAll(request.Value);
        }
        else if (request.Type == SearchType.Flashcard)
        {
            // Return specific type for flashcard
            IEnumerable<FlashcardResponseModel> flashcardResults = await _searchService.SearchFlashCard(request.Value);
            return flashcardResults;
        }
        else if (request.Type == SearchType.Subject)
        {
            // Return specific type for subject
            IEnumerable<SubjectResponseModel> subjectResults = await _searchService.SearchSubject(request.Value);
            return subjectResults;
        }
        else if (request.Type == SearchType.Document)
        {
            // Return specific type for document
            IEnumerable<DocumentResponseModel> documentResults = await _searchService.SearchDocument(request.Value);
            return documentResults;
        } else if (request.Type == SearchType.name)
        {
            // Return specific type for name
            IEnumerable<string> nameResults = await _searchService.SearchName(request.Value);
            return nameResults;
        }

        return new List<object>();
    }
}
