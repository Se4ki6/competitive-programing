using System.Text.RegularExpressions;

namespace AtCoderStudy.Domain;

internal sealed class ProblemCatalog
{
    private readonly IReadOnlyList<ProblemEntry> _entries;

    public ProblemCatalog(CatalogData data, IReadOnlySet<string> startedProblems)
    {
        _entries = BuildEntries(data, startedProblems).ToList();
    }

    public ProblemEntry? Get(string problemId) =>
        _entries.FirstOrDefault(x => string.Equals(x.Id, problemId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ProblemEntry> Find(FindQuery query)
    {
        var entries = _entries
            .Where(x => query.Series == "all" ||
                        x.ContestId.StartsWith(query.Series, StringComparison.OrdinalIgnoreCase))
            .Where(x => query.Letters.Contains(x.Index))
            .Where(x => query.Status switch
            {
                StatusFilter.New => x.Status == ProblemStatus.New,
                StatusFilter.Unaccepted => x.Status != ProblemStatus.Accepted,
                _ => true
            })
            .Where(x => (query.MinimumDifficulty is null && query.MaximumDifficulty is null) ||
                        x.Difficulty is not null &&
                        (query.MinimumDifficulty is null || x.Difficulty >= query.MinimumDifficulty) &&
                        (query.MaximumDifficulty is null || x.Difficulty <= query.MaximumDifficulty));

        entries = query.Sort == ProblemSort.Difficulty
            ? entries.OrderBy(x => x.Difficulty ?? double.MaxValue)
                .ThenByDescending(x => ContestNumber(x.ContestId)).ThenBy(x => x.Id)
            : entries.OrderByDescending(x => ContestNumber(x.ContestId))
                .ThenByDescending(x => x.ContestId).ThenBy(x => x.Index);

        return entries.Take(query.Limit).ToList();
    }

    private static IEnumerable<ProblemEntry> BuildEntries(
        CatalogData data, IReadOnlySet<string> startedProblems)
    {
        var problemsById = data.Problems.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var submissionsByProblem = data.Submissions
            .GroupBy(x => x.ProblemId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var group in data.ContestProblems.GroupBy(x => x.ProblemId, StringComparer.OrdinalIgnoreCase))
        {
            if (!problemsById.TryGetValue(group.Key, out var problem)) continue;
            var link = group.FirstOrDefault(x =>
                string.Equals(x.ContestId, problem.ContestId, StringComparison.OrdinalIgnoreCase))
                ?? group.First();
            if (!IdRules.IsSafe(link.ContestId) || !IdRules.IsSafe(link.ProblemId)) continue;

            data.Models.TryGetValue(link.ProblemId, out var model);
            var difficulty = model?.Difficulty;
            if (difficulty is not null && !double.IsFinite(difficulty.Value)) difficulty = null;

            submissionsByProblem.TryGetValue(link.ProblemId, out var history);
            var status = history?.Any(x => x.Result == "AC") == true ? ProblemStatus.Accepted
                : history?.Count > 0 ? ProblemStatus.Attempted
                : startedProblems.Contains(IdRules.ProblemKey(link.ContestId, link.ProblemId))
                    ? ProblemStatus.Started
                    : ProblemStatus.New;

            yield return new ProblemEntry(problem.Id, link.ContestId, link.ProblemIndex,
                problem.Title, difficulty, status);
        }
    }

    private static int ContestNumber(string id)
    {
        var match = Regex.Match(id, "[0-9]+$", RegexOptions.CultureInvariant);
        return match.Success && int.TryParse(match.Value, out var number) ? number : -1;
    }
}

internal static class IdRules
{
    public static bool IsSafe(string? id) =>
        !string.IsNullOrEmpty(id) &&
        Regex.IsMatch(id, "^[a-zA-Z0-9_-]+$", RegexOptions.CultureInvariant);

    public static string ProblemKey(string contestId, string problemId) =>
        $"{contestId}/{problemId}";
}
