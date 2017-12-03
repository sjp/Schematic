using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.SqlServer.Parsing
{
    internal static class SqlServerTokenParsers
    {
        public static TokenListParser<SqlServerToken, SqlIdentifier> FullyQualifiedName =>
            Token.Sequence(
                SqlServerToken.Identifier,
                SqlServerToken.Period,
                SqlServerToken.Identifier,
                SqlServerToken.Period,
                SqlServerToken.Identifier,
                SqlServerToken.Period,
                SqlServerToken.Identifier
            ).Select(tokens => new SqlIdentifier(tokens[0], tokens[2], tokens[4], tokens[6]));

        public static TokenListParser<SqlServerToken, SqlIdentifier> DatabaseQualifiedName =>
            Token.Sequence(
                SqlServerToken.Identifier,
                SqlServerToken.Period,
                SqlServerToken.Identifier,
                SqlServerToken.Period,
                SqlServerToken.Identifier
            ).Select(tokens => new SqlIdentifier(tokens[0], tokens[2], tokens[4]));

        public static TokenListParser<SqlServerToken, SqlIdentifier> SchemaQualifiedName =>
            Token.Sequence(
                SqlServerToken.Identifier,
                SqlServerToken.Period,
                SqlServerToken.Identifier
            ).Select(tokens => new SqlIdentifier(tokens[0], tokens[2]));


        public static TokenListParser<SqlServerToken, SqlIdentifier> LocalName =>
            Token.EqualTo(SqlServerToken.Identifier)
                .Select(name => new SqlIdentifier(name));

        public static TokenListParser<SqlServerToken, SqlIdentifier> QualifiedName =>
            FullyQualifiedName
                .Try().Or(DatabaseQualifiedName)
                .Try().Or(SchemaQualifiedName)
                .Try().Or(LocalName);
    }
}
