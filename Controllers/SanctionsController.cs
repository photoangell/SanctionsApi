using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SanctionsApi.Models;
using CsvHelper;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace SanctionsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SanctionsController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private List<FullName> _fullNames = new List<FullName>();
        private Container _container = new Container();
        private ReportParameters _reportParams = new ReportParameters();

        public SanctionsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        public Container Get()
        {
            ExtractNamesFromQueryString();
            int counter = 0;
            string file = "";
            string delimiter = "";
            string encoding = "";
            int headerIndex = 0;

            var config = Configuration.AsEnumerable();
            foreach (KeyValuePair<string, string> kvp in config)
            {
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":FileName")
                {
                    file = System.IO.Directory.GetCurrentDirectory() + @"\" + kvp.Value;
                }
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":Delimiter")
                {
                    delimiter = kvp.Value;
                }
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":HeaderIndex")
                {
                    headerIndex = Int32.Parse(kvp.Value);
                }
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":Encoding")
                {
                    encoding = kvp.Value;
                }

            }

            using (StreamReader fileReader = new StreamReader(file, Encoding.GetEncoding("iso-8859-1")))
            {
                var parser = new CsvParser(fileReader);
                //csv.Configuration.HasHeaderRecord = false; 
                //csv.Configuration.MissingFieldFound = null;
                parser.Configuration.BadDataFound = null;
                parser.Configuration.Delimiter = delimiter;

                var i = 0;
                while (true)
                {
                    i++;
                    var row = parser.Read();
                    if (row == null) { break; }

                    if (i == 1 && row[0] == "Last Updated") // for uk sanctions check
                        _container.report.resultSummary.version = row[0] + ' ' + row[1];

                    if (i == headerIndex)
                        RecordHeaderFields(row);

                    foreach (var fullName in _fullNames)
                    {
                    if (IsFullNameInRow(fullName, row)) {
                            AddRowToReport(row);
                        break;
                    }
                }
            }

            _container.report.resultSummary.title = "Sanctions Check Report";
            _container.report.resultSummary.searchtext = String.Join(",", _fullNames);
            _container.report.resultSummary.downloaded = System.IO.File.GetLastWriteTime(file).ToString();
            return _container;
        }


        // private bool isNameInRecord(string[] record, FullName fullName, int maxAllowedScore) {
        //     var score = 0;
        //     var ignore = "";
        //     foreach (PropertyInfo prop in record.GetType().GetProperties())
        //     {
        //         var propNames = prop.GetValue(record, null).ToString().ToLower().Split(' ');
        //         foreach(var propName in propNames) {
        //             foreach (var name in fullName.Name) {
        //                 if (string.Equals(propName, name) && !ignore.Contains(propName)) {
        //                     score ++;               //mark match
        //                     ignore += name;     //pop name from array
        //                 }
        //             }
        //         }
        //     }
        //     if (score >= maxAllowedScore) {
        //         return true;
        //     }
        //     return false;
        // }

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
                var tempField = "";
                if (foundRecord.ContainsKey(_reportParams.HeaderFields[i]))
                    tempField = "_" + i.ToString();
                if (i < row.Length)
                    foundRecord.Add(_reportParams.HeaderFields[i] + tempField, row[i]);
            }
            _container.report.record.Add(foundRecord);
        }

        private void ExtractNamesFromQueryString()
        {
            foreach (var fullName in Request.Query["name"].ToList())
                _fullNames.Add(SplitFullNameIntoList(fullName));
        }

        private FullName SplitFullNameIntoList(string fullName)
        {
            var nameList = new FullName();
            foreach (var name in fullName.ToLower().Split(' '))
                nameList.Name.Add(name);
            return nameList;
        }

    }
}
