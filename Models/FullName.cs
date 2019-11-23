using System;
using System.Collections.Generic;

namespace SanctionsApi.Models
{
    public class FullName {
        public List<string> Name {get; set;} = new List<string>();

        public override string ToString() {
            return String.Join(" ", Name);
        }
    }
}
