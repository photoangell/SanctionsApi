using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SanctionsApi.Models;
using CsvHelper;
//using CsvHelper.Configuration;
using System.IO;
//using System.Text;
using System.Reflection;

namespace SanctionsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanctionsController : ControllerBase
    {
        // GET api/values
//        [HttpGet]
//        public ActionResult<IEnumerable<string>> Get()
//        {
//            return new string[] { "value1", "value2" };
//        }

        // GET api/values/5
        [HttpGet()]
        public ObjectResult Get()
        {
            var fullNames = HttpContext.Request.Query["name"].ToList();
            int counter = 0;
            var container = new Container();
            container.report.resultSummary.searchtext = string.Join( ",", fullNames );
            container.report.resultSummary.title = "Sanctions Check Report";
            // ** get version from header container.report.resultSummary.version will be <version>Last Updated 13/04/2018</version>
            container.report.resultSummary.downloaded = System.IO.File.GetLastWriteTime(@"C:\projects\legalcontingency\Sanctions\sanctionsconlist.csv").ToString();

            using (TextReader fileReader = System.IO.File.OpenText(@"C:\projects\legalcontingency\Sanctions\sanctionsconlist.csv"))
            {
                var csv = new CsvReader(fileReader);
                //** */check column match, log if not 28 columns (email to errors@)

                csv.Configuration.HasHeaderRecord = false; 
                csv.Configuration.MissingFieldFound = null;
                var allValues = csv.GetRecords<CSV>();
                
                foreach (var record in allValues)
                {
                // do your stuff   
                    foreach (var fullName in fullNames) {
                        var name = fullName.ToLower().Split(' ');
                        var maxAllowedScore = name.Length;
                        if (maxAllowedScore > 2) {maxAllowedScore = 2;}
                        if (isNameInRecord(record, name, maxAllowedScore)) {
                            counter++;  
                            record.recordnumber = counter.ToString();
                            container.report.record.Add(record);
                            //**check obecjt definition */
                        }
                    }
                }
            }
            //var records = csv.GetRecords<MyClass>();
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

        private bool isNameInRecord(CSV record, string[] name, int maxAllowedScore) {
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

    }
}
