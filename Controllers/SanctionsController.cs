using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SanctionsApi.Services;

namespace SanctionsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class SanctionsController : ControllerBase
{
    private readonly IBuildSanctionsReport _buildSanctionsReport;
    
    public SanctionsController(IBuildSanctionsReport buildSanctionsReport)
    {
        _buildSanctionsReport = buildSanctionsReport;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string[] name, string sanctionsList)
    {
        var reportContainer = await _buildSanctionsReport.Execute(name, sanctionsList);
        return Ok(reportContainer);
    }
}