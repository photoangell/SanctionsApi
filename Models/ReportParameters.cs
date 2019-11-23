using System.Collections.Generic;

namespace SanctionsApi.Models {
    public class ReportParameters {
        public List<string> HeaderFields { get; set; } = new List<string>();
        public int HeaderIndex { get; set; }
        public string Delimiter { get; set; }
        public string File { get; set; }
        public string Encoding { get; set; }
    }
}