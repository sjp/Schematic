﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a table contains a row where the foreign key references the primary key for the same row, ensuring that row cannot be deleted.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeySelfReferenceRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeySelfReferenceRule"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="level">The reporting level.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
    public ForeignKeySelfReferenceRule(ISchematicConnection connection, RuleLevel level)
        : base(RuleId, RuleTitle, level)
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
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

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
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return AnalyseTablesCore(tables, cancellationToken);
    }

    private async IAsyncEnumerable<IRuleMessage> AnalyseTablesCore(IEnumerable<IRelationalDatabaseTable> tables, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var table in tables)
        {
            var messages = await AnalyseTableAsync(table, cancellationToken).ConfigureAwait(false);
            foreach (var message in messages)
                yield return message;
        }
    }

    /// <summary>
    /// Analyses a database table. Reports messages when the table contains a row where a foreign key references the same row.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
    protected Task<IEnumerable<IRuleMessage>> AnalyseTableAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return table.PrimaryKey.Match(
            pk => AnalyseTableWithPrimaryKeyAsync(table, pk, cancellationToken),
            () => Task.FromResult<IEnumerable<IRuleMessage>>([])
        );
    }

    private async Task<IEnumerable<IRuleMessage>> AnalyseTableWithPrimaryKeyAsync(IRelationalDatabaseTable table, IDatabaseKey primaryKey, CancellationToken cancellationToken)
    {
        var matchingForeignKeys = table.ParentKeys
            .Where(fk => fk.ParentTable == table.Name)
            .Select(fk => fk.ChildKey)
            .ToList();

        if (matchingForeignKeys.Count == 0)
            return [];

        var result = new List<IRuleMessage>();

        foreach (var foreignKey in matchingForeignKeys)
        {
            var isSelfReferencing = await TableHasSelfReferencingForeignKeyRowsAsync(table, primaryKey, foreignKey, cancellationToken).ConfigureAwait(false);
            if (isSelfReferencing)
            {
                var message = BuildMessage(table.Name, primaryKey, foreignKey);
                result.Add(message);
            }
        }

        return result;
    }

    private Task<bool> TableHasSelfReferencingForeignKeyRowsAsync(IRelationalDatabaseTable table, IDatabaseKey primaryKey, IDatabaseKey foreignKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(primaryKey);
        ArgumentNullException.ThrowIfNull(foreignKey);

        return TableHasSelfReferencingForeignKeyRowsCore(table, primaryKey, foreignKey, cancellationToken);
    }

    private async Task<bool> TableHasSelfReferencingForeignKeyRowsCore(IRelationalDatabaseTable table, IDatabaseKey primaryKey, IDatabaseKey foreignKey, CancellationToken cancellationToken)
    {
        var pkColumnNames = primaryKey.Columns.Select(c => c.Name).ToList();
        var fkColumnNames = foreignKey.Columns.Select(c => c.Name).ToList();

        var sql = await GetTableMatchingForeignKeyPrimaryKeyQueryCore(
            table.Name,
            pkColumnNames,
            fkColumnNames
        ).ConfigureAwait(false);
        return await DbConnection.ExecuteScalarAsync<bool>(sql, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> GetTableMatchingForeignKeyPrimaryKeyQueryCore(Identifier tableName, IEnumerable<Identifier> pkColumnNames, IEnumerable<Identifier> fkColumnNames)
    {
        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName));
        var quotedPrimaryKeyColumnNames = pkColumnNames.Select(n => Dialect.QuoteIdentifier(n.LocalName)).ToList();
        var quotedForeignKeyColumnNames = fkColumnNames.Select(n => Dialect.QuoteIdentifier(n.LocalName)).ToList();

        var equalsClauses = quotedPrimaryKeyColumnNames.Zip(
                quotedForeignKeyColumnNames,
                (pkCol, fkCol) =>
                {
                    var nullComparison = "(" + pkCol + " IS NULL AND " + fkCol + " IS NULL)";
                    var valueComparison = "(" + pkCol + " = " + fkCol + ")";

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

        var suffix = await _fromQuerySuffixAsync.ConfigureAwait(false);
        return suffix.IsNullOrWhiteSpace()
            ? sql
            : sql + " from " + suffix;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="primaryKey">The primary key for the table.</param>
    /// <param name="foreignKey">The self-referencing foreign key.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/>, <paramref name="primaryKey"/> or <paramref name="foreignKey"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, IDatabaseKey primaryKey, IDatabaseKey foreignKey)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(primaryKey);
        ArgumentNullException.ThrowIfNull(foreignKey);

        var primaryKeyColumnNames = primaryKey.Columns.Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var pkNameSuffix = primaryKey.Name.Match(
            pkName => Dialect.QuoteName(pkName) + " ",
            () => string.Empty
        );
        var primaryKeyMessage = $"primary key {pkNameSuffix}({primaryKeyColumnNames.Join(", ")})";

        var foreignKeyColumnNames = foreignKey.Columns.Select(c => Dialect.QuoteIdentifier(c.Name.LocalName));
        var fkNameSuffix = foreignKey.Name.Match(
            fkName => Dialect.QuoteName(fkName) + " ",
            () => string.Empty
        );
        var foreignKeyMessage = $"foreign key {fkNameSuffix}({foreignKeyColumnNames.Join(", ")})";

        var messageText = $"The table '{tableName}' contains a row where the {foreignKeyMessage} self-references the {primaryKeyMessage}. Consider removing the row by removing the foreign key first, then reintroducing after row removal.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0024";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Table contains a row where a foreign key self-references the primary key of the same row.";

    private async Task<string> GetFromQuerySuffixAsync()
    {
        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryNoTable, CancellationToken.None).ConfigureAwait(false);
            return string.Empty;
        }
        catch
        {
            // Deliberately ignoring because we are testing functionality
        }

        try
        {
            _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromSysDual, CancellationToken.None).ConfigureAwait(false);
            return "SYS.DUAL";
        }
        catch
        {
            // Deliberately ignoring because we are testing functionality
        }

        _ = await DbConnection.ExecuteScalarAsync<bool>(TestQueryFromDual, CancellationToken.None).ConfigureAwait(false);
        return "DUAL";
    }

    private const string TestQueryNoTable = "select 1 as dummy";
    private const string TestQueryFromDual = "select 1 as dummy from DUAL";
    private const string TestQueryFromSysDual = "select 1 as dummy from SYS.DUAL";

    private readonly AsyncLazy<string> _fromQuerySuffixAsync;
}