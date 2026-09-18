using System;
using System.Globalization;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Reporting.Html.Lint.Rules;

internal sealed class LikelyMisspelledNameRule : Schematic.Lint.Rules.LikelyMisspelledNameRule
{
    public LikelyMisspelledNameRule(RuleLevel? level = null)
        : base(level)
    {
    }

    protected override IRuleMessage BuildObjectNameMessage(Identifier objectName, string objectDescription, string suspectWord, string suggestedWord, int suggestedWordNameCount)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(suspectWord);
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedWord);

        var messageText = $"The name of the {objectDescription} {objectName.ToVisibleName()} contains the word '{suspectWord}', which does not appear elsewhere in the schema and differs by one character from '{suggestedWord}', used in {suggestedWordNameCount.ToString(CultureInfo.InvariantCulture)} other names. Consider whether this is a misspelling.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, objectName);
    }

    protected override IRuleMessage BuildMemberNameMessage(Identifier objectName, string objectDescription, string memberDescription, string name, string suspectWord, string suggestedWord, int suggestedWordNameCount)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(suspectWord);
        ArgumentException.ThrowIfNullOrWhiteSpace(suggestedWord);

        var messageText = $"The {memberDescription} name '{name}' in the {objectDescription} {objectName.ToVisibleName()} contains the word '{suspectWord}', which does not appear elsewhere in the schema and differs by one character from '{suggestedWord}', used in {suggestedWordNameCount.ToString(CultureInfo.InvariantCulture)} other names. Consider whether this is a misspelling.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, objectName);
    }
}
