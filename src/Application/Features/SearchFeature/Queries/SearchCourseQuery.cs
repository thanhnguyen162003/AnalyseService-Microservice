using Application.Common.Models.SearchModel;
using Application.Services.Search;
using Domain.CustomModel;
using Domain.Enums;
using Domain.QueriesFilter;

namespace Application.Features.SearchFeature.Queries;

public class SearchCourseQuery : IRequest<IEnumerable<CourseSearchResponseModel>>
{
    public SearchCourseType Type { get; set; }
    public string Value { get; set; } = null!;
    public int Limit { get; set; }
}

public class SearchCourseQueryHandler : IRequestHandler<SearchCourseQuery, IEnumerable<CourseSearchResponseModel>>
{
    private readonly ISearchService _searchService;

    public SearchCourseQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<IEnumerable<CourseSearchResponseModel>> Handle(SearchCourseQuery request, CancellationToken cancellationToken)
    {
        return await _searchService.SearchCourseName(request.Type, request.Value, request.Limit);
    }

}
