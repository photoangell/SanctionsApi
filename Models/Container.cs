namespace SanctionsApi.Models;

public class Container
{
    public Container()
    {
        report = new Report();
    }

    public Report report { get; }
}