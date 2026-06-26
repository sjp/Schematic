using Antlr4.Runtime;

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
        => CreateParser(sql).create_table_stmt();

    /// <summary>
    /// Parses a <c>CREATE TRIGGER</c> definition into an ANTLR parse tree.
    /// </summary>
    /// <param name="sql">A <c>CREATE TRIGGER</c> definition.</param>
    /// <returns>The parsed <c>CREATE TRIGGER</c> statement context.</returns>
    /// <exception cref="SqliteSyntaxErrorException">The definition could not be parsed.</exception>
    public static SQLiteParser.Create_trigger_stmtContext ParseTriggerDefinition(string sql)
        => CreateParser(sql).create_trigger_stmt();

    private static SQLiteParser CreateParser(string sql)
    {
        var inputStream = new AntlrInputStream(sql);

        var lexer = new SQLiteLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingErrorListener.Instance);

        var tokenStream = new CommonTokenStream(lexer);

        var parser = new SQLiteParser(tokenStream);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(ThrowingErrorListener.Instance);

        return parser;
    }
}
