using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.Sqlite.Parsing;

internal static class SqliteTokenParsers
{
    public static TokenListParser<SqliteToken, SqlIdentifier> QualifiedName =>
        Token.Sequence(SqliteToken.Identifier, SqliteToken.Period, SqliteToken.Identifier).Select(static tokens => new SqlIdentifier(tokens[0], tokens[2])).Try()
            .Or(Token.EqualTo(SqliteToken.Identifier).Select(static name => new SqlIdentifier(name)));
}
