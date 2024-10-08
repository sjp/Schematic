﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when database object names contain whitespace.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
/// <seealso cref="IViewRule" />
/// <seealso cref="ISequenceRule" />
/// <seealso cref="ISynonymRule" />
/// <seealso cref="IRoutineRule" />
public class WhitespaceNameRule : Rule, ITableRule, IViewRule, ISequenceRule, ISynonymRule, IRoutineRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhitespaceNameRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public WhitespaceNameRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when tables have whitespace in their names or their related schema's names.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses database views. Reports messages when views have whitespace in their names or their related schema's names.
    /// </summary>
    /// <param name="views">A set of database views.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="views"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseViews(IEnumerable<IDatabaseView> views, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(views);

        return views.SelectMany(AnalyseView).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses database sequences. Reports messages when sequences have whitespace in their names.
    /// </summary>
    /// <param name="sequences">A set of database sequences.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequences"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseSequences(IEnumerable<IDatabaseSequence> sequences, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequences);

        return sequences.SelectMany(AnalyseSequence).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses database synonyms. Reports messages when synonyms have whitespace in their names.
    /// </summary>
    /// <param name="synonyms">A set of database synonyms.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonyms"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseSynonyms(IEnumerable<IDatabaseSynonym> synonyms, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonyms);

        return synonyms.SelectMany(AnalyseSynonym).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses database routines. Reports messages when routines have whitespace in their names.
    /// </summary>
    /// <param name="routines">A set of database routines.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routines"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseRoutines(IEnumerable<IDatabaseRoutine> routines, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routines);

        return routines.SelectMany(AnalyseRoutine).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses a database table. Reports messages when a table has whitespace in its name or their related schema's names.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        var tableNameHasWs = HasWhiteSpace(table.Name.LocalName);
        if (tableNameHasWs)
        {
            var message = BuildTableMessage(table.Name);
            result.Add(message);
        }

        var whiteSpaceColumnNames = table.Columns
            .Select(c => c.Name.LocalName)
            .Where(HasWhiteSpace);

        foreach (var wsColumnName in whiteSpaceColumnNames)
        {
            var message = BuildTableColumnMessage(table.Name, wsColumnName);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses a database view. Reports messages when a view has whitespace in its name or related schema.
    /// </summary>
    /// <param name="view">A set of database views.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="view"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseView(IDatabaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

        var result = new List<IRuleMessage>();

        var viewNameHasWs = HasWhiteSpace(view.Name.LocalName);
        if (viewNameHasWs)
        {
            var message = BuildViewMessage(view.Name);
            result.Add(message);
        }

        var whiteSpaceColumnNames = view.Columns
            .Select(c => c.Name.LocalName)
            .Where(HasWhiteSpace);

        foreach (var wsColumnName in whiteSpaceColumnNames)
        {
            var message = BuildViewColumnMessage(view.Name, wsColumnName);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses a database sequence. Reports messages when a sequence has whitespace in its name.
    /// </summary>
    /// <param name="sequence">A database sequence.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequence"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseSequence(IDatabaseSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);

        var result = new List<IRuleMessage>();

        var sequenceNameHasWs = HasWhiteSpace(sequence.Name.LocalName);
        if (sequenceNameHasWs)
        {
            var message = BuildSequenceMessage(sequence.Name);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses a database synonym. Reports messages when a synonym has whitespace in its name.
    /// </summary>
    /// <param name="synonym">A database synonym.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonym"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseSynonym(IDatabaseSynonym synonym)
    {
        ArgumentNullException.ThrowIfNull(synonym);

        var result = new List<IRuleMessage>();

        var synonymNameHasWs = HasWhiteSpace(synonym.Name.LocalName);
        if (synonymNameHasWs)
        {
            var message = BuildSynonymMessage(synonym.Name);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Analyses a database routine. Reports messages when a routine has whitespace in its name.
    /// </summary>
    /// <param name="routine">A database routine.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routine"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseRoutine(IDatabaseRoutine routine)
    {
        ArgumentNullException.ThrowIfNull(routine);

        var result = new List<IRuleMessage>();

        var routineNameHasWs = HasWhiteSpace(routine.Name.LocalName);
        if (routineNameHasWs)
        {
            var message = BuildRoutineMessage(routine.Name);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Determines whether text has whitespace in any part of the text.
    /// </summary>
    /// <param name="input">A string of text representing an object name.</param>
    /// <returns><c>true</c> if whitespace is present in <paramref name="input"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <c>null</c>.</exception>
    private static bool HasWhiteSpace(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input.Any(char.IsWhiteSpace);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildTableMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table '{tableName}' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds the message used for reporting when a table's column contains whitespace.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>, or <paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
    protected virtual IRuleMessage BuildTableColumnMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table '{tableName}' contains a column '{columnName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="viewName">The name of the view.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildViewMessage(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var messageText = $"The view '{viewName}' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds the message used for reporting when a view's column contains whitespace.
    /// </summary>
    /// <param name="viewName">The name of the view.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>, or <paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
    protected virtual IRuleMessage BuildViewColumnMessage(Identifier viewName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The view '{viewName}' contains a column '{columnName}' which contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="sequenceName">The name of the sequence.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildSequenceMessage(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var messageText = $"The sequence '{sequenceName}' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="synonymName">The name of the synonym.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildSynonymMessage(Identifier synonymName)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var messageText = $"The synonym '{synonymName}' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="routineName">The name of the routine.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
    protected virtual IRuleMessage BuildRoutineMessage(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var messageText = $"The routine '{routineName}' contains whitespace and requires quoting to be used. Consider renaming to remove any whitespace.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0023";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Whitespace present in object name.";
}