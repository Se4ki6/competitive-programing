using AtCoderStudy.Application;
using AtCoderStudy.Domain;

namespace AtCoderStudy.Cli;

internal sealed class StudyCli(StudyService service)
{
    public async Task RunAsync(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            PrintHelp();
            return;
        }

        switch (args[0])
        {
            case "config":
                await ConfigAsync(args);
                break;
            case "sync":
                if (args.Length != 1) throw new ArgumentException("sync に引数はありません。");
                Console.WriteLine("問題情報と提出履歴を取得しています...");
                var result = await service.SyncAsync();
                Console.WriteLine($"同期完了: 問題 {result.ProblemCount} 件、提出 {result.SubmissionCount} 件");
                break;
            case "find":
                PrintProblems(await service.FindAsync(ParseFindQuery(args)));
                break;
            case "start":
                if (args.Length != 2)
                    throw new ArgumentException("使い方: start <問題ID> (例: start abc300_b)");
                var started = await service.StartAsync(args[1]);
                Console.WriteLine($"問題フォルダを作成しました: {started.Directory}");
                if (started.UsedFallback)
                    Console.WriteLine("問題情報を取得できていないため、問題名と難易度は未確認です。公式リンクを確認してください。");
                break;
            case "statement":
                if (args.Length != 2)
                    throw new ArgumentException("使い方: statement <問題ID> (例: statement abc300_b)");
                Console.WriteLine($"公式問題文を保存しました: {await service.FetchStatementAsync(args[1])}");
                break;
            default:
                throw new ArgumentException($"不明なコマンド: {args[0]}。help を参照してください。");
        }
    }

    private async Task ConfigAsync(string[] args)
    {
        if (args.Length == 4 && args[1] == "set" && args[2] == "user")
        {
            var user = args[3].Trim();
            await service.SetUserAsync(user);
            Console.WriteLine($"AtCoderユーザーIDを保存しました: {user}");
            return;
        }
        if (args.Length == 2 && args[1] == "show")
        {
            var user = await service.GetUserAsync();
            Console.WriteLine($"AtCoder.UserId: {(string.IsNullOrWhiteSpace(user) ? "未設定" : user)}");
            return;
        }
        throw new ArgumentException("使い方: config set user <ID> または config show");
    }

    private static FindQuery ParseFindQuery(string[] args)
    {
        var options = ParseOptions(args, 1,
            ["--series", "--letters", "--status", "--difficulty-min", "--difficulty-max", "--limit", "--sort"]);
        var series = GetOption(options, "--series", "abc").ToLowerInvariant();
        if (series != "all" && !IdRules.IsSafe(series))
            throw new ArgumentException("--series には abc、arc などを指定してください。");

        var letters = GetOption(options, "--letters", "A,B")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (letters.Count == 0 || letters.Any(x => !IdRules.IsSafe(x)))
            throw new ArgumentException("--letters は A,B の形式で指定してください。");

        var status = GetOption(options, "--status", "new") switch
        {
            "new" => StatusFilter.New,
            "unaccepted" => StatusFilter.Unaccepted,
            "all" => StatusFilter.All,
            _ => throw new ArgumentException("--status は new、unaccepted、all から選んでください。")
        };
        var sort = GetOption(options, "--sort", "newest") switch
        {
            "newest" => ProblemSort.Newest,
            "difficulty" => ProblemSort.Difficulty,
            _ => throw new ArgumentException("--sort は newest または difficulty を指定してください。")
        };
        var min = ParseOptionalNumber(options, "--difficulty-min");
        var max = ParseOptionalNumber(options, "--difficulty-max");
        if (min > max) throw new ArgumentException("難易度の最小値は最大値以下にしてください。");
        if (!int.TryParse(GetOption(options, "--limit", "20"), out var limit) || limit is < 1 or > 500)
            throw new ArgumentException("--limit は 1～500 の整数を指定してください。");

        return new FindQuery(series, letters, status, min, max, limit, sort);
    }

    private static void PrintProblems(IReadOnlyList<ProblemEntry> entries)
    {
        if (entries.Count == 0)
        {
            Console.WriteLine("条件に合う問題がありません。");
            return;
        }

        Console.WriteLine("問題ID\t番号\t難易度\t状態\t問題名 / URL");
        foreach (var entry in entries)
        {
            var difficulty = entry.Difficulty?.ToString("F0") ?? "不明";
            var status = entry.Status switch
            {
                ProblemStatus.Accepted => "AC",
                ProblemStatus.Attempted => "attempted",
                ProblemStatus.Started => "started",
                _ => "new"
            };
            Console.WriteLine($"{entry.Id}\t{entry.Index}\t{difficulty}\t{status}\t{entry.Title}");
            Console.WriteLine($"  https://atcoder.jp/contests/{entry.ContestId}/tasks/{entry.Id}");
        }
    }

    private static Dictionary<string, string> ParseOptions(
        string[] args, int start, IEnumerable<string> allowed)
    {
        var allowedSet = allowed.ToHashSet(StringComparer.Ordinal);
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = start; i < args.Length; i += 2)
        {
            if (!allowedSet.Contains(args[i]))
                throw new ArgumentException($"不明なオプション: {args[i]}");
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException($"{args[i]} の値を指定してください。");
            if (!options.TryAdd(args[i], args[i + 1]))
                throw new ArgumentException($"{args[i]} が重複しています。");
        }
        return options;
    }

    private static string GetOption(Dictionary<string, string> options, string name, string fallback) =>
        options.GetValueOrDefault(name, fallback);

    private static int? ParseOptionalNumber(Dictionary<string, string> options, string name)
    {
        if (!options.TryGetValue(name, out var value)) return null;
        if (!int.TryParse(value, out var result))
            throw new ArgumentException($"{name} には整数を指定してください。");
        return result;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("AtCoder Study - C#学習用CLI");
        Console.WriteLine();
        Console.WriteLine("  config set user <ID>     appsettings.local.json にユーザーIDを保存");
        Console.WriteLine("  config show              設定を表示");
        Console.WriteLine("  sync                     問題・難易度・提出履歴を取得");
        Console.WriteLine("  find [options]           未着手の問題を検索");
        Console.WriteLine("  start <問題ID>           問題フォルダを作成");
        Console.WriteLine("  statement <問題ID>       公式問題文をMarkdownに保存");
        Console.WriteLine();
        Console.WriteLine("find のオプション:");
        Console.WriteLine("  --series abc|arc|all     デフォルト: abc");
        Console.WriteLine("  --letters A,B            デフォルト: A,B");
        Console.WriteLine("  --status new|unaccepted|all  デフォルト: new");
        Console.WriteLine("  --difficulty-min N / --difficulty-max N");
        Console.WriteLine("  --sort newest|difficulty デフォルト: newest");
        Console.WriteLine("  --limit N                デフォルト: 20");
    }
}
