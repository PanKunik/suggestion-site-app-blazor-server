namespace SuggestionApp.Library.DataAccess;

public interface IStatusData
{
    Task<List<StatusModel>> GetStatuses();
    Task CreateStatus(StatusModel status);
}