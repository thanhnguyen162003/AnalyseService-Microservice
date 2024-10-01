using Domain.Entities;

namespace Application.Common.Models;

public class RoadMapSectionCreateRequestModel
{
    public string RoadmapName { get; set; }

    public string ContentJson { get; set; }

    public string RoadmapDescription { get; set; }
    
    public List<Guid> RoadmapDocumentId { get; set; }
    
    public List<Node> Nodes { get; set; }
    
    public List<Edge> Edges { get; set; }
}

