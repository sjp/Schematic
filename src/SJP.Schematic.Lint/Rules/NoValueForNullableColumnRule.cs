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
/// A linting rule which reports when no non-null values exist for a nullable column in a table.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class NoValueForNullableColumnRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoValueForNullableColumnRule"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="level">The reporting level.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public NoValueForNullableColumnRule(ISchematicConnection connection, RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));

        _fromQuerySuffixAsync = new AsyncLazy<string>(GetFromQuerySuffixAsync);
    }

    /// <summary>
    /// A database connection, qualified with a dialect.
    /// </summary>
    /// <value>The connection.</value>
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
    /// Analyses database tables. Reports messages when no non-null values exist for a nullable column in a table.
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

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTablesCore(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
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
    /// Analyses a database table. Reports messages when no non-null values exist for a nullable column in a table.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IRuleMessage>> AnalyseTableAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return AnalyseTableAsyncCore(table, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTableAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        var nullableColumns = table.Columns.Where(c => c.IsNullable).ToList();
        if (nullableColumns.Empty())
            return [];

        var tableHasRows = await TableHasRowsAsync(table, cancellationToken);
        if (!tableHasRows)
            return [];

        var result = new List<IRuleMessage>();

        foreach (var nullableColumn in nullableColumns)
        {
            var hasValue = await NullableColumnHasValueAsync(table, nullableColumn, cancellationToken);
            if (hasValue)
                continue;

            var message = BuildMessage(table.Name, nullableColumn.Name.LocalName);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Determines whether a table has any rows present.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if the table has any rows; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected Task<bool> TableHasRowsAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return TableHasRowsAsyncCore(table, cancellationToken);
    }

    private async Task<bool> TableHasRowsAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        var sql = await GetTableHasRowsQueryAsync(table.Name);
        return await DbConnection.ExecuteScalarAsync<bool>(sql, cancellationToken);
    }

    /// <summary>
    /// Determines whether a nullable column has any non-null values.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="column">A column from the table provided by <paramref name="table"/>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if the column has any non-null values; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="column"/> is <see langword="null" />.</exception>
    protected Task<bool> NullableColumnHasValueAsync(IRelationalDatabaseTable table, IDatabaseColumn column,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(column);

        return NullableColumnHasValueAsyncCore(table, column, cancellationToken);
    }

    private async Task<bool> NullableColumnHasValueAsyncCore(IRelationalDatabaseTable table, IDatabaseColumn column,
        CancellationToken cancellationToken)
    {
        var sql = await GetNullableColumnHasValueQueryAsync(table.Name, column.Name);
        return await DbConnection.ExecuteScalarAsync<bool>(sql, cancellationToken);
    }
    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">A name of the nullable column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />. Also thrown when <paramref name="columnName"/> is <see langword="null" />, empty or whitespace.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table '{tableName}' has a nullable column '{columnName}' whose values are always null. Consider removing the column.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    private Task<string> GetTableHasRowsQueryAsync(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetTableHasRowsQueryAsyncCore(tableName);
    }

    private async Task<string> GetTableHasRowsQueryAsyncCore(Identifier tableName)
    {
        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName));
        var filterSql = "select 1 as dummy_col from " + quotedTableName;
        var sql = $"select case when exists ({filterSql}) then 1 else 0 end as dummy";

        var suffix = await _fromQuerySuffixAsync;
        return suffix.IsNullOrWhiteSpace()
            ? sql
            : sql + " from " + suffix;
    }

    private Task<string> GetNullableColumnHasValueQueryAsync(Identifier tableName, Identifier columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnName);

        return GetNullableColumnHasValueQueryCore(tableName, columnName);
    }

    private async Task<string> GetNullableColumnHasValueQueryCore(Identifier tableName, Identifier columnName)
    {
        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName));
        var quotedColumnName = Dialect.QuoteIdentifier(columnName.LocalName);
        var filterSql = $"select 1 as exists_val from {quotedTableName} where {quotedColumnName} is not null";
        var sql = $"select case when exists ({filterSql}) then 1 else 0 end as dummy";

        var suffix = await _fromQuerySuffixAsync;
        return suffix.IsNullOrWhiteSpace()
            ? sql
            : sql + " from " + suffix;
    }

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

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0014";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "No not-null values exist for a nullable column.";

    private const string TestQueryNoTable = "select 1 as dummy";
    private const string TestQueryFromDual = "select 1 as dummy from DUAL";
    private const string TestQueryFromSysDual = "select 1 as dummy from SYS.DUAL";

    private readonly AsyncLazy<string> _fromQuerySuffixAsync;
}