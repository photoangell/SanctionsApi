using System.Collections.Generic;

namespace SanctionsApi.Models {
    public class ReportParameter {
        public string FileName { get; set; }
        public string Delimiter { get; set; }
        public int HeaderIndex { get; set; }
        public string Encoding { get; set; }
        public List<string> HeaderFields { get; set; } = new List<string>();
    }
}