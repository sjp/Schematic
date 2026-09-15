using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace SJP.Schematic.Sqlite.Parsing.Antlr;

/// <summary>
/// Constructs ANTLR parse trees for SQLite DDL statements. The lexer and parser are
/// configured to throw on the first syntax error via <see cref="ThrowingErrorListener"/>.
/// </summary>
internal static class SqliteDdlParser
{
    /// <summary>
    /// Parses a <c>CREATE TABLE</c> definition into an ANTLR parse tree.
    /// </summary>
    /// <param name="sql">A <c>CREATE TABLE</c> definition.</param>
    /// <returns>The parsed <c>CREATE TABLE</c> statement context.</returns>
    /// <exception cref="SqliteSyntaxErrorException">The definition could not be parsed.</exception>
    public static SQLiteParser.Create_table_stmtContext ParseTableDefinition(string sql)
        => Parse(sql, static parser => parser.create_table_stmt());

    /// <summary>
    /// Parses a <c>CREATE TRIGGER</c> definition into an ANTLR parse tree.
    /// </summary>
    /// <param name="sql">A <c>CREATE TRIGGER</c> definition.</param>
    /// <returns>The parsed <c>CREATE TRIGGER</c> statement context.</returns>
    /// <exception cref="SqliteSyntaxErrorException">The definition could not be parsed.</exception>
    public static SQLiteParser.Create_trigger_stmtContext ParseTriggerDefinition(string sql)
        => Parse(sql, static parser => parser.create_trigger_stmt());

    /// <summary>
    /// Applies a parser rule to SQL text in two stages. The first stage uses the faster but
    /// approximate SLL prediction mode, which gives the same parse tree as full LL prediction
    /// whenever it succeeds, and gives up immediately otherwise. Only when it gives up is the
    /// input parsed again with full LL prediction, which either succeeds or reports the syntax
    /// error that SLL could not distinguish from a prediction shortcoming.
    /// </summary>
    /// <typeparam name="T">The parse tree node produced by the rule.</typeparam>
    /// <param name="sql">SQL text to parse.</param>
    /// <param name="rule">The parser rule to apply.</param>
    /// <returns>The parsed statement context.</returns>
    /// <exception cref="SqliteSyntaxErrorException">The SQL could not be lexed or parsed.</exception>
    private static T Parse<T>(string sql, Func<SQLiteParser, T> rule)
    {
        var inputStream = new AntlrInputStream(sql);

        var lexer = new SQLiteLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener.Instance);

        var tokenStream = new CommonTokenStream(lexer);

        var parser = new SQLiteParser(tokenStream);
        // No throwing listener in the first stage: the generated rules report an error before
        // handing over to the error strategy, so a throwing listener would reject input that
        // full LL prediction is able to parse.
        parser.RemoveErrorListeners();
        parser.ErrorHandler = new BailErrorStrategy();
        parser.Interpreter.PredictionMode = PredictionMode.SLL;

        try
        {
            return rule(parser);
        }
        catch (ParseCanceledException)
        {
            parser.Reset();
            parser.AddErrorListener(ThrowingErrorListener.Instance);
            parser.ErrorHandler = new DefaultErrorStrategy();
            parser.Interpreter.PredictionMode = PredictionMode.LL;

            return rule(parser);
        }
    }
}
