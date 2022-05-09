namespace SanctionsApi.Models;

public class Container{
    public Report report {get; }

    public Container() {
        report = new Report();
    }
}