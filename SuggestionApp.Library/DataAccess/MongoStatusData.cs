using Microsoft.Extensions.Caching.Memory;

namespace SuggestionApp.Library.DataAccess;

public class MongoStatusData : IStatusData
{
    private readonly IMongoCollection<StatusModel> _statuses;
    private readonly IMemoryCache _cache;
    private const string cacheName = "StatusData";

    public MongoStatusData(IDbConnection database, IMemoryCache cache)
    {
        _cache = cache;
        _statuses = database.StatusCollection;
    }

    public async Task<List<StatusModel>> GetStatuses()
    {
        var output = _cache.Get<List<StatusModel>>(cacheName);

        if(output is null)
        {
            var results = await _statuses.FindAsync(_ => true);
            output = results.ToList();

            _cache.Set<List<StatusModel>>(cacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public Task CreateStatus(StatusModel status)
    {
        return _statuses.InsertOneAsync(status);
    }
}