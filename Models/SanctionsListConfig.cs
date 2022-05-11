using System.Collections.Generic;
using System.IO;

namespace SanctionsApi.Models;

public class SanctionsListConfig
{
    public string Area { get; init; }
    public string FileName { get; init; }
    public string Delimiter { get; init; }
    public int HeaderIndex { get; init; }
    public string Encoding { get; init; }
    public List<string> HeaderFields { get; } = new();

    public string SampleFileName => Path.Combine("SampleFiles", FileName.Substring(FileName.LastIndexOf("\\") + 1));
}