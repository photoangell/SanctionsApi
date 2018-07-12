using System;

namespace SanctionsApi.Models {
    class Report{
        //properties
        public ResultSummary resultSummary;
        //constructors
        public Report() {
            resultSummary = new ResultSummary();
        }
    }
}