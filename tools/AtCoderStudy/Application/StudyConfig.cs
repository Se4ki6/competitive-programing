namespace AtCoderStudy.Application;

internal sealed class StudyConfig
{
    public AtCoderSettings AtCoder { get; set; } = new();
}

internal sealed class AtCoderSettings
{
    public string UserId { get; set; } = "";
}
