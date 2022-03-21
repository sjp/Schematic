using Superpower;
using Superpower.Parsers;

namespace SJP.Schematic.Oracle.Parsing;

internal static class OracleTokenParsers
{
    public static TokenListParser<OracleToken, SqlIdentifier> FullyQualifiedName =>
        Token.Sequence(
            OracleToken.Identifier,
            OracleToken.Period,
            OracleToken.Identifier,
            OracleToken.Period,
            OracleToken.Identifier,
            OracleToken.Period,
            OracleToken.Identifier
        ).Select(static tokens => new SqlIdentifier(tokens[0], tokens[2], tokens[4], tokens[6]));

    public static TokenListParser<OracleToken, SqlIdentifier> DatabaseQualifiedName =>
        Token.Sequence(
            OracleToken.Identifier,
            OracleToken.Period,
            OracleToken.Identifier,
            OracleToken.Period,
            OracleToken.Identifier
        ).Select(static tokens => new SqlIdentifier(tokens[0], tokens[2], tokens[4]));

    public static TokenListParser<OracleToken, SqlIdentifier> SchemaQualifiedName =>
        Token.Sequence(
            OracleToken.Identifier,
            OracleToken.Period,
            OracleToken.Identifier
        ).Select(static tokens => new SqlIdentifier(tokens[0], tokens[2]));

    public static TokenListParser<OracleToken, SqlIdentifier> LocalName =>
        Token.EqualTo(OracleToken.Identifier)
            .Select(static name => new SqlIdentifier(name));

    public static TokenListParser<OracleToken, SqlIdentifier> QualifiedName =>
        FullyQualifiedName
            .Try().Or(DatabaseQualifiedName)
            .Try().Or(SchemaQualifiedName)
            .Try().Or(LocalName);
}