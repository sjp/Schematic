using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Lint;

/// <summary>
/// Contains pre-allocated 'empty' resources
/// </summary>
public static class Empty
{
    /// <summary>
    /// Retrieves an empty set of rule messages for general use.
    /// </summary>
    public static Task<IReadOnlyCollection<IRuleMessage>> RuleMessages { get; } = Task.FromResult<IReadOnlyCollection<IRuleMessage>>([]);
}