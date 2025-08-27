using Amusing.Models;

namespace Amusing.Services;

public class GitHubService
{
    private readonly HttpClient _httpClient;

    public GitHubService( HttpClient httpClient )
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd( "BlazorApp" ); // GitHub vereist User-Agent
    }

    public async Task<List<CommitModel>> GetCommitsAsync( string owner, string repo )
    {
        string url = $"https://api.github.com/repos/{owner}/{repo}/commits";
        List<GitHubCommitDto>? commits = await _httpClient.GetFromJsonAsync<List<GitHubCommitDto>>( url );

        return commits?.Select( c => new CommitModel
        {
            Sha = c.Sha,
            Message = c.Commit.Message,
            Author = c.Commit.Author.Name,
            Date = c.Commit.Author.Date
        } ).ToList() ?? [ ];
    }
}

public class GitHubCommitDto
{
    public string Sha { get; set; } = string.Empty;
    public CommitInfo Commit { get; set; } = new();
}

public class CommitInfo
{
    public CommitAuthor Author { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class CommitAuthor
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
