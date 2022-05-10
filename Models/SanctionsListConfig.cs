using System.Collections.Generic;
using System.IO;

namespace SanctionsApi.Models;

public class SanctionsListConfig
{
    public string Area { get; set; }
    public string FileName { get; set; }
    public string Delimiter { get; set; }
    public int HeaderIndex { get; set; }
    public string Encoding { get; set; }
    public List<string> HeaderFields { get; } = new();

    public string SampleFileName => Path.Combine("SampleFiles", FileName.Substring(FileName.LastIndexOf("\\") + 1));
}