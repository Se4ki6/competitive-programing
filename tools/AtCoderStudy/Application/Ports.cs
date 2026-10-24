using AtCoderStudy.Domain;

namespace AtCoderStudy.Application;

internal interface IAtCoderApi
{
    Task<IReadOnlyList<Problem>> GetProblemsAsync();
    Task<IReadOnlyList<ContestProblem>> GetContestProblemsAsync();
    Task<IReadOnlyDictionary<string, ProblemModel>> GetModelsAsync();
    Task<IReadOnlyList<Submission>> GetSubmissionsAsync(
        string user, IReadOnlyList<Submission> cached);
}

internal interface IStudyRepository
{
    Task<StudyConfig> ReadConfigAsync();
    Task WriteConfigAsync(StudyConfig config);
    Task<IReadOnlyList<Submission>> ReadSubmissionsAsync(string user);
    Task<CatalogData> LoadCatalogAsync(string user);
    Task<CatalogData?> TryLoadCatalogAsync(string? user);
    Task SaveCatalogAsync(string user, CatalogData data);
}

internal interface IProblemWorkspace
{
    IReadOnlySet<string> GetStartedProblems();
    Task<string> CreateAsync(ProblemEntry problem);
    Task<string> SaveStatementAsync(ProblemEntry problem, string markdown);
}

internal interface IProblemStatementSource
{
    Task<string> FetchAsync(ProblemEntry problem);
}
