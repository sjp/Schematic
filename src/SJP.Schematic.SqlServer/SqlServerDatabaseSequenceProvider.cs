using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A comment provider for SQL Server database sequences.
/// </summary>
/// <seealso cref="IDatabaseSequenceProvider" />
public class SqlServerDatabaseSequenceProvider : IDatabaseSequenceProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseSequenceProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public SqlServerDatabaseSequenceProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A type provider used to describe the type of the values a sequence generates.
    /// </summary>
    /// <value>A type provider.</value>
    protected IDbTypeProvider TypeProvider { get; } = new SqlServerDbTypeProvider();

    /// <summary>
    /// Enumerates all database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database sequences.</returns>
    public IAsyncEnumerable<IDatabaseSequence> EnumerateAllSequences(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllSequenceDefinitions.Result>(GetAllSequenceDefinitions.Sql, cancellationToken)
            .Select(row =>
            {
                var sequenceName = QualifySequenceName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.SequenceName));
                return BuildSequence(sequenceName, row);
            });
    }

    /// <summary>
    /// Gets all database sequences.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database sequences.</returns>
    public async Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default)
    {
        return await Connection.QueryEnumerableAsync<GetAllSequenceDefinitions.Result>(GetAllSequenceDefinitions.Sql, cancellationToken)
            .Select(row =>
            {
                var sequenceName = QualifySequenceName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.SequenceName));
                return BuildSequence(sequenceName, row);
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a database sequence.
    /// </summary>
    /// <param name="sequenceName">A database sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var candidateSequenceName = QualifySequenceName(sequenceName);
        return LoadSequence(candidateSequenceName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the sequence. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="sequenceName">A sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A sequence name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var candidateSequenceName = QualifySequenceName(sequenceName);
        var qualifiedSequenceName = Connection.QueryFirstOrNone(
            GetSequenceName.Sql,
            new GetSequenceName.Query { SchemaName = candidateSequenceName.Schema!, SequenceName = candidateSequenceName.LocalName },
            cancellationToken
        );

        return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSequenceName.Server, candidateSequenceName.Database, name.SchemaName, name.SequenceName));
    }

    /// <summary>
    /// Retrieves database sequence information.
    /// </summary>
    /// <param name="sequenceName">A database sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseSequence> LoadSequence(Identifier sequenceName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var candidateSequenceName = QualifySequenceName(sequenceName);
        return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
            .Bind(name => LoadSequenceData(name, cancellationToken));
    }

    private OptionAsync<IDatabaseSequence> LoadSequenceData(Identifier sequenceName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        return Connection.QueryFirstOrNone(
            GetSequenceDefinition.Sql,
            new GetSequenceDefinition.Query { SchemaName = sequenceName.Schema!, SequenceName = sequenceName.LocalName },
            cancellationToken
        ).Map<IDatabaseSequence>(row => BuildSequence(sequenceName, row));
    }

    private DatabaseSequence BuildSequence(Identifier sequenceName, ISequenceDefinitionRow row)
    {
        var typeMetadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(row.TypeSchemaName, row.TypeName),
            MaxLength = row.TypeMaxLength,
            NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
        };

        // sys.sequences reports a cached sequence with no size when CACHE was given without one,
        // in which case the engine picks a size and never says which
        var cacheMode = !row.IsCached
            ? SequenceCacheMode.None
            : row.CacheSize.HasValue
                ? SequenceCacheMode.Sized
                : SequenceCacheMode.EngineDefault;
        var cacheSize = row.IsCached && row.CacheSize.HasValue
            ? Option<int>.Some(row.CacheSize.Value)
            : Option<int>.None;

        return new DatabaseSequence(
            sequenceName,
            TypeProvider.CreateColumnType(typeMetadata),
            row.StartValue,
            row.Increment,
            Option<decimal>.Some(row.MinValue),
            Option<decimal>.Some(row.MaxValue),
            row.Cycle,
            cacheMode,
            cacheSize,
            // a SQL Server sequence is served by a single instance, so values are always ordered
            true
        );
    }

    /// <summary>
    /// Qualifies the name of the sequence.
    /// </summary>
    /// <param name="sequenceName">A view name.</param>
    /// <returns>A sequence name is at least as qualified as the given sequence name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected Identifier QualifySequenceName(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
    }
}