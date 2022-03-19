using Microsoft.AspNetCore.Components;

namespace SuggestionAppUI.Pages;

public partial class Details
{
    [Parameter]
    public string Id { get; set; }

    private SuggestionModel suggestion;
    private UserModel loggedInUser;

    private List<StatusModel> statuses;
    private string settingStatus = "";
    private string urlText = "";

    protected async override Task OnInitializedAsync()
    {
        suggestion = await suggestionData.GetSuggestion(Id);
        loggedInUser = await authProvider.GetUserFromAuth(userData);
        statuses = await statusData.GetStatuses();
    }

    private async Task CompleteSetStatus()
    {
        switch (settingStatus)
        {
            case "completed":
                if (string.IsNullOrWhiteSpace(urlText))
                {
                    return;
                }
                suggestion.Status = statuses.Where(s => s.Name.ToLower() == settingStatus.ToLower()).First();
                suggestion.OwnerNotes = "You are right, this is an important topic for developers."
                                        + $"We created resource about it here: <a href='{urlText}' target='_blank'>{urlText}</a>";
                break;
            case "watching":
                suggestion.Status = statuses.Where(s => s.Name.ToLower() == settingStatus.ToLower()).First();
                suggestion.OwnerNotes = "We noticed the interest this suggestion is getting! If more people"
                                        + "are interested we will address this topic in an upcoming resource.";
                break;
            case "upcoming":
                suggestion.Status = statuses.Where(s => s.Name.ToLower() == settingStatus.ToLower()).First();
                suggestion.OwnerNotes = "Great suggestion! We have a resource in the pipeline to address this topic.";
                break;
            case "dismissed":
                suggestion.Status = statuses.Where(s => s.Name.ToLower() == settingStatus.ToLower()).First();
                suggestion.OwnerNotes = "Sometimes a good idea doesn't fit within our scope and vision. This is one of those ideas.";
                break;
            default:
                return;
        }

        settingStatus = null;
        await suggestionData.UpdateSuggestion(suggestion);
    }

    private void ClosePage()
    {
        navigationManager.NavigateTo("/");
    }

    private string GetVoteClass()
    {
        if (suggestion.UserVotes is null || suggestion.UserVotes.Count == 0)
        {
            return "suggestion-detail-no-votes";
        }
        else if (suggestion.UserVotes.Contains(loggedInUser?.Id))
        {
            return "suggestion-detail-voted";
        }
        else
        {
            return "suggestion-detail-not-voted";
        }
    }

    private string GetUpvoteTopText()
    {
        if (suggestion.UserVotes?.Count > 0)
        {
            return suggestion.UserVotes.Count.ToString("00");
        }
        else
        {
            if (suggestion.Author.Id == loggedInUser?.Id)
            {
                return "Awaiting";
            }
            else
            {
                return "Click To";
            }
        }
    }

    private string GetUpvoteBottomText()
    {
        if (suggestion.UserVotes?.Count > 1)
            return "Upvotes";
        else
            return "Uptove";
    }
    private async Task VoteUp()
    {
        if (loggedInUser is not null)
        {
            if (suggestion.Author.Id == loggedInUser.Id)
            {
                return;
            }

            if (suggestion.UserVotes.Add(loggedInUser.Id) == false)
            {
                suggestion.UserVotes.Remove(loggedInUser.Id);
            }

            await suggestionData.UpvoteSuggestion(suggestion.Id, loggedInUser.Id);
        }
        else
        {
            navigationManager.NavigateTo("/MicrosoftIdentity/Account/SignIn", true);
        }
    }

    private string GetStatusClass()
    {
        if (suggestion is null || suggestion.Status is null)
        {
            return "suggestion-details-status-none";
        }

        string output = suggestion.Status.Name switch
        {
            "Completed" => "suggestion-detail-status-completed",
            "Watching" => "suggestion-detail-status-watching",
            "Upcoming" => "suggestion-detail-status-upcoming",
            "Dismissed" => "suggestion-detail-status-dismissed",
            _ => "suggestion-detail-status-none"
        };

        return output;
    }
}