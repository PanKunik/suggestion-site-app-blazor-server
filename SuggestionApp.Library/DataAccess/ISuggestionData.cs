namespace SuggestionApp.Library.DataAccess;

public interface ISuggestionData
{
    Task<List<SuggestionModel>> GetSuggestions();
    Task<List<SuggestionModel>> GetUsersSuggestions(string userId);
    Task<List<SuggestionModel>> GetApprovedSuggestions();
    Task<SuggestionModel> GetSuggestion(string id);
    Task<List<SuggestionModel>> GetSuggestionsWaitingForApproval();
    Task UpdateSuggestion(SuggestionModel suggestion);
    Task UpvoteSuggestion(string suggestionId, string userId);
    Task CreateSuggestion(SuggestionModel suggestion);
}