using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SanctionsApi.Exceptions;
using SanctionsApi.Models;

namespace SanctionsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class SanctionsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly Container _container = new();
    private readonly IWebHostEnvironment _env;
    private IEnumerable<FullName> _fullNames;
    private ReportParameter _reportParams = new();

    public SanctionsController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    [HttpGet]
    public Container Get()
    {
        _fullNames = ExtractNamesFromQueryString();
        _reportParams = ReadConfiguration(Request.Query["sanctionsList"]);

        using var fileReader = new StreamReader(_reportParams.FileName, Encoding.GetEncoding(_reportParams.Encoding));
        using var parser = SetupCsvParser(fileReader);

        var i = 0;
        while (parser.Read())
        {
            i++;
            var row = parser.Record;
            ProcessRow(row, i);
        }

        _container.report.resultSummary = MakeReportSummary(_container.report.resultSummary);
        return _container;
    }

    private ReportParameter ReadConfiguration(string region)
    {
        var rp = new ReportParameter();
        try
        {
            var configForRegion = _configuration.GetSection("SanctionLists").GetSection(region);
            rp.FileName = IfDevelopmentUseSampleFile(configForRegion.GetSection("FileName").Value);
            rp.Delimiter = configForRegion.GetSection("Delimiter").Value;
            rp.HeaderIndex = Int32.Parse(configForRegion.GetSection("HeaderIndex").Value);
            rp.Encoding = configForRegion.GetSection("Encoding").Value;
        }
        catch (Exception ex)
        {
            throw new ConfigIncorrectException("there was a problem reading the configuration", ex);
        }

        return rp;
    }

    private string IfDevelopmentUseSampleFile(string value)
    {
        return _env.IsDevelopment() ? Path.Combine("SampleFiles", value.Substring(value.LastIndexOf("\\") + 1)) : value;
    }

    private IEnumerable<FullName> ExtractNamesFromQueryString()
    {
        return Request.Query["name"].Select(SplitFullNameIntoList);
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
            _container.report.resultSummary.version = row[0] + ' ' + row[1];

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

        _container.report.record.Add(foundRecord);
        _container.report.resultSummary.numberOfResults++;
    }

    private static FullName SplitFullNameIntoList(string fullName)
    {
        var nameList = new FullName();
        foreach (var name in fullName.ToLower().Split(' '))
            nameList.Name.Add(name);
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