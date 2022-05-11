namespace SanctionsApi.Models;

public class ResultSummary
{
    public string title { get; set; } = default!;
    public string version { get; set; } = default!;
    public string downloaded { get; set; } = default!;
    public string searchtext { get; set; } = default!;
    public int numberOfResults { get; set; }
}