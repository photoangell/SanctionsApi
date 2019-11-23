using System.Collections.Generic;

namespace SanctionsApi.Models
{
    public class Report{
        public ResultSummary resultSummary;
        public List<Dictionary<string, string>> record = new List<Dictionary<string, string>>();

        public Report() {
            resultSummary = new ResultSummary();
            record = new List<Dictionary<string,string>>();
        }

    }
}