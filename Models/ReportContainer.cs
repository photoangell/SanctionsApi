using System.Text.Json.Serialization;

namespace SanctionsApi.Models;

public class ReportContainer
{
    public ReportContainer()
    {
        Report = new Report();
    }

    [JsonPropertyName("report")]
    public Report Report { get; }
}