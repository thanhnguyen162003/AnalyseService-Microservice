using Application.Common.Models.SearchModel;

namespace Application.Common.Models;

public class SearchResponseModel
{
    public IEnumerable<Object> Flashcards { get; set; } = new HashSet<Object>();
    public IEnumerable<Object> Subjects { get; set; } = new HashSet<Object>();
    public IEnumerable<Object> Documents { get; set; } = new HashSet<Object>();
}
