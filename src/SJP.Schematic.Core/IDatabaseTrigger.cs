using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database trigger.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
/// <seealso cref="IDatabaseOptional" />
public interface IDatabaseTrigger : IDatabaseEntity, IDatabaseOptional
{
    /// <summary>
    /// A trigger definition.
    /// </summary>
    /// <value>The trigger definition.</value>
    string Definition { get; }

    /// <summary>
    /// Describes when a trigger should be executed within a particular query.
    /// </summary>
    /// <value>The execution timing within a query.</value>
    TriggerQueryTiming QueryTiming { get; }

    /// <summary>
    /// The table events which cause this trigger to execute.
    /// </summary>
    /// <value>A bitwise value defining which events cause the trigger to fire.</value>
    TriggerEvent TriggerEvent { get; }

    /// <summary>
    /// Describes how often the trigger fires for the statement that caused it to fire.
    /// </summary>
    /// <value>The trigger granularity, <see cref="TriggerGranularity.Unknown"/> when the database does not report one.</value>
    TriggerGranularity Granularity { get; }

    /// <summary>
    /// An expression that must evaluate to <c>true</c> for the trigger body to run, i.e. a <c>WHEN</c> clause.
    /// </summary>
    /// <value>The trigger condition, or <see cref="Option{A}.None"/> when the trigger is unconditional or the database does not support conditions.</value>
    Option<string> Condition { get; }

    /// <summary>
    /// The columns that an <c>UPDATE</c> must touch for the trigger to fire, i.e. an <c>UPDATE OF</c> column list.
    /// </summary>
    /// <value>A collection of column names. Empty when the trigger fires for updates to any column.</value>
    IReadOnlyCollection<Identifier> UpdateColumns { get; }
}
