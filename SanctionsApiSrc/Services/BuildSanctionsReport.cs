using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SanctionsApi.Exceptions;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public class BuildSanctionsReport : IBuildSanctionsReport
{
    private readonly ILogger<BuildSanctionsReport> _logger;
    private readonly INameMatcher _nameMatcher;
    private readonly ReportContainer _reportContainer = new();
    private readonly ISanctionsDataLoader _sanctionsDataLoader;
    private readonly IEnumerable<SanctionsListConfig> _sanctionsListConfigs;
    private IEnumerable<FullName> _fullNames = default!;
    private SanctionsListConfig _reportParams = default!;

    public BuildSanctionsReport(ILogger<BuildSanctionsReport> logger,
        IOptionsMonitor<List<SanctionsListConfig>> sanctionsListConfigs, INameMatcher nameMatcher,
        ISanctionsDataLoader sanctionsDataLoader)
    {
        _logger = logger;
        _nameMatcher = nameMatcher;
        _sanctionsDataLoader = sanctionsDataLoader;
        _sanctionsListConfigs = sanctionsListConfigs.CurrentValue;
    }

    public async Task<ReportContainer> Execute(IEnumerable<string> names, string sanctionsList)
    {
        _fullNames = ExtractNamesFromQueryString(names);
        _logger.LogInformation("Extracted {FullNameCount} full names from query string", _fullNames.Count());
        _logger.LogInformation("Full names: {@FullNames}", _fullNames);

        _reportParams = _sanctionsListConfigs.SingleOrDefault(x => x.Area == sanctionsList) ??
                        throw new ConfigIncorrectException("there was a problem reading the configuration");

        using var fileReader = new StreamReader(_reportParams.FileName, Encoding.GetEncoding(_reportParams.Encoding));
        using var parser = SetupCsvParser(fileReader);

        var i = 0;
        while (await parser.ReadAsync()) ProcessRow(parser.Record!, ++i);

        _reportContainer.Report.ResultSummary = MakeReportSummary(_reportContainer.Report.ResultSummary);

        return _reportContainer;
    }

    private static IEnumerable<FullName> ExtractNamesFromQueryString(IEnumerable<string> names)
    {
        return names.Select(name => new FullName(name));
    }

    private CsvParser SetupCsvParser(TextReader fileReader)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = _reportParams.Delimiter,
            BadDataFound = null
        };

        return new CsvParser(fileReader, config);
    }

    private void ProcessRow(IReadOnlyList<string> row, int rowIndex)
    {
        if (row == null) return;

        if (rowIndex == _reportParams.MetaDataIndex)
            _reportContainer.Report.ResultSummary.MetaData = string.Join(" ", row).Replace(" ", "");

        if (rowIndex == _reportParams.HeaderIndex)
            _reportParams.HeaderFields.AddRange(row);

        var columnsToRead = row.Skip(_reportParams.StartColumn)
            .Take(_reportParams.EndColumn + 1 - _reportParams.StartColumn)
            .ToList();
        if (_nameMatcher.Execute(_fullNames, columnsToRead)) AddRowToReport(row);
    }

    private void AddRowToReport(IReadOnlyList<string> row)
    {
        var foundRecord = new Dictionary<string, string>();

        for (var i = 0; i < row.Count; i++)
        {
            var headerField = i >= _reportParams.HeaderFields.Count
                ? "Field " + (i + 1)
                : _reportParams.HeaderFields[i];
            if (foundRecord.ContainsKey(headerField))
                headerField += "_" + i;
            if (i < row.Count)
                foundRecord.Add(headerField, row[i]);
        }

        _reportContainer.Report.SanctionsMatches.Add(foundRecord);
        _reportContainer.Report.ResultSummary.NumberOfResults++;
    }

    private ResultSummary MakeReportSummary(ResultSummary resultSummary)
    {
        resultSummary.SearchText = string.Join(",", _fullNames);
        resultSummary.SourceFileDownloadedDate = File.GetLastWriteTime(_reportParams.FileName).ToString();
        return resultSummary;
    }
}