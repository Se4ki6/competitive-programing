using AtCoderStudy.Domain;

namespace AtCoderStudy.Application;

internal sealed record SyncResult(int ProblemCount, int SubmissionCount);

internal sealed record StartResult(string Directory, bool UsedFallback);

internal sealed class StudyService(
    IStudyRepository repository,
    IAtCoderApi api,
    IProblemWorkspace workspace,
    IProblemStatementSource statements)
{
    public async Task<string> GetUserAsync() => (await repository.ReadConfigAsync()).AtCoder.UserId;

    public async Task SetUserAsync(string user)
    {
        if (!IdRules.IsSafe(user))
            throw new ArgumentException("AtCoderユーザーIDを指定してください。");
        var config = await repository.ReadConfigAsync();
        config.AtCoder.UserId = user;
        await repository.WriteConfigAsync(config);
    }

    public async Task<SyncResult> SyncAsync()
    {
        var user = await RequireUserAsync();
        var problems = await api.GetProblemsAsync();
        var contestProblems = await api.GetContestProblemsAsync();
        var models = await api.GetModelsAsync();
        var cached = await repository.ReadSubmissionsAsync(user);
        var submissions = await api.GetSubmissionsAsync(user, cached);

        await repository.SaveCatalogAsync(user,
            new CatalogData(problems, contestProblems, models, submissions));
        return new SyncResult(problems.Count, submissions.Count);
    }

    public async Task<IReadOnlyList<ProblemEntry>> FindAsync(FindQuery query) =>
        (await LoadCatalogAsync()).Find(query);

    public async Task<StartResult> StartAsync(string problemId)
    {
        var (problem, usedFallback) = await ResolveProblemAsync(problemId);
        return new StartResult(await workspace.CreateAsync(problem), usedFallback);
    }

    public async Task<string> FetchStatementAsync(string problemId)
    {
        var (problem, _) = await ResolveProblemAsync(problemId);
        var markdown = await statements.FetchAsync(problem);
        if (!workspace.GetStartedProblems().Contains(IdRules.ProblemKey(problem.ContestId, problem.Id)))
            await workspace.CreateAsync(problem);
        return await workspace.SaveStatementAsync(problem, markdown);
    }

    private async Task<(ProblemEntry Problem, bool UsedFallback)> ResolveProblemAsync(string problemId)
    {
        if (!IdRules.IsSafe(problemId))
            throw new ArgumentException("abc300_b のような問題IDを指定してください。");

        var user = await GetUserAsync();
        var data = await repository.TryLoadCatalogAsync(user);
        var problem = data is null ? null :
            new ProblemCatalog(data, workspace.GetStartedProblems()).Get(problemId);
        return problem is null ? (CreateEntryFromId(problemId), true) : (problem, false);
    }

    private static ProblemEntry CreateEntryFromId(string problemId)
    {
        var separator = problemId.LastIndexOf('_');
        if (separator <= 0 || separator == problemId.Length - 1)
            throw new ArgumentException("問題情報がない場合は abc300_b のような問題IDを指定してください。");
        var contestId = problemId[..separator];
        if (!IdRules.IsSafe(contestId))
            throw new ArgumentException("問題IDからコンテストIDを判定できません。");
        return new ProblemEntry(problemId, contestId,
            problemId[(separator + 1)..].ToUpperInvariant(), "問題名未取得", null, ProblemStatus.New);
    }

    private async Task<ProblemCatalog> LoadCatalogAsync()
    {
        var user = await RequireUserAsync();
        var data = await repository.LoadCatalogAsync(user);
        return new ProblemCatalog(data, workspace.GetStartedProblems());
    }

    private async Task<string> RequireUserAsync()
    {
        var user = await GetUserAsync();
        return !string.IsNullOrWhiteSpace(user) ? user
            : throw new InvalidOperationException(
                "appsettings.local.json に AtCoder.UserId を設定するか、config set user <ID> を実行してください。");
    }
}
