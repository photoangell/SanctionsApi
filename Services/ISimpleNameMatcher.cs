using System.Collections.Generic;
using SanctionsApi.Models;

namespace SanctionsApi.Services;

public interface ISimpleNameMatcher
{
    bool Execute(IEnumerable<FullName> fullNames, IReadOnlyList<string> row);
}