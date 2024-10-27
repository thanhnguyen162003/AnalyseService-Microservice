using Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Infrastructure.Data;

public class AnalyseDbContext
{
    private readonly IMongoDatabase _database;

    public AnalyseDbContext()
    {
    }
    public AnalyseDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDbConnection");
        var databaseName = configuration["MongoDbSettings:DatabaseName"];
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }
    public IMongoCollection<Edge> Edge => _database.GetCollection<Edge>("Edge");
    public IMongoCollection<Node> Node => _database.GetCollection<Node>("Node");
    public IMongoCollection<Roadmap> Roadmap => _database.GetCollection<Roadmap>("Roadmap");
    public IMongoCollection<UserAnalyseEntity> UserAnalyseEntity => _database.GetCollection<UserAnalyseEntity>("UserAnalyseEntity");
    public IMongoCollection<RecommendedData> RecommendedData => _database.GetCollection<RecommendedData>("RecommendedData");
    public IMongoCollection<DocumentDay> DocumentDay => _database.GetCollection<DocumentDay>("DocumentDay");

}
