using AtCoderStudy.Application;
using AtCoderStudy.Domain;

namespace AtCoderStudy.Infrastructure;

internal sealed class AtCoderStatementClient : IProblemStatementSource, IDisposable
{
    private readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(30) };

    public AtCoderStatementClient()
    {
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("AtCoderStudy/1.0 (personal learning CLI)");
        _client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ja");
    }

    public async Task<string> FetchAsync(ProblemEntry problem)
    {
        var url = $"https://atcoder.jp/contests/{problem.ContestId}/tasks/{problem.Id}?lang=ja";
        using var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadAsStringAsync();
        return AtCoderStatementMarkdownConverter.Convert(page, new Uri(url));
    }

    public void Dispose() => _client.Dispose();
}
