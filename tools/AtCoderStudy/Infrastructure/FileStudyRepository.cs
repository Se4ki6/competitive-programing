using System.Text.Json;
using AtCoderStudy.Application;
using AtCoderStudy.Domain;

namespace AtCoderStudy.Infrastructure;

internal sealed class FileStudyRepository(string root) : IStudyRepository
{
    private readonly string _studyDirectory = Path.Combine(root, ".study");

    private string ConfigPath => Path.Combine(root, "appsettings.local.json");
    private string LegacyConfigPath => Path.Combine(_studyDirectory, "config.json");
    private string CachePath(string name) => Path.Combine(_studyDirectory, "cache", name + ".json");
    private static string SubmissionCacheName(string user) => "submissions-" + user.ToLowerInvariant();

    public async Task<StudyConfig> ReadConfigAsync()
    {
        if (File.Exists(ConfigPath))
        {
            var config = await ReadJsonAsync<StudyConfig>(ConfigPath, JsonFormats.Config);
            if (config.AtCoder is null ||
                !string.IsNullOrWhiteSpace(config.AtCoder.UserId) && !IdRules.IsSafe(config.AtCoder.UserId))
                throw new InvalidDataException("appsettings.local.json の AtCoder.UserId が不正です。");
            return config;
        }

        if (File.Exists(LegacyConfigPath))
        {
            var legacy = await ReadJsonAsync<LegacyConfig>(LegacyConfigPath, JsonFormats.Config);
            if (!string.IsNullOrWhiteSpace(legacy.User) && !IdRules.IsSafe(legacy.User))
                throw new InvalidDataException(".study/config.json の User が不正です。");
            return new StudyConfig { AtCoder = new AtCoderSettings { UserId = legacy.User } };
        }

        return new StudyConfig();
    }

    public Task WriteConfigAsync(StudyConfig config) =>
        WriteJsonAsync(ConfigPath, config, JsonFormats.Config);

    public async Task<IReadOnlyList<Submission>> ReadSubmissionsAsync(string user)
    {
        var path = CachePath(SubmissionCacheName(user));
        return File.Exists(path)
            ? await ReadJsonAsync<List<Submission>>(path, JsonFormats.Api)
            : [];
    }

    public async Task<CatalogData> LoadCatalogAsync(string user)
    {
        var problems = await ReadCacheAsync<List<Problem>>("problems");
        var contestProblems = await ReadCacheAsync<List<ContestProblem>>("contest-problem");
        var models = await ReadCacheAsync<Dictionary<string, ProblemModel>>("problem-models");
        var submissions = await ReadCacheAsync<List<Submission>>(SubmissionCacheName(user));
        return new CatalogData(problems, contestProblems, models, submissions);
    }

    public async Task<CatalogData?> TryLoadCatalogAsync(string? user)
    {
        if (!File.Exists(CachePath("problems")) ||
            !File.Exists(CachePath("contest-problem")) ||
            !File.Exists(CachePath("problem-models")))
            return null;

        var problems = await ReadCacheAsync<List<Problem>>("problems");
        var contestProblems = await ReadCacheAsync<List<ContestProblem>>("contest-problem");
        var models = await ReadCacheAsync<Dictionary<string, ProblemModel>>("problem-models");
        var submissions = string.IsNullOrWhiteSpace(user)
            ? []
            : await ReadSubmissionsAsync(user);
        return new CatalogData(problems, contestProblems, models, submissions);
    }

    public async Task SaveCatalogAsync(string user, CatalogData data)
    {
        await WriteJsonAsync(CachePath("problems"), data.Problems, JsonFormats.Api);
        await WriteJsonAsync(CachePath("contest-problem"), data.ContestProblems, JsonFormats.Api);
        await WriteJsonAsync(CachePath("problem-models"), data.Models, JsonFormats.Api);
        await WriteJsonAsync(CachePath(SubmissionCacheName(user)), data.Submissions, JsonFormats.Api);
    }

    private async Task<T> ReadCacheAsync<T>(string name)
    {
        var path = CachePath(name);
        if (!File.Exists(path))
            throw new InvalidOperationException("取得データがありません。先に sync を実行してください。");
        return await ReadJsonAsync<T>(path, JsonFormats.Api);
    }

    private static async Task<T> ReadJsonAsync<T>(string path, JsonSerializerOptions options)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, options)
            ?? throw new InvalidDataException($"JSON が空です: {path}");
    }

    private static async Task WriteJsonAsync<T>(string path, T value, JsonSerializerOptions options)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporaryPath = path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
            await JsonSerializer.SerializeAsync(stream, value, options);
        File.Move(temporaryPath, path, overwrite: true);
    }

    private sealed class LegacyConfig
    {
        public string User { get; set; } = "";
    }
}
