using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using SanctionsApi.Exceptions;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public class BuildSanctionsReport : IBuildSanctionsReport
{
    private readonly ReportContainer _reportContainer = new();
    private readonly IEnumerable<SanctionsListConfig> _sanctionsListConfigs;
    private IEnumerable<FullName> _fullNames = default!;
    private SanctionsListConfig _reportParams = default!;

    public BuildSanctionsReport(IOptionsMonitor<List<SanctionsListConfig>> sanctionsListConfigs)
    {
        _sanctionsListConfigs = sanctionsListConfigs.CurrentValue;
    }

    public async Task<ReportContainer> Execute(string[] name, string sanctionsList)
    {
        _fullNames = ExtractNamesFromQueryString(name);
        _reportParams = _sanctionsListConfigs.SingleOrDefault(x => x.Area == sanctionsList) ??
                        throw new ConfigIncorrectException("there was a problem reading the configuration");

        using var fileReader = new StreamReader(_reportParams.FileName, Encoding.GetEncoding(_reportParams.Encoding));
        using var parser = SetupCsvParser(fileReader);

        var i = 0;
        while (await parser.ReadAsync())
        {
            ProcessRow(parser.Record, ++i);
        }

        _reportContainer.Report.ResultSummary = MakeReportSummary(_reportContainer.Report.ResultSummary);

        return _reportContainer;
    }

    private static IEnumerable<FullName> ExtractNamesFromQueryString(IEnumerable<string> names)
    {
        return names.Select(MapNameToFullNameObject);
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
        if (rowIndex == 1 && row[0] == "Last Updated") // for uk sanctions check
            _reportContainer.Report.ResultSummary.SourceFileVersion = row[0] + ' ' + row[1];

        if (rowIndex == _reportParams.HeaderIndex)
            RecordHeaderFields(row);

        if (_fullNames.Any(fullName => IsFullNameInRow(fullName, row)))
        {
            AddRowToReport(row);
        }
    }

    private void RecordHeaderFields(IEnumerable<string> row)
    {
        foreach (var field in row)
            _reportParams.HeaderFields.Add(field);
    }

    private static bool IsFullNameInRow(FullName fullName, IEnumerable<string> row)
    {
        var countMatchedNames = row.SelectMany(r => r.Split(' '))
            .Distinct()
            .Join(fullName.Names,
                r => r.ToLower(),
                n => n.ToLower(),
                (r, _) => new { r })
            .Count();

        return countMatchedNames >= fullName.MaxAllowedCount;
    }

    private void AddRowToReport(IReadOnlyList<string> row)
    {
        var foundRecord = new Dictionary<string, string>();

        for (var i = 0; i < _reportParams.HeaderFields.Count; i++)
        {
            //TODO: this feels messy, cleaner way of doing this???
            var tempField = "";
            if (foundRecord.ContainsKey(_reportParams.HeaderFields[i]))
                tempField = "_" + i;
            if (i < row.Count)
                foundRecord.Add(_reportParams.HeaderFields[i] + tempField, row[i]);
        }

        _reportContainer.Report.SanctionsMatches.Add(foundRecord);
        _reportContainer.Report.ResultSummary.NumberOfResults++;
    }

    private static FullName MapNameToFullNameObject(string fullName)
    {
        var nameList = new FullName();
        var cleanedNames = fullName.Trim().ToLower().Split(' ').Select(n => n.Trim()).Where(n => n.Length > 0);
        var cleanedAndDeNoisedNames = cleanedNames.Where(DeNoiseName);
        nameList.Names.AddRange(cleanedAndDeNoisedNames);
        return nameList;
    }

    private static bool DeNoiseName(string s)
    {
        var commonWords = new[]
        {
            "of",
            "the",
            "and",
            "a",
            "an",
            "to",
            "at",
            "in",
            "on",
            "for",
            "by",
            "with",
            "from",
            "as",
            "is",
            "was",
            "were",
            "be",
            "been",
            "are",
            "were",
            "has",
            "had"
        };
        return !commonWords.Contains(s);
    }

    private ResultSummary MakeReportSummary(ResultSummary resultSummary)
    {
        resultSummary.ReportTitle = "Sanctions Check Report";
        resultSummary.SearchText = String.Join(",", _fullNames);
        resultSummary.SourceFileDownloadedDate = File.GetLastWriteTime(_reportParams.FileName).ToString();
        return resultSummary;
    }
}