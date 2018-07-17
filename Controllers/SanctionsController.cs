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
        [HttpGet("{searchString}")]
        public ObjectResult Get(string searchString)
        {
            int counter = 0;
            var container = new Container();
            container.report.resultSummary.searchtext = searchString;
            container.report.resultSummary.title = "Sanctions Check Report";
            container.report.resultSummary.downloaded = System.IO.File.GetLastWriteTime(@"C:\projects\legalcontingency\Sanctions\sanctionsconlist.csv").ToString();
            string[] names = searchString.ToLower().Split(new [] { '#' }, StringSplitOptions.RemoveEmptyEntries);

            //return string.Join(",", id);
            //string bums = id.ToString();
            //string bums = string.Join(",", id);
            
            using (TextReader fileReader = System.IO.File.OpenText(@"C:\projects\legalcontingency\Sanctions\sanctionsconlist.csv"))
            {
                var csv = new CsvReader(fileReader);
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
