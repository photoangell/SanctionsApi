using System.Collections.Generic;
using System.Linq;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public class SimpleNameMatcher : ISimpleNameMatcher
{
    public bool Execute(IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        return fullNames.Any(fullName => IsFullNameInRow(fullName, row));
    }

    private static bool IsFullNameInRow(FullName fullName, IEnumerable<string> row)
    {
        var countMatchedNames = row.SelectMany(r => r.Split(' '))
            .Distinct()
            .Join(fullName.Names,
                r => r.ToLower(),
                n => n.ToLower(),
                (r, _) => new { r })
            .Count();

        return countMatchedNames >= fullName.MaxAllowedCount;
    }
}