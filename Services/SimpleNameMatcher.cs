using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public class SimpleNameMatcher : INameMatcher
{
    private readonly ILogger<SimpleNameMatcher> _logger;

    public SimpleNameMatcher(ILogger<SimpleNameMatcher> logger)
    {
        _logger = logger;
    }

    public bool Execute(IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        return fullNames.Any(fullName => IsFullNameInRow(fullName, row));
    }

    private bool IsFullNameInRow(FullName fullName, IEnumerable<string> row)
    {
        var countMatchedNames = row.SelectMany(r => r.Split(' '))
            .Select(n => new string(n.ToLower().Where(Char.IsLetterOrDigit).ToArray()))
            .Distinct()
            .Join(fullName.Names,
                r => r,
                n => n,
                (_, _) => true)
            .Count();

        if (countMatchedNames >= fullName.MaxAllowedCount)
        {
            _logger.LogDebug("Found {countMatchedNames} matches for fullname {fullName}", countMatchedNames, fullName);
            _logger.LogDebug("\tRow contents: {row}", row);
        }

        return countMatchedNames >= fullName.MaxAllowedCount;
    }
}