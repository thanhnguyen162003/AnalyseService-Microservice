using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class RecommendedData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public ICollection<Guid>? SubjectIds { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public ICollection<Guid>? DocumentIds { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public ICollection<Guid>? FlashcardIds { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid? UserId { get; set; }
    public int Grade { get; set; }
    public string? TypeExam { get; set; }
}
