using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SanctionsApi.Models;
using CsvHelper;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using SanctionsApi.Exceptions;

namespace SanctionsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SanctionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private List<FullName> _fullNames = new List<FullName>();
        private Container _container = new Container();
        private ReportParameter _reportParams = new ReportParameter();

        public SanctionsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public Container Get()
        {
            ExtractNamesFromQueryString();
            ReadConfiguration(Request.Query["sanctionsList"]);

            using var fileReader = new StreamReader(_reportParams.FileName, Encoding.GetEncoding(_reportParams.Encoding));
            var parser = SetupCsvParser(fileReader);
            var row = parser.Read();

            for (var i = 1; row != null; i++)
            {
                ProcessRow(row, i);
                row = parser.Read();
            }

            WriteReportSummary();
            return _container;
        }

        private void ReadConfiguration(string region)
        {
            try
            {
                var configForRegion = _configuration.GetSection("SanctionLists").GetSection(region);
                _reportParams.FileName = configForRegion.GetSection("FileName").Value;
                _reportParams.Delimiter = configForRegion.GetSection("Delimiter").Value;
                _reportParams.HeaderIndex = int.Parse(configForRegion.GetSection("HeaderIndex").Value);
                _reportParams.Encoding = configForRegion.GetSection("Encoding").Value;
            }
            catch (Exception ex) {
                //throw new InvalidOperationException("there was a problem reading the configuration", ex);
                throw new ConfigIncorrectException("there was a problem reading the configuration", ex);
            }
        }

        private void ExtractNamesFromQueryString()
        {
            foreach (var fullName in Request.Query["name"].ToList())
                _fullNames.Add(SplitFullNameIntoList(fullName));
        }

        private CsvParser SetupCsvParser(StreamReader fileReader)
        {
            var parser = new CsvParser(fileReader);
            parser.Configuration.BadDataFound = null;
            parser.Configuration.Delimiter = _reportParams.Delimiter;
            return parser;
        }

        private void ProcessRow(string[] row, int rowIndex)
        {
            if (rowIndex == 1 && row[0] == "Last Updated") // for uk sanctions check
                _container.report.resultSummary.version = row[0] + ' ' + row[1];

            if (rowIndex == _reportParams.HeaderIndex)
                RecordHeaderFields(row);

            foreach (var fullName in _fullNames)
            {
                if (IsFullNameInRow(fullName, row))
                {
                    AddRowToReport(row);
                    break;
                }
            }
        }

        private void RecordHeaderFields(string[] row)
        {
            foreach (var field in row)
                _reportParams.HeaderFields.Add(field);
        }

        private bool IsFullNameInRow(FullName fullName, string[] row)
        {
            var countMatchedNames = row.SelectMany(r => r.Split(' '))
                .Distinct()
                .Join(fullName.Name,
                    r => r.ToLower(),
                    n => n.ToLower(),
                    (r, n) => new { r })
                .Count();

            return (countMatchedNames >= fullName.MaxAllowedCount);
        }

        private void AddRowToReport(string[] row)
        {
            _container.report.resultSummary.numberOfResults++;
            var foundRecord = new Dictionary<string, string>();

            for (var i = 0; i < _reportParams.HeaderFields.Count; i++)
            {
                //TODO: this feels messy, cleaner way of doing this???
                var tempField = "";
                if (foundRecord.ContainsKey(_reportParams.HeaderFields[i]))
                    tempField = "_" + i.ToString();
                if (i < row.Length)
                    foundRecord.Add(_reportParams.HeaderFields[i] + tempField, row[i]);
            }
            _container.report.record.Add(foundRecord);
        }

        private FullName SplitFullNameIntoList(string fullName)
        {
            var nameList = new FullName();
            foreach (var name in fullName.ToLower().Split(' '))
                nameList.Name.Add(name);
            return nameList;
        }

        private void WriteReportSummary()
        {
            _container.report.resultSummary.title = "Sanctions Check Report";
            _container.report.resultSummary.searchtext = String.Join(",", _fullNames);
            _container.report.resultSummary.downloaded = System.IO.File.GetLastWriteTime(_reportParams.FileName).ToString();
        }
    }
}