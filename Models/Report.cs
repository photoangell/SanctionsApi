using System.Collections.Generic;

namespace SanctionsApi.Models;

public class Report{
    public ResultSummary resultSummary {get; set;}
    public List<Dictionary<string, string>> record {get; }

    public Report() {
        resultSummary = new ResultSummary();
        record = new List<Dictionary<string,string>>();
    }
}