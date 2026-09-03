using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database user-defined type provider that returns no types. Not intended to be used directly.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedTypeProvider" />
public sealed class EmptyDatabaseUserDefinedTypeProvider : IDatabaseUserDefinedTypeProvider
{
    /// <summary>
    /// Enumerates all database user-defined types. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database user-defined types.</returns>
    public IAsyncEnumerable<IDatabaseUserDefinedType> EnumerateAllUserDefinedTypes(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IDatabaseUserDefinedType>();

    /// <summary>
    /// Gets all database user-defined types. This will always be an empty collection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An empty collection of database user-defined types.</returns>
    public Task<IReadOnlyCollection<IDatabaseUserDefinedType>> GetAllUserDefinedTypes(CancellationToken cancellationToken = default) => Empty.Tasks.UserDefinedTypes;

    /// <summary>
    /// Gets a database user-defined type. This will always be a 'none' result.
    /// </summary>
    /// <param name="typeName">A database type name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database user-defined type in the 'none' state.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseUserDefinedType> GetUserDefinedType(Identifier typeName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        return OptionAsync<IDatabaseUserDefinedType>.None;
    }
}
