using Application.Common.Models;
using Application.Common.Models.FlashcardFolderModel;
using Application.Common.Models.NewsModel;
using Application.Common.Models.SearchModel;

namespace Application.Services.Search;

public interface ISearchService
{
    Task<IEnumerable<FlashcardResponseModel>> SearchFlashCard(string value);
    Task<IEnumerable<SubjectResponseModel>> SearchSubject(string value);
    Task<IEnumerable<DocumentResponseModel>> SearchDocument(string value);
    Task<SearchResponseModel> SearchAll(string value);
    Task<IEnumerable<string>> SearchName(string value);
    Task<IEnumerable<NewsPreviewResponseModel>> SearchTips(string value);
    Task<IEnumerable<FolderUserResponse>> SearchFolder(string value);
}
