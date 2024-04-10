using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public class ImprovedNameMatcher : INameMatcher

{
    private readonly ILogger<SimpleNameMatcher> _logger;
    private readonly IWebHostEnvironment _env;

    public ImprovedNameMatcher(ILogger<SimpleNameMatcher> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public bool Execute(IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        return fullNames.Any(fullName => IsFullNameInRow(fullName, row));
    }

    private bool IsFullNameInRow(FullName fullName, IEnumerable<string> row)
    {
        foreach (var item in row)
        {
            var words = item.Split(' ');

            var countMatchedNames = words.Intersect(fullName.NameParts, StringComparer.InvariantCultureIgnoreCase).Count();

            if (_env.IsDevelopment() && countMatchedNames >= fullName.MaxAllowedCount)
            {
                _logger.LogDebug("Found {countMatchedNames} matches for fullname {fullName}", countMatchedNames, fullName);
                _logger.LogDebug("\tRow contents: {item}", item);
            }

            if (countMatchedNames >= fullName.MaxAllowedCount)
            {
                return true;
            }
        }

        return false;
    }
}