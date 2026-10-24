using System.Text.Json;

namespace AtCoderStudy.Infrastructure;

internal static class JsonFormats
{
    public static readonly JsonSerializerOptions Api = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public static readonly JsonSerializerOptions Config = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
