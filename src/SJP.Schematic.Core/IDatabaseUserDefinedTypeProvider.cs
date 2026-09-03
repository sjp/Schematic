using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database user-defined type provider that retrieves type information for a database.
/// </summary>
public interface IDatabaseUserDefinedTypeProvider
{
    /// <summary>
    /// Gets a database user-defined type.
    /// </summary>
    /// <param name="typeName">A database type name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database user-defined type in the 'some' state if found; otherwise 'none'.</returns>
    OptionAsync<IDatabaseUserDefinedType> GetUserDefinedType(Identifier typeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all database user-defined types.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database user-defined types.</returns>
    IAsyncEnumerable<IDatabaseUserDefinedType> EnumerateAllUserDefinedTypes(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all database user-defined types, in parallel if possible.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database user-defined types.</returns>
    Task<IReadOnlyCollection<IDatabaseUserDefinedType>> GetAllUserDefinedTypes(CancellationToken cancellationToken = default);
}
