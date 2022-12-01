using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SanctionsApi.Models;

public class Report
{
    public Report()
    {
        ResultSummary = new ResultSummary();
        SanctionsMatches = new List<IDictionary<string, string>>();
    }

    [JsonPropertyName("resultSummary")]
    public ResultSummary ResultSummary { get; set; }

    [JsonPropertyName("record")]
    public ICollection<IDictionary<string, string>> SanctionsMatches { get; }
}