using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace SanctionsApi.Models {
    public class Report{
        //properties
        public ResultSummary resultSummary;
        public List<Dictionary<string, string>> record = new List<Dictionary<string, string>>();

        //constructors
        public Report() {
            resultSummary = new ResultSummary();
            record = new List<Dictionary<string,string>>();
        }

    }
}