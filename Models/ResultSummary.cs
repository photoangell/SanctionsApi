using System.Text.Json.Serialization;

namespace SanctionsApi.Models;

public class ResultSummary
{
    [JsonPropertyName("title")]
    public string ReportTitle { get; set; } = default!;

    [JsonPropertyName("version")]
    public string SourceFileVersion { get; set; } = default!;

    [JsonPropertyName("downloaded")]
    public string SourceFileDownloadedDate { get; set; } = default!;

    [JsonPropertyName("searchtext")]
    public string SearchText { get; set; } = default!;

    [JsonPropertyName("numberOfResults")]
    public int NumberOfResults { get; set; }
}