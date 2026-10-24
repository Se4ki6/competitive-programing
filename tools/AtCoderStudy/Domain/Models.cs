namespace AtCoderStudy.Domain;

internal sealed record Problem(string Id, string ContestId, string Title);

internal sealed record ContestProblem(string ContestId, string ProblemId, string ProblemIndex);

internal sealed record ProblemModel(double? Difficulty);

internal sealed record Submission(long Id, string ProblemId, long EpochSecond, string Result);

internal sealed record CatalogData(
    IReadOnlyList<Problem> Problems,
    IReadOnlyList<ContestProblem> ContestProblems,
    IReadOnlyDictionary<string, ProblemModel> Models,
    IReadOnlyList<Submission> Submissions);

internal enum ProblemStatus { New, Started, Attempted, Accepted }

internal enum StatusFilter { New, Unaccepted, All }

internal enum ProblemSort { Newest, Difficulty }

internal sealed record ProblemEntry(
    string Id,
    string ContestId,
    string Index,
    string Title,
    double? Difficulty,
    ProblemStatus Status);

internal sealed record FindQuery(
    string Series,
    IReadOnlySet<string> Letters,
    StatusFilter Status,
    int? MinimumDifficulty,
    int? MaximumDifficulty,
    int Limit,
    ProblemSort Sort);
