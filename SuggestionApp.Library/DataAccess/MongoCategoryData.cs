using Microsoft.Extensions.Caching.Memory;

namespace SuggestionApp.Library.DataAccess;

public class MongoCategoryData : ICategoryData
{
    private readonly IMongoCollection<CategoryModel> _categories;
    private readonly IMemoryCache _cache;
    private const string CacheName = "CategoryData";

    public MongoCategoryData(IDbConnection database, IMemoryCache cache)
    {
        _cache = cache;
        _categories = database.CategoryCollection;
    }

    public async Task<List<CategoryModel>> GetCategories()
    {
        var output = _cache.Get<List<CategoryModel>>(CacheName);

        if(output is null)
        {
            var results = await _categories.FindAsync(_ => true);
            output = results.ToList();

            _cache.Set<List<CategoryModel>>(CacheName, output, TimeSpan.FromDays(1));
        }

        return output;
    }

    public Task CreateCategory(CategoryModel category)
    {
        return _categories.InsertOneAsync(category);
    }
}