namespace SuggestionApp.Library.DataAccess;

public interface IUserData
{
    Task<List<UserModel>> GetUsers();
    Task<UserModel> GetUser(string id);
    Task<UserModel> GetUserFromAuthentication(string objectId);
    Task CreateUser(UserModel user);
    Task UpdateUser(UserModel user);
}