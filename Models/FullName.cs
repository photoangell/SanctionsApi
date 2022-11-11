using System;
using System.Collections.Generic;

namespace SanctionsApi.Models;

public class FullName
{
    public List<string> Names { get; } = new();
    public int MaxAllowedCount => Names.Count > 2 ? 2 : Names.Count;

    public override string ToString()
    {
        return String.Join(" ", Names);
    }
}