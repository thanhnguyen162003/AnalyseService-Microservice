using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using Application.Common.Models;
using Application.Common.Models.SearchModel;
using Application.Constants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Services.Search;

public class SearchService : ISearchService
{
    private readonly SearchClient _client;
    private readonly IConfiguration _configuration;

    public SearchService(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new SearchClient(_configuration["AlgoliaSetting:ApplicationId"], _configuration["AlgoliaSetting:SearchApiKey"]);
    }

    public async Task<SearchResponseModel> SearchAll(string value)
    {

        // Search
        var result = await _client.SearchAsync<Object>(
            new SearchMethodParams
            {
                Requests = new List<SearchQuery>
                {
                   new SearchQuery
                   (
                        new SearchForHits
                        {
                           IndexName = IndexSearchConstant.Flashcard,
                           Query = value
                        }
                   ),
                   new SearchQuery
                   (
                        new SearchForHits
                        {
                           IndexName = IndexSearchConstant.Subject,
                           Query = value
                        }
                   ),
                   new SearchQuery
                   (
                        new SearchForHits
                        {
                           IndexName = IndexSearchConstant.Document,
                           Query = value
                        }
                   ),
                }
            }
        );

        return new SearchResponseModel()
        {
            Flashcards = result.Results.ElementAt(0).AsSearchResponse().Hits,
            Subjects = result.Results.ElementAt(1).AsSearchResponse().Hits,
            Documents =  result.Results.ElementAt(2).AsSearchResponse().Hits,
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
