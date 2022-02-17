namespace SuggestionApp.Library.DataAccess;

public interface ICategoryData
{
    Task<List<CategoryModel>> GetCategories();
    Task CreateCategory(CategoryModel category);
}