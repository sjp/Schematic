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
                .Then(static cols =>
                    ConflictClause
                        .Select(_ => new TableConstraint.PrimaryKey(cols.ToList())).Try()
                        .Or(Parse.Return<SqliteToken, TableConstraint.PrimaryKey>(new TableConstraint.PrimaryKey(cols.ToList()))));

        private static TokenListParser<SqliteToken, TableConstraint.UniqueKey> TableUniqueKey =>
            Token.EqualTo(SqliteToken.Unique).Select(static _ => _.ToEnumerable())
                .IgnoreThen(IndexedColumnList)
                .Then(static cols =>
                    ConflictClause
                        .Select(_ => new TableConstraint.UniqueKey(cols.ToList())).Try()
                        .Or(Parse.Return<SqliteToken, TableConstraint.UniqueKey>(new TableConstraint.UniqueKey(cols.ToList()))));

        private static TokenListParser<SqliteToken, TableConstraint.ForeignKey> TableForeignKey =>
            Token.Sequence(SqliteToken.Foreign, SqliteToken.Key)
                .IgnoreThen(ColumnList)
                .Then(static prev => ForeignKeyClause.Select(clause => new TableConstraint.ForeignKey(prev.ToList(), clause.ParentTable, clause.ParentColumnNames.ToList())));

        private static TokenListParser<SqliteToken, IEnumerable<string>> ColumnList =>
            Token.EqualTo(SqliteToken.LParen)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Identifier)
                        .ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma))
                        .Select(static columns => columns.Select(static col => new SqlIdentifier(col).Value.LocalName))
                )
                .Then(prev => Token.EqualTo(SqliteToken.RParen).Select(_ => prev));

        private static TokenListParser<SqliteToken, IndexedColumn> IndexedColumnParser =>
            Token.EqualTo(SqliteToken.Identifier).Select(static ident => new IndexedColumn(new SqlIdentifier(ident)))
                .Or(Expression.Select(static expr => new IndexedColumn(expr)))
                .Then(static prev =>
                    Collate.Select(prev.WithCollation)
                        .Or(Parse.Return<SqliteToken, IndexedColumn>(prev)))
                .Then(static prev =>
                    ColumnOrdering
                        .Select(prev.WithColumnOrder).Try()
                        .Or(Parse.Return<SqliteToken, IndexedColumn>(prev)));

        private static TokenListParser<SqliteToken, IEnumerable<IndexedColumn>> IndexedColumnList =>
        Token.EqualTo(SqliteToken.LParen)
            .IgnoreThen(IndexedColumnParser.ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma)))
            .Then(static prev => Token.EqualTo(SqliteToken.RParen).Select(_ => prev.AsEnumerable()));

        private static TokenListParser<SqliteToken, TableConstraint.Check> TableCheckConstraint =>
            Token.EqualTo(SqliteToken.Check)
                .IgnoreThen(Expression.Select(static expr => new TableConstraint.Check(expr)));

        private static TokenListParser<SqliteToken, TableConstraint> TableConstraintParser =>
            TablePrimaryKey.Select(static _ => _ as TableConstraint)
                .Or(TableUniqueKey.Select(static _ => _ as TableConstraint))
                .Or(TableCheckConstraint.Select(static _ => _ as TableConstraint))
                .Or(TableForeignKey.Select(static _ => _ as TableConstraint));

        private static TokenListParser<SqliteToken, TableConstraint> TableConstraintDefinition =>
            TableConstraintParser
                .Or(
                    ConstraintName
                        .Then(static name => TableConstraintParser.Select(cons => cons.WithName(name)))
                );

        public static TokenListParser<SqliteToken, Token<SqliteToken>> CreateTablePrefix =>
            Token.EqualTo(SqliteToken.Create)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Temporary).IgnoreThen(Token.EqualTo(SqliteToken.Table)).Try()
                        .Or(Token.EqualTo(SqliteToken.Table))
                );

        public static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> IfNotExistsClause =>
            Token.Sequence(SqliteToken.If, SqliteToken.Not, SqliteToken.Exists).Select(static _ => _.AsEnumerable());

        public static TokenListParser<SqliteToken, SqlIdentifier> QualifiedName =>
            Token.Sequence(SqliteToken.Identifier, SqliteToken.Period, SqliteToken.Identifier).Select(static tokens => new SqlIdentifier(tokens[0], tokens[2])).Try()
                .Or(Token.EqualTo(SqliteToken.Identifier).Select(static name => new SqlIdentifier(name)));

        private static TokenListParser<SqliteToken, Token<SqliteToken>> ExpressionContent =>
            new[] { SqliteToken.LParen, SqliteToken.RParen }.NotEqualTo();

        private static TokenListParser<SqliteToken, SqlExpression> Expression =>
            Token.EqualTo(SqliteToken.LParen)
                .Then(static lparen =>
                    Expression.Select(static expr => expr.Tokens)
                        .Or(ExpressionContent.Select(static token => token.ToEnumerable()))
                        .Many().Select(content => lparen.ToEnumerable().Concat(content.SelectMany(static _ => _))))
                .Then(static prev =>
                    Token.EqualTo(SqliteToken.RParen)
                        .Select(rparen => new SqlExpression(prev.Concat(rparen.ToEnumerable()))));

        private static TokenListParser<SqliteToken, SqlIdentifier> ConstraintName =>
            Token.EqualTo(SqliteToken.Constraint)
                .Then(static _ => Token.EqualTo(SqliteToken.Identifier).Select(static ident => new SqlIdentifier(ident)));

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> TypeDefinition =>
            Token.EqualTo(SqliteToken.Identifier)
                .AtLeastOnce()
                .Then(static idents =>
                    NumericTypeLengthConstraint.Select(c => idents.Concat(c))
                        .Try().Or(TypeLengthConstraint.Select(c => idents.Concat(c)))
                        .Try().Or(Parse.Return<SqliteToken, IEnumerable<Token<SqliteToken>>>(idents)));

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> TypeLengthConstraint =>
            Token.Sequence(SqliteToken.LParen, SqliteToken.Number, SqliteToken.RParen)
                .Select(static _ => _.AsEnumerable());

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> NumericTypeLengthConstraint =>
            Token.Sequence(SqliteToken.LParen, SqliteToken.Number, SqliteToken.Comma, SqliteToken.Number, SqliteToken.RParen)
                .Select(static _ => _.AsEnumerable());

        private static TokenListParser<SqliteToken, IndexColumnOrder> ColumnOrdering =>
            Token.EqualTo(SqliteToken.Ascending).Select(static _ => IndexColumnOrder.Ascending)
                .Or(Token.EqualTo(SqliteToken.Descending).Select(static _ => IndexColumnOrder.Descending));

        private static TokenListParser<SqliteToken, ColumnConstraint.Collation> Collate =>
            Token.EqualTo(SqliteToken.Collate)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Identifier)
                        .Or(Token.EqualTo(SqliteToken.None))
                        .Select(static c => new ColumnConstraint.Collation(c)));

        private static TokenListParser<SqliteToken, ColumnConstraint.Nullable> Nullable =>
            Token.Sequence(SqliteToken.Not, SqliteToken.Null)
                .Then(static prev =>
                    ConflictClause.Try().Or(Parse.Return<SqliteToken, Token<SqliteToken>>(prev[^1]))
                        .Select(static _ => new ColumnConstraint.Nullable(false))
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
            Token.EqualTo(SqliteToken.Number).Select(static num => num.ToEnumerable())
                .Or(Token.EqualTo(SqliteToken.HexNumber).Select(static num => num.ToEnumerable()))
                .Or(
                    Token.EqualTo(SqliteToken.Plus).Or(Token.EqualTo(SqliteToken.Minus))
                        .Then(static prev =>
                            Token.EqualTo(SqliteToken.Number).Or(Token.EqualTo(SqliteToken.HexNumber))
                                .Select(num => new[] { prev, num }.AsEnumerable()))
                );

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> SqlLiteral =>
            SignedNumber
                .Or(Token.EqualTo(SqliteToken.String).Select(static _ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.Blob).Select(static _ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.Null).Select(static _ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.CurrentDate).Select(static _ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.CurrentTime).Select(static _ => _.ToEnumerable()))
                .Or(Token.EqualTo(SqliteToken.CurrentTimestamp).Select(static _ => _.ToEnumerable()));

        private static TokenListParser<SqliteToken, ColumnConstraint.Check> ColumnCheckConstraint =>
            Token.EqualTo(SqliteToken.Check)
                .Then(static _ => Expression.Select(static expr => new ColumnConstraint.Check(expr)));

        private static TokenListParser<SqliteToken, ColumnConstraint.GeneratedAlways> GeneratedAlwaysConstraint =>
            GeneratedAlwaysConstraintNamed.Or(GeneratedAlwaysConstraintAs);

        private static TokenListParser<SqliteToken, ColumnConstraint.GeneratedAlways> GeneratedAlwaysConstraintNamed =>
            Token.EqualTo(SqliteToken.Generated)
                .Then(static _ => Token.EqualTo(SqliteToken.Always))
                .Then(static _ => Token.EqualTo(SqliteToken.As))
                .Then(static _ => Expression.Select(static expr => new ColumnConstraint.GeneratedAlways(expr)))
                .Then(static prev => Token.EqualTo(SqliteToken.Stored)
                    .Select(_ => new ColumnConstraint.GeneratedAlways(new SqlExpression(prev.Definition), SqliteGeneratedColumnType.Stored)).Try()
                    .Or(Parse.Return<SqliteToken, ColumnConstraint.GeneratedAlways>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.GeneratedAlways> GeneratedAlwaysConstraintAs =>
            Token.EqualTo(SqliteToken.As)
                .Then(static _ => Expression.Select(static expr => new ColumnConstraint.GeneratedAlways(expr)))
                .Then(static prev => Token.EqualTo(SqliteToken.Stored)
                    .Select(_ => new ColumnConstraint.GeneratedAlways(new SqlExpression(prev.Definition), SqliteGeneratedColumnType.Stored)).Try()
                    .Or(Parse.Return<SqliteToken, ColumnConstraint.GeneratedAlways>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.PrimaryKey> ColumnPrimaryKey =>
            Token.Sequence(SqliteToken.Primary, SqliteToken.Key)
                .IgnoreThen(ColumnOrdering.Select(static ordering => new ColumnConstraint.PrimaryKey(ordering)).Try().Or(Parse.Return<SqliteToken, ColumnConstraint.PrimaryKey>(new ColumnConstraint.PrimaryKey())))
                .Then(static prev => ConflictClause.Select(_ => prev).Try().Or(Parse.Return<SqliteToken, ColumnConstraint.PrimaryKey>(prev)))
                .Then(static prev =>
                    Token.EqualTo(SqliteToken.AutoIncrement)
                        .Select(_ => new ColumnConstraint.PrimaryKey(prev.ColumnOrder, true)).Try()
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.PrimaryKey>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.UniqueKey> ColumnUniqueKey =>
            Token.EqualTo(SqliteToken.Unique).Select(static _ => new ColumnConstraint.UniqueKey())
                .Then(static prev =>
                    ConflictClause.Select(_ => prev).Try()
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.UniqueKey>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.DefaultConstraint> ColumnDefaultValue =>
            Token.EqualTo(SqliteToken.Default)
                .Then(static _ =>
                    SqlLiteral.Select(static literal => new ColumnConstraint.DefaultConstraint(literal.ToList()))
                        .Or(Expression.Select(static expr => new ColumnConstraint.DefaultConstraint(expr.Tokens.ToList())))
                );

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> ForeignKeyMatch =>
            Token.EqualTo(SqliteToken.Match)
                .Then(static prev => Token.EqualTo(SqliteToken.Identifier).Select(_ => new[] { prev, _ }.AsEnumerable()));

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> ForeignKeyRules =>
            Token.EqualTo(SqliteToken.On)
                .Then(static prev =>
                    Token.EqualTo(SqliteToken.Update)
                        .Or(Token.EqualTo(SqliteToken.Delete))
                        .Select(_ => new[] { prev, _ }))
                .Then(static prev =>
                    Token.Sequence(SqliteToken.Set, SqliteToken.Null).Select(_ => prev.Concat(_))
                        .Try().Or(Token.Sequence(SqliteToken.Set, SqliteToken.Default).Select(_ => prev.Concat(_)))
                        .Try().Or(Token.EqualTo(SqliteToken.Cascade).Select(_ => prev.Concat(_.ToEnumerable())))
                        .Try().Or(Token.EqualTo(SqliteToken.Restrict).Select(_ => prev.Concat(_.ToEnumerable())))
                        .Try().Or(Token.Sequence(SqliteToken.No, SqliteToken.Action).Select(_ => prev.Concat(_)))
                );

        private static TokenListParser<SqliteToken, IEnumerable<Token<SqliteToken>>> ForeignKeyDeferrable =>
            Token.EqualTo(SqliteToken.Not).Then(static not => Token.EqualTo(SqliteToken.Deferrable).Select(def => new[] { not, def }.AsEnumerable()))
                .Or(Token.EqualTo(SqliteToken.Deferrable).Select(static _ => _.ToEnumerable()))
                .Then(static prev =>
                    Token.Sequence(SqliteToken.Initially, SqliteToken.Deferred).Select(_ => prev.Concat(_))
                        .Or(Token.Sequence(SqliteToken.Initially, SqliteToken.Immediate).Select(_ => prev.Concat(_)))
                        .Or(Parse.Return<SqliteToken, IEnumerable<Token<SqliteToken>>>(prev))
                );

        private static TokenListParser<SqliteToken, ColumnConstraint.ForeignKey> ForeignKeyClause =>
            Token.EqualTo(SqliteToken.References)
                .IgnoreThen(QualifiedName)
                .Then(static name => Token.EqualTo(SqliteToken.LParen).Select(_ => name))
                .Then(static name =>
                    Token.EqualTo(SqliteToken.Identifier)
                        .ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma))
                        .Select(columns => new { TableName = name, ColumnNames = columns.Select(static c => new SqlIdentifier(c)) }))
                .Then(static prev => Token.EqualTo(SqliteToken.RParen).Select(_ => new ColumnConstraint.ForeignKey(prev.TableName, prev.ColumnNames.ToList())))
                .Then(static prev =>
                    ForeignKeyMatch.Or(ForeignKeyRules)
                        .Many().Select(_ => prev)
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.ForeignKey>(prev))
                )
                .Then(static prev =>
                    ForeignKeyDeferrable.Select(_ => prev)
                        .Or(Parse.Return<SqliteToken, ColumnConstraint.ForeignKey>(prev)));

        private static TokenListParser<SqliteToken, ColumnConstraint> ColumnConstraintParser =>
            ColumnPrimaryKey.Select(static _ => _ as ColumnConstraint)
                .Or(Nullable.Select(static _ => _ as ColumnConstraint))
                .Or(ColumnUniqueKey.Select(static _ => _ as ColumnConstraint))
                .Or(ColumnCheckConstraint.Select(static _ => _ as ColumnConstraint))
                .Or(ColumnDefaultValue.Select(static _ => _ as ColumnConstraint))
                .Or(Collate.Select(static _ => _ as ColumnConstraint))
                .Or(ForeignKeyClause.Select(static _ => _ as ColumnConstraint))
                .Or(GeneratedAlwaysConstraint.Select(static _ => _ as ColumnConstraint));

        private static TokenListParser<SqliteToken, ColumnConstraint> ColumnConstraintDefinition =>
            ConstraintName.Then(static name => ColumnConstraintParser.Select(cons => cons.WithName(name)))
                .Try().Or(ColumnConstraintParser);

        private static TokenListParser<SqliteToken, ColumnDefinition> ColumnDefinitionParser =>
            Token.EqualTo(SqliteToken.Identifier).Select(static ident => new SqlIdentifier(ident))
                .Then(static ident =>
                    TypeDefinition.Select(typeDef => new ColumnDefinition(ident, typeDef))
                        .Or(Parse.Return<SqliteToken, ColumnDefinition>(new ColumnDefinition(ident))))
                .Then(static prev =>
                    ColumnConstraintDefinition.Many()
                        .Select(consDefs => new ColumnDefinition(prev.Name, prev.TypeDefinition, consDefs)));

        private static TokenListParser<SqliteToken, TableMember> TableMemberParser =>
            ColumnDefinitionParser.Select(static column => new TableMember(column))
                .Or(TableConstraintDefinition.Select(static table => new TableMember(table)));

        public static TokenListParser<SqliteToken, IEnumerable<TableMember>> TableMembers =>
            TableMemberParser
                .ManyDelimitedBy(Token.EqualTo(SqliteToken.Comma))
                .Select(static _ => _.AsEnumerable());

        // triggers
        private static TokenListParser<SqliteToken, Token<SqliteToken>> CreateTriggerPrefix =>
            Token.EqualTo(SqliteToken.Create)
                .IgnoreThen(
                    Token.EqualTo(SqliteToken.Temporary).IgnoreThen(Token.EqualTo(SqliteToken.Trigger))
                        .Try().Or(Token.EqualTo(SqliteToken.Trigger))
                );

        private static TokenListParser<SqliteToken, TriggerQueryTiming> TriggerTiming =>
            Token.EqualTo(SqliteToken.Before).Select(static _ => TriggerQueryTiming.Before)
                .Or(Token.EqualTo(SqliteToken.After).Select(static _ => TriggerQueryTiming.After))
                .Or(Token.Sequence(SqliteToken.Instead, SqliteToken.Of).Select(static _ => TriggerQueryTiming.InsteadOf))
                .Or(Parse.Return<SqliteToken, TriggerQueryTiming>(TriggerQueryTiming.After));

        private static TokenListParser<SqliteToken, TriggerEvent> DeclaredTriggerEvent =>
            Token.EqualTo(SqliteToken.Delete).Select(static _ => TriggerEvent.Delete)
                .Or(Token.EqualTo(SqliteToken.Insert).Select(static _ => TriggerEvent.Insert))
                .Or(Token.EqualTo(SqliteToken.Update).Select(static _ => TriggerEvent.Update));

        public static TokenListParser<SqliteToken, (TriggerQueryTiming timing, TriggerEvent evt)> TriggerDefinition =>
            CreateTriggerPrefix
                .Then(static prev => IfNotExistsClause.Try().Or(Parse.Return<SqliteToken, IEnumerable<Token<SqliteToken>>>(prev.ToEnumerable())))
                .IgnoreThen(QualifiedName)
                .IgnoreThen(TriggerTiming)
                .Then(static timing => DeclaredTriggerEvent.Select(evt => (timing, evt)));
    }
}
