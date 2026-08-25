using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a table contains a row where a foreign key references the key it targets in the same row, ensuring that row cannot be deleted.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeySelfReferenceRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: information, because
    /// self-referencing keys are legitimate for hierarchies.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Information;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeySelfReferenceRule"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public ForeignKeySelfReferenceRule(ISchematicConnection connection, RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));

        _fromQuerySuffixAsync = new AsyncLazy<string>(GetFromQuerySuffixAsync);
    }

    /// <summary>
    /// A database connection.
    /// </summary>
    /// <value>The connection to the database.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>The database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// A database dialect.
    /// </summary>
    /// <value>The dialect associated with <see cref="DbConnection"/>.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Analyses database tables. Reports messages when a table contains a row where a foreign key references the same row.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return AnalyseTablesCore(tables, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTablesCore(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        var messages = await tables
            .Select(t => AnalyseTableAsync(t, cancellationToken))
            .ToArray()
            .WhenAll();

        return messages
            .SelectMany(_ => _)
            .ToArray();
    }

    /// <summary>
    /// Analyses a database table. Reports messages when the table contains a row where a foreign key references the same row.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IRuleMessage>> AnalyseTableAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return AnalyseTableCoreAsync(table, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTableCoreAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        var selfReferencingKeys = table.ParentKeys
            .Where(fk => fk.ParentTable == table.Name)
            .ToList();

        if (selfReferencingKeys.Count == 0)
            return [];

        var result = new List<IRuleMessage>();

        foreach (var relationalKey in selfReferencingKeys)
        {
            var isSelfReferencing = await TableHasSelfReferencingForeignKeyRowsAsync(table, relationalKey.ParentKey, relationalKey.ChildKey, cancellationToken);
            if (isSelfReferencing)
            {
                var message = BuildMessage(table.Name, relationalKey.ParentKey, relationalKey.ChildKey);
                result.Add(message);
            }
        }

        return result;
    }

    private Task<bool> TableHasSelfReferencingForeignKeyRowsAsync(IRelationalDatabaseTable table, IDatabaseKey targetKey, IDatabaseKey foreignKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(targetKey);
        ArgumentNullException.ThrowIfNull(foreignKey);

        return TableHasSelfReferencingForeignKeyRowsCore(table, targetKey, foreignKey, cancellationToken);
    }

    private async Task<bool> TableHasSelfReferencingForeignKeyRowsCore(IRelationalDatabaseTable table, IDatabaseKey targetKey, IDatabaseKey foreignKey, CancellationToken cancellationToken)
    {
        var targetColumnNames = targetKey.Columns.Select(c => c.Name).ToList();
        var fkColumnNames = foreignKey.Columns.Select(c => c.Name).ToList();

        var sql = await GetTableMatchingForeignKeyTargetKeyQueryCore(
            table.Name,
            targetColumnNames,
            fkColumnNames
        );
        return await DbConnection.ExecuteScalarAsync<bool>(sql, cancellationToken);
    }

    private async Task<string> GetTableMatchingForeignKeyTargetKeyQueryCore(Identifier tableName, IEnumerable<Identifier> targetColumnNames, IEnumerable<Identifier> fkColumnNames)
    {
        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName));
        var quotedTargetKeyColumnNames = targetColumnNames.Select(n => Dialect.QuoteIdentifier(n.LocalName)).ToList();
        var quotedForeignKeyColumnNames = fkColumnNames.Select(n => Dialect.QuoteIdentifier(n.LocalName)).ToList();

        var equalsClauses = quotedTargetKeyColumnNames.Zip(
                quotedForeignKeyColumnNames,
                (targetCol, fkCol) =>
                {
                    var nullComparison = "(" + targetCol + " IS NULL AND " + fkCol + " IS NULL)";
                    var valueComparison = "(" + targetCol + " = " + fkCol + ")";

                    return "(" + nullComparison + " OR " + valueComparison + ")";
                }
            ).ToList();
        var whereFilterClauses = equalsClauses.Join(" AND ");

        var filterSql = $@"
select 1
from {quotedTableName}
where {whereFilterClauses}
";
        var sql = $"select case when exists ({filterSql}) then 1 else 0 end as dummy";

        var suffix = await _fromQuerySuffixAsync;
        return suffix.IsNullOrWhiteSpace()
            ? sql
            : sql + " from " + suffix;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="targetKey">The key in the table which the foreign key refers to.</param>
    /// <param name="foreignKey">The self-referencing foreign key.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/>, <paramref name="targetKey"/> or <paramref name="foreignKey"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, IDatabaseKey targetKey, IDatabaseKey foreignKey)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(targetKey);
        ArgumentNullException.ThrowIfNull(foreignKey);

        var targetKeyColumnNames = targetKey.Columns.Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var targetKeyNameSuffix = targetKey.Name.Match(
            targetKeyName => Dialect.QuoteName(targetKeyName) + " ",
            () => string.Empty
        );
        var targetKeyMessage = $"{GetKeyTypeDescription(targetKey.KeyType)} {targetKeyNameSuffix}({targetKeyColumnNames.Join(", ")})";

        var foreignKeyColumnNames = foreignKey.Columns.Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var fkNameSuffix = foreignKey.Name.Match(
            fkName => Dialect.QuoteName(fkName) + " ",
            () => string.Empty
        );
        var foreignKeyMessage = $"foreign key {fkNameSuffix}({foreignKeyColumnNames.Join(", ")})";

        var messageText = $"The table '{tableName}' contains a row where the {foreignKeyMessage} self-references the {targetKeyMessage}. Consider removing the row by removing the foreign key first, then reintroducing after row removal.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Describes a key in the manner it is referred to in a linting message.
    /// </summary>
    /// <param name="keyType">The type of a key.</param>
    /// <returns>A description of the key type.</returns>
    protected static string GetKeyTypeDescription(DatabaseKeyType keyType)
    {
        return keyType switch
        {
            DatabaseKeyType.Primary => "primary key",
            DatabaseKeyType.Unique => "unique key",
            DatabaseKeyType.Foreign => "foreign key",
            _ => "key"
        };
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0024";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Table contains a row where a foreign key self-references the key it targets in the same row.";

    private async Task<string> GetFromQuerySuffixAsync()
    {
        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryNoTable, CancellationToken.None);
            return string.Empty;
        }
        catch
        {
            // Deliberately ignoring because we are testing functionality
        }

        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromSysDual, CancellationToken.None);
            return "SYS.DUAL";
        }
        catch
        {
            // Deliberately ignoring because we are testing functionality
        }

        _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromDual, CancellationToken.None);
        return "DUAL";
    }

    private const string TestQueryNoTable = "select 1 as dummy";
    private const string TestQueryFromDual = "select 1 as dummy from DUAL";
    private const string TestQueryFromSysDual = "select 1 as dummy from SYS.DUAL";

    private readonly AsyncLazy<string> _fromQuerySuffixAsync;
}