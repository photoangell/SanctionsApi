using System.Collections.Generic;

namespace SanctionsApi.Models;

public class SanctionsListConfig
{
    public string Area { get; init; } = default!;
    public string FileName { get; init; } = default!;
    public string Delimiter { get; init; } = default!;
    public int HeaderIndex { get; init; }
    public string Encoding { get; init; } = "iso-8859-1";
    public List<string> HeaderFields { get; } = new();
}