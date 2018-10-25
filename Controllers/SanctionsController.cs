using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SanctionsApi.Models;
using CsvHelper;
//using CsvHelper.Configuration;
using System.IO;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.Extensions.Configuration;

namespace SanctionsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SanctionsController : ControllerBase
    {
        private readonly IConfiguration Configuration;

        public SanctionsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IEnumerable<KeyValuePair<string, string>> FilteredConfiguration { get; private set; }

        // GET api/values/5
        [HttpGet()]
        [Produces("application/json")]
        public ObjectResult Get()
        {
            List<String> fullNames = HttpContext.Request.Query["name"].ToList();
            int counter = 0;
            string file = "";
            string delimiter = "";
            string encoding = "";
            int headerIndex = 0;
            Container container = new Container();
 
            var config = Configuration.AsEnumerable();
             foreach (KeyValuePair<string,string> kvp in config) {
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":FileName") {
                    file = System.IO.Directory.GetCurrentDirectory() + @"\" + kvp.Value;
                }
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":Delimiter") {
                    delimiter = kvp.Value;
                }
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":HeaderIndex") {
                    headerIndex = Int32.Parse(kvp.Value);
                }
                if (kvp.Key == "SanctionLists:" + HttpContext.Request.Query["sanctionsList"] + ":Encoding") {
                    encoding = kvp.Value;
                }
                
            }
            container.report.resultSummary.searchtext = string.Join( ",", fullNames );
            container.report.resultSummary.title = "Sanctions Check Report";
            container.report.resultSummary.downloaded = System.IO.File.GetLastWriteTime(file).ToString();
            
            using (StreamReader fileReader = new StreamReader(file, Encoding.GetEncoding("iso-8859-1")))
            {
                var parser = new CsvParser( fileReader );
                //csv.Configuration.HasHeaderRecord = false; 
                //csv.Configuration.MissingFieldFound = null;
                parser.Configuration.BadDataFound = null;
                parser.Configuration.Delimiter = delimiter;
                
                var i = 0;
                var headerFields = new List<string>();
                while( true )
                {
                    i++;
                    var row = parser.Read();
                    if (row == null) {break;}

                    if (i == 1 && row[0] == "Last Updated") {  // for uk sanctions check
                        container.report.resultSummary.version = row[0] + ' ' + row[1];
                    }

                    if (i == headerIndex ) {
                        //get header values
                        foreach (var field in row) {
                            headerFields.Add(field);
                        }
                    }
                    
                    foreach (var fullName in fullNames) {
                        var name = fullName.ToLower().Split(' ');
                        var maxAllowedScore = name.Length;
                        if (maxAllowedScore > 2) {maxAllowedScore = 2;}
                        if (isNameInRecordStringArray(row, name, maxAllowedScore)) {
                            counter++;  
                            Dictionary<string, string> foundRecord = new Dictionary<string, string>();
                            
                            var headerFieldsCount = headerFields.Count;
                            for (var i2 = 0; i2 < headerFieldsCount; i2++) {
                                var tempField = "";
                                if (foundRecord.ContainsKey(headerFields[i2])) {
                                    tempField = "_" + i2.ToString();
                                }
                                if (i2 <= row.Length) {
                                    foundRecord.Add(headerFields[i2] + tempField, row[i2]);
                                }
                            }
                            container.report.record.Add(foundRecord);
                        }
                    }
                }
            }
            container.report.resultSummary.numberOfResults = counter;
            return new ObjectResult(container);
        }

        // POST api/values
//        [HttpPost]
//        public void Post([FromBody] string value)
//        {
//        }

        // PUT api/values/5
//        [HttpPut("{id}")]
//       public void Put(int id, [FromBody] string value)
//        {
//        }

        // DELETE api/values/5
//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }

        // GET api/values
//        [HttpGet]
//        public ActionResult<IEnumerable<string>> Get()
//        {
//            return new string[] { "value1", "value2" };
//        }

        private bool isNameInRecord(string[] record, string[] name, int maxAllowedScore) {
            var score = 0;
            var ignore = "";
            foreach (PropertyInfo prop in record.GetType().GetProperties())
            {
                var propNames = prop.GetValue(record, null).ToString().ToLower().Split(' ');
                foreach(var propName in propNames) {
                    foreach (var namePart in name) {
                        if (string.Equals(propName, namePart) && !ignore.Contains(propName)) {
                            score ++;               //mark match
                            ignore += namePart;     //pop name from array
                        }
                    }
                }
            }
            if (score >= maxAllowedScore) {
                return true;
            }
            return false;
        }
        private bool isNameInRecordStringArray(string[] record, string[] name, int maxAllowedScore) {
            var score = 0;
            var ignore = "";
            foreach (var field in record)
            {
                var fieldWords = field.ToString().ToLower().Split(' ');
                foreach(var fieldWord in fieldWords) {
                    foreach (var namePart in name) {
                        if (string.Equals(fieldWord, namePart) && !ignore.Contains(namePart)) {
                            score ++;               //mark match
                            ignore += namePart;     //pop name from array
                        }
                    }
                }
            }
            if (score >= maxAllowedScore) {
                return true;
            }
            return false;
        }

    }
}
