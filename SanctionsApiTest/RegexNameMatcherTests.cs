using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SanctionsApi.Models;
using SanctionsApi.Services;

namespace SanctionsApiTest;

[TestFixture]
public class RegexNameMatcherTests
{
    private RegexNameMatcher _matcher;
    private Mock<ILogger<SimpleNameMatcher>> _loggerMock;
    private Mock<IWebHostEnvironment> _envMock;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<SimpleNameMatcher>>();
        _envMock = new Mock<IWebHostEnvironment>();
        _matcher = new RegexNameMatcher(_loggerMock.Object, _envMock.Object);
    }

    [Test]
    public void Execute_FindsExpectedNameInRowData_ReturnsTrue()
    {
        // Arrange
        var fullNames = new List<FullName>()
        {
            new FullName()
            {
                NameParts = new List<string>{ "B", "Limited" },
                MaxAllowedCount = 2
            }
        };
        var rowData = new List<string>()
        {
            "ABC Limited", 
            "A&B Limited", 
            "A Limited", 
            "B Limited", 
            "ACB Limited", 
            "B & C Limited"
        };
          
        // Act
        var result = _matcher.Execute(fullNames, rowData);
        
        // Assert
        Assert.True(result);
    }
}