using System.Collections.Generic;
using System.Threading.Tasks;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public interface IBuildSanctionsReport
{
    Task<ReportContainer> Execute(IEnumerable<string> names, string sanctionsList);
}