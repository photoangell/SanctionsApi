using System;
using System.Collections;
using System.Collections.Generic;

namespace SanctionsApi.Models {
    public class Report{
        //properties
        public ResultSummary resultSummary;
        public List<CSV> record = new List<CSV>();

        //constructors
        public Report() {
            resultSummary = new ResultSummary();
            record = new List<CSV>();
        }

    }
}