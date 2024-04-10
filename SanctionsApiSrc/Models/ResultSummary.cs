using System.Text.Json.Serialization;

namespace SanctionsApi.Models;

public class ResultSummary
{
    [JsonPropertyName("title")]
    public string ReportTitle => "Sanctions Check Report";

    [JsonPropertyName("version")]
    public string MetaData { get; set; } = default!;

    [JsonPropertyName("downloaded")]
    public string SourceFileDownloadedDate { get; set; } = default!;

    [JsonPropertyName("searchtext")]
    public string SearchText { get; set; } = default!;

    [JsonPropertyName("numberOfResults")]
    public int NumberOfResults { get; set; }
}