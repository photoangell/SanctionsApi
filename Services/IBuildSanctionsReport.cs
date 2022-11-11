using System.Threading.Tasks;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public interface IBuildSanctionsReport
{
    Task<ReportContainer> Execute(string[] name, string sanctionsList);
}