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
            var names = HttpContext.Request.Query["name"].ToList();
            int counter = 0;
            var container = new Container();
            container.report.resultSummary.searchtext = string.Join( ",", names.ToArray() );
            container.report.resultSummary.title = "Sanctions Check Report";
            // ** get version from header container.report.resultSummary.version will be <version>Last Updated 13/04/2018</version>
            container.report.resultSummary.downloaded = System.IO.File.GetLastWriteTime(@"C:\projects\legalcontingency\Sanctions\sanctionsconlist.csv").ToString();

            using (TextReader fileReader = System.IO.File.OpenText(@"C:\projects\legalcontingency\Sanctions\sanctionsconlist.csv"))
            {
                var csv = new CsvReader(fileReader);
                //** */check column match, log if not 28 columns (email to errors@)

                //csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.MissingFieldFound = null;
                var allValues = csv.GetRecords<CSV>();
                foreach (var record in allValues)
                {
                // do your stuff   
                    foreach (var name in names) {
                        if (isNameInRecord(record, name)) {
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

        private bool isNameInRecord(CSV record, string name) {
            if (record.Name1.ToLower() == name) {return true;}
            if (record.Name2.ToLower() == name) {return true;}
            if (record.Name3.ToLower() == name) {return true;}
            if (record.Name4.ToLower() == name) {return true;}
            if (record.Name5.ToLower() == name) {return true;}
            if (record.Name6.ToLower() == name) {return true;}
            return false;
        }

    }
}
