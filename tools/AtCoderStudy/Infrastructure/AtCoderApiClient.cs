using System.Text.Json;
using AtCoderStudy.Application;
using AtCoderStudy.Domain;

namespace AtCoderStudy.Infrastructure;

internal sealed class AtCoderApiClient : IAtCoderApi, IDisposable
{
    private const string Resources = "https://kenkoooo.com/atcoder/resources";
    private const string Submissions = "https://kenkoooo.com/atcoder/atcoder-api/v3/user/submissions";
    private readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(90) };
    private DateTimeOffset _nextRequestAt = DateTimeOffset.MinValue;

    public AtCoderApiClient() =>
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("AtCoderStudy/1.0 (personal learning CLI)");

    public async Task<IReadOnlyList<Problem>> GetProblemsAsync() =>
        await GetJsonAsync<List<Problem>>($"{Resources}/problems.json");

    public async Task<IReadOnlyList<ContestProblem>> GetContestProblemsAsync() =>
        await GetJsonAsync<List<ContestProblem>>($"{Resources}/contest-problem.json");

    public async Task<IReadOnlyDictionary<string, ProblemModel>> GetModelsAsync() =>
        await GetJsonAsync<Dictionary<string, ProblemModel>>($"{Resources}/problem-models.json");

    public async Task<IReadOnlyList<Submission>> GetSubmissionsAsync(
        string user, IReadOnlyList<Submission> cached)
    {
        var byId = cached.ToDictionary(x => x.Id);
        // Re-read the recent tail so changed judge results are refreshed.
        var fromSecond = byId.Count == 0 ? 0 : Math.Max(0, byId.Values.Max(x => x.EpochSecond) - 86400);

        while (true)
        {
            var url = $"{Submissions}?user={Uri.EscapeDataString(user)}&from_second={fromSecond}";
            var page = await GetJsonAsync<List<Submission>>(url);
            foreach (var submission in page) byId[submission.Id] = submission;
            if (page.Count < 500) break;

            var nextSecond = page.Max(x => x.EpochSecond) + 1;
            if (nextSecond <= fromSecond)
                throw new InvalidDataException("提出履歴のページを進められませんでした。");
            fromSecond = nextSecond;
        }

        return byId.Values.OrderBy(x => x.EpochSecond).ThenBy(x => x.Id).ToList();
    }

    private async Task<T> GetJsonAsync<T>(string url)
    {
        var delay = _nextRequestAt - DateTimeOffset.UtcNow;
        if (delay > TimeSpan.Zero) await Task.Delay(delay);
        _nextRequestAt = DateTimeOffset.UtcNow.AddMilliseconds(1100);

        using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonFormats.Api)
            ?? throw new InvalidDataException($"API から空の JSON が返されました: {url}");
    }

    public void Dispose() => _client.Dispose();
}
