using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SanctionsApi.Exceptions;
using SanctionsApi.Models;

namespace SanctionsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class SanctionsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ReportContainer _reportContainer = new();
    private readonly IEnumerable<SanctionsListConfig> _sanctionsListConfigs;
    private IEnumerable<FullName> _fullNames = default!;
    private SanctionsListConfig _reportParams = default!;

    public SanctionsController(IWebHostEnvironment env, IOptionsMonitor<List<SanctionsListConfig>> sanctionsListConfigs)
    {
        _env = env;
        _sanctionsListConfigs = sanctionsListConfigs.CurrentValue;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] string[] name, string sanctionsList)
    {
        _fullNames = ExtractNamesFromQueryString(name);
        _reportParams = _sanctionsListConfigs.FirstOrDefault(x => x.Area == sanctionsList) ??
                        throw new ConfigIncorrectException("there was a problem reading the configuration");

        var file = _env.IsDevelopment() ? _reportParams.SampleFileName : _reportParams.FileName;
        using var fileReader = new StreamReader(file, Encoding.GetEncoding(_reportParams.Encoding));
        using var parser = SetupCsvParser(fileReader);

        var i = 0;
        while (await parser.ReadAsync())
        {
            ProcessRow(parser.Record, ++i);
        }

        _reportContainer.report.resultSummary = MakeReportSummary(_reportContainer.report.resultSummary);
        return Ok(_reportContainer);
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
            _reportContainer.report.resultSummary.version = row[0] + ' ' + row[1];

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
            .Join(fullName.Name,
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

        _reportContainer.report.record.Add(foundRecord);
        _reportContainer.report.resultSummary.numberOfResults++;
    }

    private static FullName MapNameToFullNameObject(string fullName)
    {
        var nameList = new FullName();
        var cleanedNames = fullName.Trim().ToLower().Split(' ').Select(n => n.Trim()).Where(n => n.Length > 0)
            .ToArray();
        nameList.Name.AddRange(cleanedNames);
        return nameList;
    }

    private ResultSummary MakeReportSummary(ResultSummary resultSummary)
    {
        resultSummary.title = "Sanctions Check Report";
        resultSummary.searchtext = String.Join(",", _fullNames);
        resultSummary.downloaded = System.IO.File.GetLastWriteTime(_reportParams.FileName).ToString();
        return resultSummary;
    }
}