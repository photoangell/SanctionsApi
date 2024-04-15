using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SanctionsApi.Models;
using SanctionsApi.Services;

namespace SanctionsApiTest;

[TestFixture]
public class NameMatcherTests
{
    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<MarksRegexMatcher>>();
        _envMock = new Mock<IWebHostEnvironment>();
        _matcher = new MarksRegexMatcher(_loggerMock.Object, _envMock.Object);
    }

    private INameMatcher _matcher;
    private Mock<ILogger<MarksRegexMatcher>> _loggerMock;
    private Mock<IWebHostEnvironment> _envMock;

    [Test]
    [TestCase("B Limited", "B", true)]
    [TestCase("C Limited", "B", false)]
    [TestCase("ABC Limited", "B Limited", false)]
    [TestCase("B & C Limited", "B & C Limited", true)]
    [TestCase("Bob Limited", "B & C Limited", false)]
    [TestCase("Bob", "Bob Limited", true)]
    [TestCase("B&C Limited", "B & C Limited", true)]
    public void Execute_FindsExpectedNameInRowData_ReturnsExpectedTrue(string nameInRow, string nameToTest, bool expectedResult)
    {
        // Arrange
        var fullNames = new List<FullName>();
        var fullName = new FullName();
        var newFullName = fullName.MapNameToFullNameObject(nameToTest);
        fullNames.Add(newFullName);
        
        var rowData = new List<string> { nameInRow };

        // Act
        var result = _matcher.Execute(fullNames, rowData);

        // Assert
        Assert.That(expectedResult, Is.EqualTo(result));
    }
}