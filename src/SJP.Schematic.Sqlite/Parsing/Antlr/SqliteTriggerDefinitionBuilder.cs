using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Parsing.Antlr;

/// <summary>
/// Translates an ANTLR <c>CREATE TRIGGER</c> parse tree into <see cref="ParsedTriggerData"/>.
/// </summary>
internal static class SqliteTriggerDefinitionBuilder
{
    public static ParsedTriggerData Build(SQLiteParser.Create_trigger_stmtContext context)
    {
        var timing = GetTiming(context);
        var triggerEvent = GetEvent(context);

        return new ParsedTriggerData(timing, triggerEvent);
    }

    private static TriggerQueryTiming GetTiming(SQLiteParser.Create_trigger_stmtContext context)
    {
        if (context.BEFORE_() != null)
            return TriggerQueryTiming.Before;
        if (context.INSTEAD_() != null && context.OF_().Length > 0)
            return TriggerQueryTiming.InsteadOf;

        // AFTER is both the explicit keyword case and the default when no timing is declared,
        // matching the behaviour of the previous parser.
        return TriggerQueryTiming.After;
    }

    private static TriggerEvent GetEvent(SQLiteParser.Create_trigger_stmtContext context)
    {
        if (context.DELETE_() != null)
            return TriggerEvent.Delete;
        if (context.INSERT_() != null)
            return TriggerEvent.Insert;
        if (context.UPDATE_() != null)
            return TriggerEvent.Update;

        return TriggerEvent.None;
    }
}
