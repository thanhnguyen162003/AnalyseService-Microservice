namespace Application.Common.Models.SearchModel;

public class SubjectResponseModel
{
    public Guid Id { get; set; }

    public string? SubjectName { get; set; }

    public string? Image { get; set; }

    public string? Information { get; set; }

    // public string? Class { get; set; } 

    public string? SubjectDescription { get; set; }

    public string? SubjectCode { get; set; }

    // public ICollection<ChapterSubjectModel>? Chapters { get; set; } 
    public int? NumberOfChapters { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CategoryName { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Slug { get; set; } = null!;

    public int? Like { get; set; }

    public int? View { get; set; }

    public int? NumberEnrollment { get; set; }
}
