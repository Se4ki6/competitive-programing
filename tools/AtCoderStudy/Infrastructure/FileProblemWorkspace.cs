using AtCoderStudy.Application;
using AtCoderStudy.Domain;

namespace AtCoderStudy.Infrastructure;

internal sealed class FileProblemWorkspace(string root) : IProblemWorkspace
{
    private string ProblemsRoot => Path.Combine(root, "problems");

    public IReadOnlySet<string> GetStartedProblems()
    {
        var started = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(ProblemsRoot)) return started;

        foreach (var contest in Directory.EnumerateDirectories(ProblemsRoot))
        foreach (var problem in Directory.EnumerateDirectories(contest))
            started.Add(IdRules.ProblemKey(Path.GetFileName(contest), Path.GetFileName(problem)));
        return started;
    }

    public async Task<string> CreateAsync(ProblemEntry problem)
    {
        var directory = Path.Combine(ProblemsRoot, problem.ContestId, problem.Id);
        if (Directory.Exists(directory))
            throw new InvalidOperationException($"既に問題フォルダがあります: {directory}");

        Directory.CreateDirectory(directory);
        var problemUrl = $"https://atcoder.jp/contests/{problem.ContestId}/tasks/{problem.Id}";
        var editorialUrl = $"https://atcoder.jp/contests/{problem.ContestId}/editorial";
        var difficulty = problem.Difficulty?.ToString("F0") ?? "不明";

        await File.WriteAllTextAsync(Path.Combine(directory, "README.md"),
            $"# {problem.ContestId.ToUpperInvariant()} {problem.Index}: {problem.Title}\n\n" +
            $"- 問題: {problemUrl}\n- ローカル問題文: `statement.md`（Git管理外）\n" +
            $"- 推定難易度: {difficulty}\n\n" +
            "## 問題の要約\n\n\n## 自分の方針\n\n\n## 計算量\n\n\n## 提出・振り返り\n\n");

        await File.WriteAllTextAsync(Path.Combine(directory, "Program.cs"),
            "using System;\n\ninternal static class Program\n{\n    private static void Main()\n    {\n        // 解答を書く\n    }\n}\n");

        await File.WriteAllTextAsync(Path.Combine(directory, "Solution.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
            "  <PropertyGroup>\n" +
            "    <OutputType>Exe</OutputType>\n" +
            "    <TargetFramework>net10.0</TargetFramework>\n" +
            "    <ImplicitUsings>enable</ImplicitUsings>\n" +
            "    <Nullable>enable</Nullable>\n" +
            "  </PropertyGroup>\n" +
            "</Project>\n");

        await File.WriteAllTextAsync(Path.Combine(directory, "editorial.md"),
            "# 模範解答との比較\n\n" +
            $"- コンテスト解説: {editorialUrl}\n\n" +
            "## 解法の要点\n\n\n## 自分の解答との差\n\n\n## 参考実装・別解\n\n" +
            "必要に応じて、出典リンクと自分で整理したC#の実装を記録する。\n\n" +
            "## 次に解くときの着眼点\n\n");

        return directory;
    }

    public async Task<string> SaveStatementAsync(ProblemEntry problem, string markdown)
    {
        var directory = Path.Combine(ProblemsRoot, problem.ContestId, problem.Id);
        if (!Directory.Exists(directory))
            throw new InvalidOperationException($"問題フォルダがありません: {directory}");

        var path = Path.Combine(directory, "statement.md");
        var temporaryPath = path + ".tmp";
        await File.WriteAllTextAsync(temporaryPath, markdown);
        File.Move(temporaryPath, path, overwrite: true);
        return path;
    }
}
