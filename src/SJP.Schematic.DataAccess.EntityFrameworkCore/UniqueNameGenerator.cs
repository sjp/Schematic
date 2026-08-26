using System.Collections.Generic;
using System.Globalization;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore;

/// <summary>
/// Generates names that are unique within a scope, such as the members declared on a single class.
/// </summary>
internal static class UniqueNameGenerator
{
    /// <summary>
    /// Returns <paramref name="candidateName"/> when it has not already been used, otherwise a numerically suffixed variant of it. The returned name is added to <paramref name="usedNames"/>.
    /// </summary>
    /// <param name="usedNames">The names that have already been generated within the scope.</param>
    /// <param name="candidateName">The preferred name.</param>
    /// <returns>A name that is unique within <paramref name="usedNames"/>.</returns>
    public static string GenerateUniqueName(HashSet<string> usedNames, string candidateName)
    {
        // Terminates because each iteration tries a name that has not been tried before, and only finitely many are in use.
        var uniqueName = candidateName;
        for (var suffix = 1; !usedNames.Add(uniqueName); suffix++)
            uniqueName = candidateName + "_" + suffix.ToString(CultureInfo.InvariantCulture);

        return uniqueName;
    }
}
