using System.Collections.Generic;
using System.Linq;

namespace SanctionsApi.Models;

public class FullName
{
    private static readonly HashSet<string> _commonWords = new()
    {
        "of",
        "the",
        "and",
        "a",
        "an",
        "to",
        "at",
        "in",
        "on",
        "for",
        "by",
        "with",
        "from",
        "as",
        "is",
        "was",
        "were",
        "be",
        "been",
        "are",
        "were",
        "has",
        "had",
        "limited",
        "ltd",
        "company",
        "plc"
    };

    public FullName()
    {
    }

    public FullName(string fullName)
    {
        Names = new List<string>();

        var cleanedNames = fullName.Trim().ToLower().Split(' ').Select(n => n.Trim()).Where(n => n.Length > 0);
        var cleanedAndDeNoisedNames = cleanedNames.Where(DeNoiseName);
        var cleanedDeNoisedAlphaNumericNames =
            cleanedAndDeNoisedNames.Select(n => new string(n.Where(char.IsLetterOrDigit).ToArray()));
        cleanedDeNoisedAlphaNumericNames.ToList().ForEach(Names.Add);
    }

    public ICollection<string> Names { get; } = new List<string>();
    public int MaxAllowedCount => Names.Count > 2 ? 2 : Names.Count;
    public IEnumerable<string> NameParts => Names.SelectMany(name => name.Split(' '));

    public override string ToString()
    {
        return string.Join(" ", Names);
    }

    private static bool DeNoiseName(string s)
    {
        return !_commonWords.Contains(s);
    }
}