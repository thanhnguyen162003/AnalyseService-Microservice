using System.Net;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;


namespace Application.Features.RoadmapFeature.Commands;

public record CreateRoadmapSectionCommand : IRequest<ResponseModel>
{
    public RoadMapSectionCreateRequestModel RoadMapSectionCreateCommand;
}
public class CreateRoadmapSectionCommandHandler(
    IMapper mapper,
    AnalyseDbContext dbContext,
    IConfiguration configuration,
    ILogger<CreateRoadmapSectionCommandHandler> logger)
    : IRequestHandler<CreateRoadmapSectionCommand, ResponseModel>
{
    public async Task<ResponseModel> Handle(CreateRoadmapSectionCommand request, CancellationToken cancellationToken)
    {
        var client = new MongoClient(configuration.GetValue<string>("ConnectionStrings:MongoDbConnection"));
        using (var session = await client.StartSessionAsync(cancellationToken: cancellationToken))
        {
            // Begin transaction
            session.StartTransaction();
            try
            {
                var section = mapper.Map<Section>(request.RoadMapSectionCreateCommand);
                section.Id = ObjectId.GenerateNewId().ToString();
                await dbContext.Section.InsertOneAsync(section, cancellationToken: cancellationToken);
                foreach (var content in request.RoadMapSectionCreateCommand.Nodes)
                {
                    content.SectionId = section.Id;
                    content.Id = ObjectId.GenerateNewId().ToString();
                    content.CreatedAt = DateTime.UtcNow;
                    content.UpdatedAt = DateTime.UtcNow;
                    content.DeletedAt = null;
                }
                foreach (var content in request.RoadMapSectionCreateCommand.Edges)
                {
                    content.SectionId = section.Id;
                    content.Id = ObjectId.GenerateNewId().ToString();
                    content.CreatedAt = DateTime.UtcNow;
                    content.UpdatedAt = DateTime.UtcNow;
                    content.DeletedAt = null;
                }
                await dbContext.Node.InsertManyAsync(request.RoadMapSectionCreateCommand.Nodes,
                    cancellationToken: cancellationToken);
                await dbContext.Edge.InsertManyAsync(request.RoadMapSectionCreateCommand.Edges,
                    cancellationToken: cancellationToken);
                await session.CommitTransactionAsync(cancellationToken);
                return new ResponseModel(HttpStatusCode.OK, "Roadmap section created");
            }
            catch (Exception e)
            {
                logger.LogError("Error writing to MongoDB: " + e.Message);
                await session.AbortTransactionAsync(cancellationToken);
                return new ResponseModel(HttpStatusCode.BadRequest,"Unable to create roadmap section");
            }
        }
    }
}
