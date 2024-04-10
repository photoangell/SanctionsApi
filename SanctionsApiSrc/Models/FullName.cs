using System;
using System.Collections.Generic;
using System.Linq;

namespace SanctionsApi.Models;

public class FullName
{
    public ICollection<string> Names { get; } = new List<string>();
    public int MaxAllowedCount => Names.Count > 2 ? 2 : Names.Count;
    public IEnumerable<string> NameParts => Names.SelectMany(name => name.Split(' '));

    public override string ToString()
    {
        return String.Join(" ", Names);
    }
}