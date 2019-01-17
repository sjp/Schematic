using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal static class SqliteTokenParsers
    {
        private static TokenListParser<SqliteToken, TableConstraint.PrimaryKey> TablePrimaryKey =>
            Token.Sequence(SqliteToken.Primary, SqliteToken.Key)
                .IgnoreThen(IndexedColumnList)
                .Then(cols =>
                    ConflictClause
                        .Select(_ => new TableConstraint.PrimaryKey(cols.ToList())).Try()
                        .Or(Parse.Return<SqliteToken, TableConstraint.PrimaryKey>(new TableConstraint.PrimaryKey(cols.ToList()))));

        private static TokenListParser<SqliteToken, TableConstraint.UniqueKey> TableUniqueKey =>
            Token.EqualTo(SqliteToken.Unique).Select(_ => _.ToEnumerable())
                .IgnoreThen(IndexedColumnList)
                .Then(cols =>
                    ConflictClause
                        .Select(_ => new TableConstraint.UniqueKey(cols.ToList())).Try()
                        .Or(Parse.Return<SqliteToken, TableConstraint.UniqueKey>(new TableConstraint.UniqueKey(cols.ToList()))));

        private static TokenListParser<SqliteToken, TableConstraint.ForeignKey> TableForeignKey =>
            Token.Sequence(SqliteToken.Foreign, SqliteToken.Key)
                .IgnoreThen(ColumnList)
                .Then(prev => ForeignKeyClause.Select(clause => new TableConstraint.ForeignKey(prev.ToList(), clause.ParentTable, clause.ParentColumnNames.ToList())));

        private static TokenListParser<SqliteToken, IEnumerable<string>> ColumnList =>
            Token.EqualTo(SqliteToken.LParen)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Identifier)
                        .ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma))
                        .Select(columns => columns.Select(col => new SqlIdentifier(col).Value.LocalName))
                )
                .Then(prev => Token.EqualTo(SqliteToken.RParen).Select(_ => prev));

        private static TokenListParser<SqliteToken, IndexedColumn> IndexedColumnParser =>
            Token.EqualTo(SqliteToken.Identifier).Select(ident => new IndexedColumn(new SqlIdentifier(ident)))
                .Or(Expression.Select(expr => new IndexedColumn(expr)))
                .Then(prev =>
                    Collate.Select(prev.WithCollation)
                        .Or(Parse.Return<SqliteToken, IndexedColumn>(prev)))
                .Then(prev =>
                    ColumnOrdering
                        .Select(prev.WithColumnOrder).Try()
                        .Or(Parse.Return<SqliteToken, IndexedColumn>(prev)));

        private static TokenListParser<SqliteToken, IEnumerable<IndexedColumn>> IndexedColumnList =>
        Token.EqualTo(SqliteToken.LParen)
            .IgnoreThen(IndexedColumnParser.ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma)))
            .Then(prev => Token.EqualTo(SqliteToken.RParen).Select(_ => prev.AsEnumerable()));

        private static TokenListParser<SqliteToken, TableConstraint.Check> TableCheckConstraint =>
            Token.EqualTo(SqliteToken.Check)
                .IgnoreThen(Expression.Select(expr => new TableConstraint.Check(expr)));

        private static TokenListParser<SqliteToken, TableConstraint> TableConstraintParser =>
            TablePrimaryKey.Select(_ => _ as TableConstraint)
                .Or(TableUniqueKey.Select(_ => _ as TableConstraint))
                .Or(TableCheckConstraint.Select(_ => _ as TableConstraint))
                .Or(TableForeignKey.Select(_ => _ as TableConstraint));

        private static TokenListParser<SqliteToken, TableConstraint> TableConstraintDefinition =>
            TableConstraintParser
                .Or(
                    ConstraintName
                        .Then(name => TableConstraintParser.Select(cons => cons.WithName(name)))
                );

        public static TokenListParser<SqliteToken, Token<SqliteToken>> CreateTablePrefix =>
            Token.EqualTo(SqliteToken.Create)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Temporary).IgnoreThen(Token.EqualTo(SqliteToken.Table)).Try()
                        .Or(Token.EqualTo(SqliteToken.Table))
                );

        public static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> IfNotExistsClause =>
            Token.Sequence(SqliteToken.If, SqliteToken.Not, SqliteToken.Exists).Select(_ => _.AsEnumerable());

        public static TokenListParser<SqliteToken, SqlIdentifier> QualifiedName =>
            Token.Sequence(SqliteToken.Identifier, SqliteToken.Period, SqliteToken.Identifier).Select(tokens => new SqlIdentifier(tokens[0], tokens[2])).Try()
                .Or(Token.EqualTo(SqliteToken.Identifier).Select(name => new SqlIdentifier(name)));

        private static TokenListParser<SqliteToken, Token<SqliteToken>> ExpressionContent =>
            new[] { SqliteToken.LParen, SqliteToken.RParen }.NotEqualTo();

        private static TokenListParser<SqliteToken, SqlExpression> Expression =>
            Token.EqualTo(SqliteToken.LParen)
                .Then(lparen =>
                    Expression.Select(expr => expr.Tokens)
                        .Or(ExpressionContent.Select(token => token.ToEnumerable()))
                        .Many().Select(content => lparen.ToEnumerable().Concat(content.SelectMany(_ => _))))
                .Then(prev =>
                    Token.EqualTo(SqliteToken.RParen)
                        .Select(rparen => new SqlExpression(prev.Concat(rparen.ToEnumerable()))));

        private static TokenListParser<SqliteToken, SqlIdentifier> ConstraintName =>
            Token.EqualTo(SqliteToken.Constraint)
                .Then(_ => Token.EqualTo(SqliteToken.Identifier).Select(ident => new SqlIdentifier(ident)));

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> TypeDefinition =>
            Token.EqualTo(SqliteToken.Identifier)
                .AtLeastOnce()
                .Then(idents =>
                    NumericTypeLengthConstraint.Select(c => idents.Concat(c))
                        .Try().Or(TypeLengthConstraint.Select(c => idents.Concat(c)))
                        .Try().Or(Parse.Return<SqliteToken, IEnumerable<Token<SqliteToken>>>(idents)));

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> TypeLengthConstraint =>
            Token.Sequence(SqliteToken.LParen, SqliteToken.Number, SqliteToken.RParen)
                .Select(_ => _.AsEnumerable());

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> NumericTypeLengthConstraint =>
            Token.Sequence(SqliteToken.LParen, SqliteToken.Number, SqliteToken.Comma, SqliteToken.Number, SqliteToken.RParen)
                .Select(_ => _.AsEnumerable());

        private static TokenListParser<SqliteToken, IndexColumnOrder> ColumnOrdering =>
            Token.EqualTo(SqliteToken.Ascending).Select(_ => IndexColumnOrder.Ascending)
                .Or(Token.EqualTo(SqliteToken.Descending).Select(_ => IndexColumnOrder.Descending));

        private static TokenListParser<SqliteToken, ColumnConstraint.Collation> Collate =>
            Token.EqualTo(SqliteToken.Collate)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Identifier)
                        .Or(Token.EqualTo(SqliteToken.None))
                        .Select(c => new ColumnConstraint.Collation(c)));

        private static TokenListParser<SqliteToken, ColumnConstraint.Nullable> Nullable =>
            Token.Sequence(SqliteToken.Not, SqliteToken.Null)
                .Then(prev =>
                    ConflictClause.Try().Or(Parse.Return<SqliteToken, Token<SqliteToken>>(prev.Last()))
                        .Select(_ => new ColumnConstraint.Nullable(false))
                )
                .Try().Or(Token.EqualTo(SqliteToken.Null).Select(_ => new ColumnConstraint.Nullable(true)));

        private static TokenListParser<SqliteToken, Token<SqliteToken>> ConflictClause =>
            Token.EqualTo(SqliteToken.On)
                .IgnoreThen(Token.EqualTo(SqliteToken.Conflict))
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Rollback)
                        .Or(Token.EqualTo(SqliteToken.Abort))
                        .Or(Token.EqualTo(SqliteToken.Fail))
                        .Or(Token.EqualTo(SqliteToken.Ignore))
                        .Or(Token.EqualTo(SqliteToken.Replace))
                );

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> SignedNumber =>
            Token.EqualTo(SqliteToken.Number).Select(num => num.ToEnumerable())
                .Or(Token.EqualTo(SqliteToken.HexNumber).Select(num => num.ToEnumerable()))
                .Or(
                    Token.EqualTo(SqliteToken.Plus).Or(Token.EqualTo(SqliteToken.Minus))
                        .Then(prev =>
                            Token.EqualTo(SqliteToken.Number).Or(Token.EqualTo(SqliteToken.HexNumber))
                                .Select(num => new[] { prev, num }.AsEnumerable()))
                );

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> SqlLiteral =>
            SignedNumber
                .Or(Token.EqualTo(SqliteToken.String).Select(_ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.Blob).Select(_ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.Null).Select(_ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.CurrentDate).Select(_ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.CurrentTime).Select(_ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.CurrentTimestamp).Select(_ => _.ToEnumerable()));

        private static TokenListParser<SqliteToken, ColumnConstraint.Check> ColumnCheckConstraint =>
            Token.EqualTo(SqliteToken.Check)
                .Then(_ => Expression.Select(expr => new ColumnConstraint.Check(expr)));

        private static TokenListParser<SqliteToken, ColumnConstraint.PrimaryKey> ColumnPrimaryKey =>
            Token.Sequence(SqliteToken.Primary, SqliteToken.Key)
                .IgnoreThen(ColumnOrdering.Select(ordering => new ColumnConstraint.PrimaryKey(ordering)).Try().Or(Parse.Return<SqliteToken, ColumnConstraint.PrimaryKey>(new ColumnConstraint.PrimaryKey())))
                .Then(prev => ConflictClause.Select(_ => prev).Try().Or(Parse.Return<SqliteToken, ColumnConstraint.PrimaryKey>(prev)))
                .Then(prev =>
                    Token.EqualTo(SqliteToken.Autoincrement)
                        .Select(_ => new ColumnConstraint.PrimaryKey(prev.ColumnOrder, true)).Try()
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.PrimaryKey>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.UniqueKey> ColumnUniqueKey =>
            Token.EqualTo(SqliteToken.Unique).Select(_ => new ColumnConstraint.UniqueKey())
                .Then(prev =>
                    ConflictClause.Select(_ => prev).Try()
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.UniqueKey>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.DefaultConstraint> ColumnDefaultValue =>
            Token.EqualTo(SqliteToken.Default)
                .Then(_ =>
                    SqlLiteral.Select(literal => new ColumnConstraint.DefaultConstraint(literal.ToList()))
                        .Or(Expression.Select(expr => new ColumnConstraint.DefaultConstraint(expr.Tokens.ToList())))
                );

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> ForeignKeyMatch =>
            Token.EqualTo(SqliteToken.Match)
                .Then(prev => Token.EqualTo(SqliteToken.Identifier).Select(_ => new[] { prev, _ }.AsEnumerable()));

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> ForeignKeyRules =>
            Token.EqualTo(SqliteToken.On)
                .Then(prev =>
                    Token.EqualTo(SqliteToken.Update)
                        .Or(Token.EqualTo(SqliteToken.Delete))
                        .Select(_ => new[] { prev, _ }))
                .Then(prev =>
                    Token.Sequence(SqliteToken.Set, SqliteToken.Null).Select(_ => prev.Concat(_))
                        .Try().Or(Token.Sequence(SqliteToken.Set, SqliteToken.Default).Select(_ => prev.Concat(_)))
                        .Try().Or(Token.EqualTo(SqliteToken.Cascade).Select(_ => prev.Concat(_.ToEnumerable())))
                        .Try().Or(Token.EqualTo(SqliteToken.Restrict).Select(_ => prev.Concat(_.ToEnumerable())))
                        .Try().Or(Token.Sequence(SqliteToken.No, SqliteToken.Action).Select(_ => prev.Concat(_)))
                );

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> ForeignKeyDeferrable =>
            Token.EqualTo(SqliteToken.Not).Then(not => Token.EqualTo(SqliteToken.Deferrable).Select(def => new[] { not, def }.AsEnumerable()))
                .Or(Token.EqualTo(SqliteToken.Deferrable).Select(_ => _.ToEnumerable()))
                .Then(prev =>
                    Token.Sequence(SqliteToken.Initially, SqliteToken.Deferred).Select(_ => prev.Concat(_))
                        .Or(Token.Sequence(SqliteToken.Initially, SqliteToken.Immediate).Select(_ => prev.Concat(_)))
                        .Or(Parse.Return<SqliteToken, IEnumerable<Token<SqliteToken>>>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.ForeignKey> ForeignKeyClause =>
            Token.EqualTo(SqliteToken.References)
                .IgnoreThen(QualifiedName)
                .Then(name => Token.EqualTo(SqliteToken.LParen).Select(_ => name))
                .Then(name =>
                    Token.EqualTo(SqliteToken.Identifier)
                        .ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma))
                        .Select(columns => new { TableName = name, ColumnNames = columns.Select(c => new SqlIdentifier(c)) }))
                .Then(prev => Token.EqualTo(SqliteToken.RParen).Select(_ => new ColumnConstraint.ForeignKey(prev.TableName, prev.ColumnNames.ToList())))
                .Then(prev =>
                    ForeignKeyMatch.Or(ForeignKeyRules)
                        .Many().Select(_ => prev)
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.ForeignKey>(prev))
                )
                .Then(prev =>
                    ForeignKeyDeferrable.Select(_ => prev)
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.ForeignKey>(prev)));

        private static TokenListParser<SqliteToken, ColumnConstraint> ColumnConstraintParser =>
            ColumnPrimaryKey.Select(_ => _ as ColumnConstraint)
                .Or(Nullable.Select(_ => _ as ColumnConstraint))
                .Or(ColumnUniqueKey.Select(_ => _ as ColumnConstraint))
                .Or(ColumnCheckConstraint.Select(_ => _ as ColumnConstraint))
                .Or(ColumnDefaultValue.Select(_ => _ as ColumnConstraint))
                .Or(Collate.Select(_ => _ as ColumnConstraint))
                .Or(ForeignKeyClause.Select(_ => _ as ColumnConstraint));

        private static TokenListParser<SqliteToken, ColumnConstraint> ColumnConstraintDefinition =>
            ConstraintName.Then(name => ColumnConstraintParser.Select(cons => cons.WithName(name)))
                .Try().Or(ColumnConstraintParser);

        private static TokenListParser<SqliteToken, ColumnDefinition> ColumnDefinitionParser =>
            Token.EqualTo(SqliteToken.Identifier).Select(ident => new SqlIdentifier(ident))
                .Then(ident =>
                    TypeDefinition.Select(typeDef => new ColumnDefinition(ident, typeDef))
                        .Or(Parse.Return<SqliteToken, ColumnDefinition>(new ColumnDefinition(ident))))
                .Then(prev =>
                    ColumnConstraintDefinition.Many()
                        .Select(consDefs => new ColumnDefinition(prev.Name, prev.TypeDefinition, consDefs)));

        private static TokenListParser<SqliteToken, TableMember> TableMemberParser =>
            ColumnDefinitionParser.Select(column => new TableMember(column))
                .Or(TableConstraintDefinition.Select(table => new TableMember(table)));

        public static TokenListParser<SqliteToken, IEnumerable<TableMember>> TableMembers =>
            TableMemberParser
                .ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma))
                .Select(_ => _.AsEnumerable());

        // triggers
        private static TokenListParser<SqliteToken, Token<SqliteToken>> CreateTriggerPrefix =>
            Token.EqualTo(SqliteToken.Create)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Temporary).IgnoreThen(Token.EqualTo(SqliteToken.Trigger))
                        .Try().Or(Token.EqualTo(SqliteToken.Trigger))
                );

        private static TokenListParser<SqliteToken, TriggerQueryTiming> TriggerTiming =>
            Token.EqualTo(SqliteToken.Before).Select(_ => TriggerQueryTiming.Before)
                .Or(Token.EqualTo(SqliteToken.After).Select(_ => TriggerQueryTiming.After))
                .Or(Token.Sequence(SqliteToken.Instead, SqliteToken.Of).Select(_ => TriggerQueryTiming.InsteadOf))
                .Or(Parse.Return<SqliteToken, TriggerQueryTiming>(TriggerQueryTiming.After));

        private static TokenListParser<SqliteToken, TriggerEvent> DeclaredTriggerEvent =>
            Token.EqualTo(SqliteToken.Delete).Select(_ => TriggerEvent.Delete)
                .Or(Token.EqualTo(SqliteToken.Insert).Select(_ => TriggerEvent.Insert))
                .Or(Token.EqualTo(SqliteToken.Update).Select(_ => TriggerEvent.Update));

        public static TokenListParser<SqliteToken, (TriggerQueryTiming timing, TriggerEvent evt)> TriggerDefinition =>
            CreateTriggerPrefix
                .Then(prev => IfNotExistsClause.Try().Or(Parse.Return<SqliteToken, IEnumerable<Token<SqliteToken>>>(prev.ToEnumerable())))
                .IgnoreThen(QualifiedName)
                .IgnoreThen(TriggerTiming)
                .Then(timing => DeclaredTriggerEvent.Select(evt => (timing, evt)));
    }
}
