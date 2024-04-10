using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SanctionsApi.Models;
using SanctionsApi.Services;

namespace SanctionsApiTest;

[TestFixture]
public class RegexNameMatcherTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<SimpleNameMatcher>>();
        _envMock = new Mock<IWebHostEnvironment>();
        _matcher = new RegexNameMatcher(_loggerMock.Object, _envMock.Object);
    }

    private RegexNameMatcher _matcher;
    private Mock<ILogger<SimpleNameMatcher>> _loggerMock;
    private Mock<IWebHostEnvironment> _envMock;

    [Test]
    [TestCase("B Limited", "B", true)]
    [TestCase("C Limited", "B", false)]
    public void Execute_FindsExpectedNameInRowData_ReturnsExpectedTrue(string nameInRow, string nameToTest, bool expectedResult)
    {
        // Arrange
        var fullNames = new List<FullName>();
        var fullName = new FullName();
        fullName.Names.Add(nameToTest);
        fullNames.Add(fullName);

        // "ABC Limited", 
        // "A&B Limited", 
        // "A Limited", 
        // "ACB Limited", 
        // "B & C Limited"

        var rowData = new List<string> { nameInRow };

        // Act
        var result = _matcher.Execute(fullNames, rowData);

        // Assert
        Assert.True(expectedResult == result);
    }
}