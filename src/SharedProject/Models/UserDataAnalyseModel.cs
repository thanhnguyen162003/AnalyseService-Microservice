namespace SharedProject.Models;

public class UserDataAnalyseModel
{
    public Guid UserId { get; set; }
    public string? Address { get; set; }
    public int Grade { get; set; }
    public string? SchoolName { get; set; }
    public string? Major { get; set; }
    public string? TypeExam { get; set; }
    public ICollection<Guid> Subjects { get; set; } = new HashSet<Guid>();
}
