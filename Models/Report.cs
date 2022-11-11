using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SanctionsApi.Models;

public class Report
{
    public Report()
    {
        ResultSummary = new ResultSummary();
        SanctionsMatches = new List<Dictionary<string, string>>();
    }

    [JsonPropertyName("resultSummary")]
    public ResultSummary ResultSummary { get; set; }

    [JsonPropertyName("record")]
    public List<Dictionary<string, string>> SanctionsMatches { get; }
}