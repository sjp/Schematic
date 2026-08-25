using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The lint summary payload (<c>data/lint.json</c>).
/// </summary>
/// <remarks>
/// Messages are a single flat list rather than being nested inside their rule, so a message's
/// text is serialized exactly once no matter how the UI chooses to slice it. The rule catalogue
/// carries only per-rule metadata and counts; the UI joins the two on <c>ruleId</c> to render a
/// grouped view, and reads the flat list directly for a filterable message view.
/// </remarks>
public sealed class LintResults
{
    public LintResults(IEnumerable<LintRule> lintRules, IEnumerable<LintMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(lintRules);
        ArgumentNullException.ThrowIfNull(messages);

        var ruleList = lintRules.ToList();
        var messageList = messages.ToList();

        LintRules = ruleList;
        LintRulesCount = ruleList.UCount();
        Messages = messageList;
        MessageCount = messageList.UCount();

        ErrorCount = CountAtLevel(messageList, RuleLevel.Error);
        WarningCount = CountAtLevel(messageList, RuleLevel.Warning);
        InformationCount = CountAtLevel(messageList, RuleLevel.Information);

        ObjectsAffectedCount = (uint)messageList
            .Where(static m => m.ObjectUrl != null)
            .Select(static m => m.ObjectUrl!)
            .Distinct(StringComparer.Ordinal)
            .Count();
    }

    /// <summary>The rules that raised at least one message, ordered by rule identifier.</summary>
    public IEnumerable<LintRule> LintRules { get; }

    /// <summary>How many distinct rules raised a message. Not the number of issues found.</summary>
    public uint LintRulesCount { get; }

    /// <summary>Every message raised, across every rule.</summary>
    public IEnumerable<LintMessage> Messages { get; }

    /// <summary>The total number of issues found.</summary>
    public uint MessageCount { get; }

    public uint ErrorCount { get; }

    public uint WarningCount { get; }

    public uint InformationCount { get; }

    /// <summary>
    /// How many distinct database objects raised at least one message. Schema-wide messages that
    /// belong to no single object are excluded.
    /// </summary>
    public uint ObjectsAffectedCount { get; }

    private static uint CountAtLevel(IReadOnlyCollection<LintMessage> messages, RuleLevel level)
    {
        return (uint)messages.Count(m => m.Level == level);
    }

    /// <summary>
    /// A rule that raised at least one message, with the count of messages it raised.
    /// </summary>
    public sealed class LintRule
    {
        public LintRule(string ruleId, string ruleTitle, RuleLevel level, uint messageCount)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(ruleTitle);

            RuleId = ruleId;
            RuleTitle = ruleTitle;
            Level = level;
            MessageCount = messageCount;
        }

        /// <summary>The rule's stable identifier, e.g. <c>SCHEMATIC0009</c>.</summary>
        public string RuleId { get; }

        public string RuleTitle { get; }

        public RuleLevel Level { get; }

        public uint MessageCount { get; }
    }

    /// <summary>
    /// A single lint finding, linked back to the object that raised it where there is one.
    /// </summary>
    public sealed class LintMessage
    {
        public LintMessage(string ruleId, string ruleTitle, RuleLevel level, string message, Identifier? objectName, LintObjectType? objectType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
            ArgumentException.ThrowIfNullOrWhiteSpace(ruleTitle);
            ArgumentException.ThrowIfNullOrWhiteSpace(message);

            RuleId = ruleId;
            RuleTitle = ruleTitle;
            Level = level;
            Message = message;

            // A message with no owning object (a schema-wide finding) leaves all three object
            // fields null, which the serializer omits entirely.
            if (objectName != null && objectType.HasValue)
            {
                ObjectName = objectName.ToVisibleName();
                ObjectType = objectType;
                ObjectUrl = GetObjectUrl(objectName, objectType.Value);
            }
        }

        public string RuleId { get; }

        public string RuleTitle { get; }

        public RuleLevel Level { get; }

        public string Message { get; }

        /// <summary>The qualified name of the object this message concerns, if any.</summary>
        public string? ObjectName { get; }

        public LintObjectType? ObjectType { get; }

        /// <summary>A hash route to the object's detail page, if any.</summary>
        public string? ObjectUrl { get; }

        private static string GetObjectUrl(Identifier objectName, LintObjectType objectType)
        {
            return objectType switch
            {
                LintObjectType.Table => UrlRouter.GetTableUrl(objectName),
                LintObjectType.View => UrlRouter.GetViewUrl(objectName),
                LintObjectType.Sequence => UrlRouter.GetSequenceUrl(objectName),
                LintObjectType.Synonym => UrlRouter.GetSynonymUrl(objectName),
                LintObjectType.Routine => UrlRouter.GetRoutineUrl(objectName),
                _ => throw new ArgumentOutOfRangeException(nameof(objectType), objectType, "Unknown lint object type."),
            };
        }
    }
}

/// <summary>
/// The kind of database object a lint message is attributed to. Determines which detail route
/// the message links to.
/// </summary>
public enum LintObjectType
{
    Table,
    View,
    Sequence,
    Synonym,
    Routine,
}
