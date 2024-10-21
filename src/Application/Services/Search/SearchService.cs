using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using Application.Common.Models;
using Application.Common.Models.SearchModel;
using Application.Constants;
using Newtonsoft.Json;

namespace Application.Services.Search;

public class SearchService : ISearchService
{
    private readonly SearchClient _client;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public SearchService(IConfiguration configuration, IMapper mapper)
    {
        _configuration = configuration;
        _client = new SearchClient(_configuration["AlgoliaSetting:ApplicationId"], _configuration["AlgoliaSetting:SearchApiKey"]);
        _mapper = mapper;
    }

    public async Task<SearchResponseModel> SearchAll(string value)
    {

        // Search
        var result = await _client.SearchAsync<object>(
            new SearchMethodParams
            {
                Requests = new List<SearchQuery>
                {
                    new SearchQuery(new SearchForHits { IndexName = IndexSearchConstant.Flashcard, Query = value }),
                    new SearchQuery(new SearchForHits { IndexName = IndexSearchConstant.Subject, Query = value }),
                    new SearchQuery(new SearchForHits { IndexName = IndexSearchConstant.Document, Query = value }),
                }
            }
        );

        var flashcards = result.Results.ElementAt(0).AsSearchResponse().Hits;
        var subjects = result.Results.ElementAt(1).AsSearchResponse().Hits;
        var documents = result.Results.ElementAt(2).AsSearchResponse().Hits;

        return new SearchResponseModel
    {
        Flashcards = flashcards.Select(hit => 
            JsonConvert.DeserializeObject<FlashcardResponseModel>(hit.ToString())).Select(card => new FlashcardResponseModel
            {
                Id = card.Id,
                Like = card.Like,
                Slug = card.Slug,
                Star = card.Star,
                Status = card.Status,
                CreatedAt = card.CreatedAt,
                CreatedBy = card.CreatedBy,
                FlashcardDescription = card.FlashcardDescription,
                FlashcardName = card.FlashcardName,
                SubjectId = card.SubjectId,
                UpdatedAt = card.UpdatedAt,
                UpdatedBy = card.UpdatedBy,
                UserId = card.UserId,
                NumberOfFlashcardContent = flashcards.Count()
            }),
        Subjects = subjects.Select(hit => 
            JsonConvert.DeserializeObject<SubjectResponseModel>(hit.ToString())).Select(subject => new SubjectResponseModel
            {
                Id = subject.Id,
                Like = subject.Like,
                Slug = subject.Slug,
                CreatedAt = subject.CreatedAt,
                UpdatedAt = subject.UpdatedAt,
                Image = subject.Image,
                Information = subject.Information,
                View = subject.View,
                CategoryName = subject.CategoryName,
                NumberEnrollment = subject.NumberEnrollment,
                SubjectCode = subject.SubjectCode,
                SubjectDescription = subject.SubjectDescription,
                SubjectName = subject.SubjectName,
                NumberOfChapters = subject.NumberOfChapters
            }),
        Documents = documents.Select(hit => 
            JsonConvert.DeserializeObject<DocumentResponseModel>(hit.ToString())).Select(doc => new DocumentResponseModel
            {
                Id = doc.Id,
                Like = doc.Like,
                CreatedAt = doc.CreatedAt,
                UpdatedAt = doc.UpdatedAt,
                UpdatedBy = doc.UpdatedBy,
                CreatedBy = doc.CreatedBy,
                View = doc.View,
                Subject = doc.Subject,
                Download = doc.Download,
                Category = doc.Category,
                DocumentDescription = doc.DocumentDescription,
                DocumentName = doc.DocumentName,
                DocumentSlug = doc.DocumentSlug,
                DocumentYear = doc.DocumentYear
            }),
    };
}

    public async Task<IEnumerable<SubjectResponseModel>> SearchSubject(string value)
    {
        // Create a search query
        var searchQuery = new SearchQuery
        (
            new SearchForHits
            {
                IndexName = IndexSearchConstant.Flashcard,
                Query = value
            }
        );

        // Search
        var result = await _client.SearchAsync<SubjectResponseModel>(
            new SearchMethodParams
            {
                Requests = new List<SearchQuery>
                {
                        searchQuery
                }
            }
        );

        return result.Results.ElementAt(0).AsSearchResponse().Hits;
    }

    public async Task<IEnumerable<DocumentResponseModel>> SearchDocument(string value)
    {
        // Create a search query
        var searchQuery = new SearchQuery
        (
            new SearchForHits
            {
                IndexName = IndexSearchConstant.Document,
                Query = value
            }
        );

        // Search
        var result = await _client.SearchAsync<DocumentResponseModel>(
            new SearchMethodParams
            {
                Requests = new List<SearchQuery>
                {
                        searchQuery
                }
            }
        );

        return result.Results.ElementAt(0).AsSearchResponse().Hits;
    }

    public async Task<IEnumerable<FlashcardResponseModel>> SearchFlashCard(string value)
    {
        // Create a search query
        var searchQuery = new SearchQuery
        (
            new SearchForHits
            {
                IndexName = IndexSearchConstant.Flashcard,
                Query = value
            }
        );

        // Search
        var result = await _client.SearchAsync<FlashcardResponseModel>(
            new SearchMethodParams
            {
                Requests = new List<SearchQuery>
                {
                        searchQuery
                }
            }
        );

        return result.Results.ElementAt(0).AsSearchResponse().Hits;
    } 

}
