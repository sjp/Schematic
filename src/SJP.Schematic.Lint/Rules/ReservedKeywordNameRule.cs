using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when database objects are named using reserved keywords.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
/// <seealso cref="IViewRule" />
/// <seealso cref="ISequenceRule" />
/// <seealso cref="ISynonymRule" />
/// <seealso cref="IRoutineRule" />
public class ReservedKeywordNameRule : Rule, ITableRule, IViewRule, ISequenceRule, ISynonymRule, IRoutineRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: warning, because
    /// a keyword name breaks any query that does not quote it.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Warning;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReservedKeywordNameRule"/> class.
    /// </summary>
    /// <param name="dialect">A database dialect.</param>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dialect"/> is <see langword="null" />.</exception>
    public ReservedKeywordNameRule(IDatabaseDialect dialect, RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
        Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
    }

    /// <summary>
    /// A database dialect.
    /// </summary>
    /// <value>The database dialect.</value>
    protected IDatabaseDialect Dialect { get; }

    /// <summary>
    /// Analyses database tables. Reports messages when tables, their columns, indexes, named constraints, triggers, or their related schema have reserved keyword names.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var messages = tables.SelectMany(AnalyseTable)
            .Concat(AnalyseSchemaNames(tables.Select(static t => t.Name)))
            .ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses database views. Reports messages when views, their columns, or their related schema have reserved keyword names.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="views"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseViews(IReadOnlyCollection<IDatabaseView> views, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(views);

        var messages = views.SelectMany(AnalyseView)
            .Concat(AnalyseSchemaNames(views.Select(static v => v.Name)))
            .ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses database sequences. Reports messages when sequences, or their related schema have reserved keyword names.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequences"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseSequences(IReadOnlyCollection<IDatabaseSequence> sequences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequences);

        var messages = sequences.SelectMany(AnalyseSequence)
            .Concat(AnalyseSchemaNames(sequences.Select(static s => s.Name)))
            .ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses database synonyms. Reports messages when synonyms, or their related schema have reserved keyword names.
    /// </summary>
    /// <param name="synonyms">A set of database synonyms.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonyms"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseSynonyms(IReadOnlyCollection<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonyms);

        var messages = synonyms.SelectMany(AnalyseSynonym)
            .Concat(AnalyseSchemaNames(synonyms.Select(static s => s.Name)))
            .ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses database routines. Reports messages when routines, or their related schema have reserved keyword names.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routines"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseRoutines(IReadOnlyCollection<IDatabaseRoutine> routines, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routines);

        var messages = routines.SelectMany(AnalyseRoutine)
            .Concat(AnalyseSchemaNames(routines.Select(static r => r.Name)))
            .ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses a database table. Reports messages when a table, its columns, indexes, named constraints or triggers have reserved keyword names.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        var tableNameIsKeyword = Dialect.IsReservedKeyword(table.Name.LocalName);
        if (tableNameIsKeyword)
        {
            var message = BuildTableMessage(table.Name);
            result.Add(message);
        }

        var keywordColumnNames = table.Columns
            .Select(c => c.Name.LocalName)
            .Where(Dialect.IsReservedKeyword);

        foreach (var kwColumnName in keywordColumnNames)
        {
            var message = BuildTableColumnMessage(table.Name, kwColumnName);
            result.Add(message);
        }

        var keywordIndexNames = table.Indexes
            .Select(static i => i.Name.LocalName)
            .Where(Dialect.IsReservedKeyword);

        foreach (var kwIndexName in keywordIndexNames)
        {
            var message = BuildTableIndexMessage(table.Name, kwIndexName);
            result.Add(message);
        }

        table.PrimaryKey
            .Bind(static pk => pk.Name)
            .Where(pkName => Dialect.IsReservedKeyword(pkName.LocalName))
            .Map(pkName => BuildTablePrimaryKeyMessage(table.Name, pkName.LocalName))
            .IfSome(result.Add);

        foreach (var uniqueKey in table.UniqueKeys)
        {
            uniqueKey.Name
                .Where(ukName => Dialect.IsReservedKeyword(ukName.LocalName))
                .Map(ukName => BuildTableUniqueKeyMessage(table.Name, ukName.LocalName))
                .IfSome(result.Add);
        }

        foreach (var foreignKey in table.ParentKeys.Select(static fk => fk.ChildKey))
        {
            foreignKey.Name
                .Where(fkName => Dialect.IsReservedKeyword(fkName.LocalName))
                .Map(fkName => BuildTableForeignKeyMessage(table.Name, fkName.LocalName))
                .IfSome(result.Add);
        }

        foreach (var check in table.Checks)
        {
            check.Name
                .Where(ckName => Dialect.IsReservedKeyword(ckName.LocalName))
                .Map(ckName => BuildTableCheckConstraintMessage(table.Name, ckName.LocalName))
                .IfSome(result.Add);
        }

        var keywordTriggerNames = table.Triggers
            .Select(static t => t.Name.LocalName)
            .Where(Dialect.IsReservedKeyword);

        foreach (var kwTriggerName in keywordTriggerNames)
        {
            var message = BuildTableTriggerMessage(table.Name, kwTriggerName);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses the schemas that a set of object names belong to. Each distinct schema is reported
    /// at most once, so that a keyword schema name does not repeat for every object within it.
    /// </summary>
    /// <param name="objectNames">The names of the objects being analysed.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    private IEnumerable<IRuleMessage> AnalyseSchemaNames(IEnumerable<Identifier> objectNames)
    {
        var seenSchemas = new HashSet<string>(StringComparer.Ordinal);

        foreach (var objectName in objectNames)
        {
            var schemaName = objectName.Schema;
            if (schemaName == null || !seenSchemas.Add(schemaName))
                continue;

            if (Dialect.IsReservedKeyword(schemaName))
                yield return BuildSchemaMessage(objectName, schemaName);
        }
    }

    /// <summary>
    /// Analyses a database view. Reports messages when a view or its columns have reserved keyword names.
    /// </summary>
    /// <param name="view">A database view.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseView(IDatabaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        var result = new List<IRuleMessage>();

        var viewNameIsKeyword = Dialect.IsReservedKeyword(view.Name.LocalName);
        if (viewNameIsKeyword)
        {
            var message = BuildViewMessage(view.Name);
            result.Add(message);
        }

        var keywordColumnNames = view.Columns
            .Select(c => c.Name.LocalName)
            .Where(Dialect.IsReservedKeyword);

        foreach (var kwColumnName in keywordColumnNames)
        {
            var message = BuildViewColumnMessage(view.Name, kwColumnName);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses a database sequence. Reports messages when a sequence has a reserved keyword name.
    /// </summary>
    /// <param name="sequence">A database sequence.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequence"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseSequence(IDatabaseSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        var result = new List<IRuleMessage>();

        var sequenceNameIsKeyword = Dialect.IsReservedKeyword(sequence.Name.LocalName);
        if (sequenceNameIsKeyword)
        {
            var message = BuildSequenceMessage(sequence.Name);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses a database synonym. Reports messages when a synonym has a reserved keyword name.
    /// </summary>
    /// <param name="synonym">A set of database synonyms.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonym"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseSynonym(IDatabaseSynonym synonym)
    {
        ArgumentNullException.ThrowIfNull(synonym);

        var result = new List<IRuleMessage>();

        var synonymNameIsKeyword = Dialect.IsReservedKeyword(synonym.Name.LocalName);
        if (synonymNameIsKeyword)
        {
            var message = BuildSynonymMessage(synonym.Name);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses a database routine. Reports messages when a routine has a reserved keyword name.
    /// </summary>
    /// <param name="routine">A database routine.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routine"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseRoutine(IDatabaseRoutine routine)
    {
        ArgumentNullException.ThrowIfNull(routine);

        var result = new List<IRuleMessage>();

        var routineNameIsKeyword = Dialect.IsReservedKeyword(routine.Name.LocalName);
        if (routineNameIsKeyword)
        {
            var message = BuildRoutineMessage(routine.Name);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Builds the message used for reporting when a table's name is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildTableMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table '{tableName}' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's column is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table '{tableName}' contains a column '{columnName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's index name is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="indexName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="indexName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTableIndexMessage(Identifier tableName, string indexName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        var messageText = $"The table '{tableName}' contains an index '{indexName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's primary key name is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="primaryKeyName">The name of the primary key.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="primaryKeyName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="primaryKeyName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTablePrimaryKeyMessage(Identifier tableName, string primaryKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(primaryKeyName);

        var messageText = $"The table '{tableName}' contains a primary key '{primaryKeyName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's unique key name is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="uniqueKeyName">The name of the unique key.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="uniqueKeyName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="uniqueKeyName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTableUniqueKeyMessage(Identifier tableName, string uniqueKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueKeyName);

        var messageText = $"The table '{tableName}' contains a unique key '{uniqueKeyName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's foreign key name is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="foreignKeyName">The name of the foreign key.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="foreignKeyName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="foreignKeyName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTableForeignKeyMessage(Identifier tableName, string foreignKeyName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(foreignKeyName);

        var messageText = $"The table '{tableName}' contains a foreign key '{foreignKeyName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's check constraint name is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="checkName">The name of the check constraint.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="checkName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="checkName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTableCheckConstraintMessage(Identifier tableName, string checkName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(checkName);

        var messageText = $"The table '{tableName}' contains a check constraint '{checkName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's trigger name is a reserved keyword.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="triggerName">The name of the trigger.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="triggerName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="triggerName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTableTriggerMessage(Identifier tableName, string triggerName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(triggerName);

        var messageText = $"The table '{tableName}' contains a trigger '{triggerName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// Builds the message used for reporting when a schema's name is a reserved keyword.
    /// </summary>
    /// <param name="objectName">The name of an object within the schema, used to anchor the message.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="objectName"/> or <paramref name="schemaName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="schemaName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildSchemaMessage(Identifier objectName, string schemaName)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        var messageText = $"The schema '{schemaName}' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, objectName);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="viewName">The name of the view.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildViewMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var messageText = $"The view '{viewName}' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, viewName);
    }

    /// <summary>
    /// Builds the message used for reporting when a view's column is a reserved keyword.
    /// </summary>
    /// <param name="viewName">The name of the view.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> or <paramref name="columnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The view '{viewName}' contains a column '{columnName}' which is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, viewName);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildSequenceMessage(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var messageText = $"The sequence '{sequenceName}' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, sequenceName);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="synonymName">The name of the synonym.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildSynonymMessage(Identifier synonymName)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var messageText = $"The synonym '{synonymName}' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, synonymName);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="routineName">The name of the routine.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildRoutineMessage(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var messageText = $"The routine '{routineName}' is also a database keyword and may require quoting to be used. Consider renaming to a non-keyword name.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, routineName);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0020";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Object name is a reserved keyword.";
}