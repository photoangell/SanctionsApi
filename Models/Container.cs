namespace SanctionsApi.Models
{
    public class Container{
        public Report report {get; set;}

        public Container() {
            report = new Report();
        }
    }
}