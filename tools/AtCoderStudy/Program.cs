using System.Text.Json;
using AtCoderStudy.Application;
using AtCoderStudy.Cli;
using AtCoderStudy.Infrastructure;

try
{
    var root = FindRoot();
    using var api = new AtCoderApiClient();
    using var statements = new AtCoderStatementClient();
    var service = new StudyService(
        new FileStudyRepository(root), api, new FileProblemWorkspace(root), statements);
    await new StudyCli(service).RunAsync(args);
}
catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or
                           InvalidDataException or IOException or HttpRequestException or
                           TaskCanceledException or JsonException)
{
    Console.Error.WriteLine($"エラー: {ex.Message}");
    Environment.ExitCode = 1;
}

static string FindRoot()
{
    for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory()); directory is not null;
         directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "tools", "AtCoderStudy", "AtCoderStudy.csproj")))
            return directory.FullName;
    }
    throw new InvalidOperationException("リポジトリ内から実行してください。");
}
