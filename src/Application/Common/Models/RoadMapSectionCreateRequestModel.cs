using Domain.Entities;

namespace Application.Common.Models;

public class RoadMapSectionCreateRequestModel
{
    // public string Id { get; set; }
    
    public string SectionName { get; set; }

    public string Content { get; set; }

    public string SectionDescription { get; set; }
    
    public List<Node> Nodes { get; set; }
    
    public List<Edge> Edges { get; set; }
}

