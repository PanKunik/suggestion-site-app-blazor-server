using Microsoft.Extensions.Caching.Memory;

namespace SuggestionApp.Library.DataAccess;

public class MongoSuggestionData : ISuggestionData
{
    private readonly IDbConnection _database;
    private readonly IUserData _userData;
    private readonly IMemoryCache _cache;
    private readonly IMongoCollection<SuggestionModel> _suggestions;
    private const string CacheName = "SugestionDate";

    public MongoSuggestionData(IDbConnection database, IUserData userData, IMemoryCache cache)
    {
        _database = database;
        _userData = userData;
        _cache = cache;
        _suggestions = database.SuggestionCollection;
    }

    public async Task<List<SuggestionModel>> GetSuggestions()
    {
        var output = _cache.Get<List<SuggestionModel>>(CacheName);

        if (output is null)
        {
            var results = await _suggestions.FindAsync(s => s.Archived == false);
            output = results.ToList();

            _cache.Set<List<SuggestionModel>>(CacheName, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<SuggestionModel>> GetUsersSuggestions(string userId)
    {
        var output = _cache.Get<List<SuggestionModel>>(userId);
        if(output is null)
        {
            var results = await _suggestions.FindAsync(s => s.Author.Id == userId);
            output = results.ToList();

            _cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }

        return output;
    }

    public async Task<List<SuggestionModel>> GetApprovedSuggestions()
    {
        var output = await GetSuggestions();
        return output.Where(x => x.ApprovedForRelease).ToList();
    }

    public async Task<SuggestionModel> GetSuggestion(string id)
    {
        var results = await _suggestions.FindAsync(s => s.Id == id);
        return await results.FirstOrDefaultAsync();
    }

    public async Task<List<SuggestionModel>> GetSuggestionsWaitingForApproval()
    {
        var output = await GetSuggestions();
        return output.Where(x => x.ApprovedForRelease == false
                              && x.Rejected == false).ToList();
    }

    public async Task UpdateSuggestion(SuggestionModel suggestion)
    {
        await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
        _cache.Remove(CacheName);
    }

    public async Task UpvoteSuggestion(string suggestionId, string userId)
    {
        var client = _database.Client;

        using var session = await client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var database = client.GetDatabase(_database.DatabaseName);
            var suggestionsInTransaction = database.GetCollection<SuggestionModel>(_database.SuggestionCollectionName);
            var suggestion = (await suggestionsInTransaction.FindAsync(s => s.Id == suggestionId)).First();

            bool isUpvote = suggestion.UserVotes.Add(userId);
            if (isUpvote == false)
            {
                suggestion.UserVotes.Remove(userId);
            }

            await suggestionsInTransaction.ReplaceOneAsync(s => s.Id == suggestionId, suggestion);

            var usersInTransaction = database.GetCollection<UserModel>(_database.UserCollectionName);
            var user = await _userData.GetUser(suggestion.Author.Id);

            if (isUpvote)
            {
                user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
            }
            else
            {
                var suggestionToRemove = user.VotedOnSuggestions.Where(s => s.Id == suggestionId).First();
                user.VotedOnSuggestions.Remove(suggestionToRemove);
            }

            await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);
            await session.CommitTransactionAsync();

            _cache.Remove(CacheName);
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task CreateSuggestion(SuggestionModel suggestion)
    {
        var client = _database.Client;

        using var session = await client.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var database = client.GetDatabase(_database.DatabaseName);
            var suggestionsInTransaction = database.GetCollection<SuggestionModel>(_database.SuggestionCollectionName);
            await suggestionsInTransaction.InsertOneAsync(suggestion);

            var usersInTransaction = database.GetCollection<UserModel>(_database.UserCollectionName);
            var user = await _userData.GetUser(suggestion.Author.Id);
            user.AuthoredSuggestions.Add(new BasicSuggestionModel(suggestion));
            await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

            await session.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}