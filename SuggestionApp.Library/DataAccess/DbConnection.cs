using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace SuggestionApp.Library.DataAccess;

// TODO: Register on Atlas MongoDB

public class DbConnection
{
    private readonly IConfiguration _configuration;
    private readonly IMongoDatabase _database;
    private string _connectionId = "MongoDB";

    public string DatabaseName { get; private set; }
    public string CategoryCollectionName { get; private set; } = "categories";
    public string StatusCollectionName { get; private set; } = "statuses";
    public string UserCollectionName { get; private set; } = "users";
    public string SuggestionCollectionName { get; private set; } = "suggestions";

    public MongoClient Client { get; private set; }
    public IMongoCollection<CategoryModel> CategoryCollection { get; private set; }
    public IMongoCollection<StatusModel> StatusCollection { get; private set; }
    public IMongoCollection<UserModel> UserCollection { get; private set; }
    public IMongoCollection<SuggestionModel> SuggestionCollection { get; private set; }

    public DbConnection(IConfiguration configuration)
    {
        _configuration = configuration;

        Client = new MongoClient(_configuration.GetConnectionString(_connectionId));
        DatabaseName = _configuration["DatabaseName"];
        _database = Client.GetDatabase(DatabaseName);

        CategoryCollection = _database.GetCollection<CategoryModel>(CategoryCollectionName);
        StatusCollection = _database.GetCollection<StatusModel>(StatusCollectionName);
        UserCollection = _database.GetCollection<UserModel>(UserCollectionName);
        SuggestionCollection = _database.GetCollection<SuggestionModel>(SuggestionCollectionName);
    }
}