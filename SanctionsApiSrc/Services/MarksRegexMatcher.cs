using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public class MarksRegexMatcher : INameMatcher
{
    private readonly ILogger<MarksRegexMatcher> _logger;
    private readonly IWebHostEnvironment _env;

    public MarksRegexMatcher(ILogger<MarksRegexMatcher> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public bool Execute(IEnumerable<FullName> fullNames, IReadOnlyList<string> row)
    {
        return fullNames.Any(fullName => IsFullNameInRow(fullName, row));
    }

    private bool IsFullNameInRow(FullName fullName, IReadOnlyList<string> row)
    {
        foreach (var word in row)
        {
            if (MatchWholeWord(word, fullName.ToString()))
            {
                return true;
            }
        }

        return false;
    }

    static bool MatchWholeWord(string word, string letter)
    {
        var pattern = @"\b(?<!\w)" + letter + @"(?!\w)\b";

        return Regex.IsMatch(word, pattern, RegexOptions.IgnoreCase);
    }
}