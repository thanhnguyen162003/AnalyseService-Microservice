using System.Text.Json.Serialization;
using Discussion_Microservice.Domain.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Section : BaseAuditableEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore]
    public string Id { get; set; }
    
    public string SectionName { get; set; }

    public string Content { get; set; }

    public string SectionDescription { get; set; }
    
    public int Order { get; set; }
    
    public ICollection<Node> Nodes { get; set; } = new List<Node>();

    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
}
