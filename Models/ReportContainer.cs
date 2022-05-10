namespace SanctionsApi.Models;

public class ReportContainer
{
    public ReportContainer()
    {
        report = new Report();
    }

    public Report report { get; }
}