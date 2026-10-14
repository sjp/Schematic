using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using SJP.Schematic.Core;
using Identifier = SJP.Schematic.Core.Identifier;
using ScriptDom = Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SJP.Schematic.SqlServer.Tests;

/// <summary>
/// Checks that <see cref="SqlServerDependencyProvider"/> finds the same names as reading
/// ScriptDom's own token stream does, including failing on the same malformed input.
/// </summary>
internal static class SqlServerDependencyProviderDifferentialTests
{
    private static readonly Identifier ObjectName = Identifier.CreateQualifiedIdentifier("dbo", "defined_object");

    [TestCaseSource(nameof(Definitions))]
    public static void GetDependencies_GivenDefinition_MatchesScriptDomTokens(string expression)
    {
        var provider = new SqlServerDependencyProvider();

        var expected = Describe(() => ScriptDomDependencies(ObjectName, expression));
        var actual = Describe(() => provider.GetDependencies(ObjectName, expression));

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public static void GetDependencies_GivenAllDefinitionsJoined_MatchesScriptDomTokens()
    {
        var provider = new SqlServerDependencyProvider();
        var expression = string.Join(Environment.NewLine, ValidDefinitions);

        var expected = Describe(() => ScriptDomDependencies(ObjectName, expression));
        var actual = Describe(() => provider.GetDependencies(ObjectName, expression));

        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("select * from [t")]
    [TestCase("select 'abc")]
    [TestCase("select \"abc")]
    [TestCase("select 1 /* /* */")]
    [TestCase("select $(var")]
    public static void GetDependencies_GivenUnterminatedText_ThrowsArgumentException(string expression)
    {
        var provider = new SqlServerDependencyProvider();

        Assert.That(
            () => provider.GetDependencies(ObjectName, expression),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("expression")
        );
    }

    // Each name as "server|database|schema|local", sorted, or the exception type when one is thrown.
    private static string[] Describe(Func<IEnumerable<Identifier>> getDependencies)
    {
        try
        {
            return getDependencies()
                .Select(i => string.Join('|', i.Server, i.Database, i.Schema, i.LocalName))
                .Order(StringComparer.Ordinal)
                .ToArray();
        }
        catch (ArgumentException ex)
        {
            return [ex.GetType().Name];
        }
    }

    // The names found by grouping ScriptDom's non-whitespace tokens into dotted names.
    private static HashSet<Identifier> ScriptDomDependencies(Identifier objectName, string expression)
    {
        var tokens = ScriptDomTokenizer.Tokenize(expression, nameof(expression))
            .Where(t => t.TokenType is not (TSqlTokenType.WhiteSpace or TSqlTokenType.EndOfFile))
            .ToList();

        var result = new HashSet<Identifier>(IdentifierComparer.Ordinal);
        for (var i = 0; i < tokens.Count; i++)
        {
            if (!IsIdentifier(tokens[i]))
                continue;

            var nameEnd = i;
            while (nameEnd + 2 < tokens.Count
                && tokens[nameEnd + 1].TokenType == TSqlTokenType.Dot
                && IsIdentifier(tokens[nameEnd + 2]))
            {
                nameEnd += 2;
            }

            var pieceCount = (nameEnd - i) / 2 + 1;
            var identifier = pieceCount switch
            {
                2 => Identifier.CreateQualifiedIdentifier(GetValue(tokens[i]), GetValue(tokens[i + 2])),
                3 => Identifier.CreateQualifiedIdentifier(GetValue(tokens[i]), GetValue(tokens[i + 2]), GetValue(tokens[i + 4])),
                4 => Identifier.CreateQualifiedIdentifier(GetValue(tokens[i]), GetValue(tokens[i + 2]), GetValue(tokens[i + 4]), GetValue(tokens[i + 6])),
                _ => Identifier.CreateQualifiedIdentifier(GetValue(tokens[i])),
            };

            if (!IdentifierComparer.Ordinal.Equals(identifier, objectName))
                result.Add(identifier);

            if (pieceCount is > 1 and <= 4)
                i = nameEnd;
        }

        return result;
    }

    private static bool IsIdentifier(TSqlParserToken token) => token.TokenType
        is TSqlTokenType.Identifier
        or TSqlTokenType.QuotedIdentifier
        or TSqlTokenType.AsciiStringOrQuotedIdentifier;

    private static string GetValue(TSqlParserToken token) => token.TokenType == TSqlTokenType.Identifier
        ? token.Text
        : ScriptDom.Identifier.DecodeIdentifier(token.Text, out _);

    private static IEnumerable<string> Definitions => ValidDefinitions.Concat(EdgeCases).Concat(InvalidText);

    // definitions created by the SQL Server integration tests
    private static readonly string[] FixtureDefinitions =
    [
        "create view db_test_view_1 as select 1 as dummy",
        "create view encrypted_test_view_1 with encryption as select 1 as test_column",
        "create view view_comment_view_2 as select 1 as test_column_1, 'test' as test_column_2",
        "create view view_test_view_2 with schemabinding as select table_id as test from [dbo].[view_test_table_1]",
        "create view view_test_view_3 as select table_id as test from [dbo].[view_test_table_1] where table_id > 0 with check option",
        "CREATE PROCEDURE db_test_routine_4\r\nAS\r\nSELECT DB_NAME() AS ThisDB",
        "CREATE PROCEDURE db_test_routine_5\r\n    @first int,\r\n    @second nvarchar(50) OUTPUT\r\nAS\r\nSELECT @first",
        "\r\nCREATE FUNCTION dbo.db_test_routine_6(@first int, @second varchar(20))\r\nRETURNS bigint\r\nAS\r\nBEGIN\r\n     RETURN(@first);\r\nEND",
        "create table table_test_table_35 ( test_column int identity (10, 5) primary key )",
        "create table index_column_partitioned_1 (part_key int not null, test_column_1 int, test_column_2 int) on ps_index_column_1 (part_key)",
    ];

    // longer definitions in the style of real modules
    private static readonly string[] ModuleDefinitions =
    [
        """
        CREATE VIEW [Sales].[vOrderSummary]
        WITH SCHEMABINDING
        AS
        -- one row per order, with the customer's current name
        SELECT o.OrderID,
               o.OrderDate,
               c.CustomerName AS [Customer Name],
               "c"."PhoneNumber",
               SUM(ol.Quantity * ol.UnitPrice) AS OrderTotal,
               COUNT_BIG(*) AS LineCount
        FROM Sales.Orders o
        INNER JOIN Sales.OrderLines AS ol ON ol.OrderID = o.OrderID
        INNER JOIN [Sales] . [Customers] c ON c.CustomerID = o.CustomerID
        LEFT JOIN Warehouse.StockItems si ON si.StockItemID = ol.StockItemID
        WHERE o.OrderDate >= '2016-01-01' AND N'it''s' <> c.CustomerName
        GROUP BY o.OrderID, o.OrderDate, c.CustomerName, "c"."PhoneNumber"
        """,
        """
        CREATE PROCEDURE [dbo].[usp_ArchiveOrders]
            @CutoffDate date = NULL,
            @BatchSize int = 5000,
            @Rows bigint OUTPUT
        AS
        BEGIN
            SET NOCOUNT ON;
            /* archive in batches
               /* nested: keep the log small */
            */
            DECLARE @moved TABLE (OrderID int PRIMARY KEY);
            IF @CutoffDate IS NULL SET @CutoffDate = DATEADD(year, -2, CAST(SYSDATETIME() AS date));

        again:
            BEGIN TRY
                BEGIN TRANSACTION;
                DELETE TOP (@BatchSize) o
                OUTPUT deleted.OrderID INTO @moved
                OUTPUT deleted.* INTO archive.dbo.Orders
                FROM dbo.Orders AS o
                WHERE o.OrderDate < @CutoffDate;

                SET @Rows = @@ROWCOUNT;
                MERGE archive.dbo.OrderAudit AS target
                USING (SELECT OrderID FROM @moved) AS source ON target.OrderID = source.OrderID
                WHEN NOT MATCHED THEN INSERT (OrderID, MovedAt) VALUES (source.OrderID, CURRENT_TIMESTAMP)
                OUTPUT $action, inserted.OrderID;
                COMMIT;
                IF @Rows = @BatchSize GOTO again;
            END TRY
            BEGIN CATCH
                IF XACT_STATE() <> 0 ROLLBACK;
                EXEC [srv1].[archive].dbo.usp_LogError @Message = N'archive failed', @Amount = $12.50;
                THROW;
            END CATCH
            SELECT x.value('(/a/b)[1]', 'int') AS v, geography::Point(1.5e2, .5, 4326).STAsText()
            FROM dbo.Docs d CROSS APPLY d.Body.nodes('/a') AS t(x)
            WHERE d.Flags & 0x0F = 0x01 AND d.Ratio > 1.;
        END
        """,
        """
        CREATE TRIGGER Sales.trg_Orders_Audit ON Sales.Orders
        AFTER INSERT, UPDATE
        AS
        IF UPDATE(OrderDate)
            INSERT INTO audit.OrderChanges (OrderID, ChangedBy, ChangedAt)
            SELECT i.OrderID, SYSTEM_USER, CURRENT_TIMESTAMP FROM inserted i
            LEFT JOIN deleted d ON d.OrderID = i.OrderID
            WHERE i.OrderDate <> d.OrderDate OR d.OrderID IS NULL;
        """,
        """
        CREATE FUNCTION dbo.ufn_Lines(@OrderID int)
        RETURNS TABLE
        AS RETURN
            SELECT ol.*, p.[Name]]Odd], #tmp.Value, ##shared.Value, [$partition].x
            FROM dbo.OrderLines ol, ##shared, #tmp
            CROSS APPLY OPENJSON(ol.Attributes) WITH (Colour nvarchar(20) '$.colour') j
            JOIN Production.Product p ON p.ProductID = ol.ProductID
            WHERE ol.OrderID = @OrderID AND $partition.pfOrders(ol.OrderDate) = 1
        """,
    ];

    private static readonly string[] ValidDefinitions = [.. FixtureDefinitions, .. ModuleDefinitions];

    // lexing corner cases: character classes, literals, comments, labels and qualified names
    private static readonly string[] EdgeCases =
    [
        "select a.b.c.d.e.f from t",
        "select * from a.b.c.d.e",
        "select * from db..t",
        "select * from s.t. x",
        "select x. from t",
        "select * from dbo./* note */t",
        "select * from dbo.-- note\r\nt",
        "select * from dbo.\"t\" where \"\" = ''",
        "select \"\"\"\" from \"a\"\"b\"",
        "select * from [a]].b].[c]",
        "select t.5, t.1x, 1abc, 1e5x, 1.e, 1.2.3, 0x1Fg, 0xZZ, 1d from t",
        "select $1.x, $ 1, $-1, $1e5, $a1, $_a, $#a, $(x.y) a",
        "select @a.b, @@rowcount, @, @.x, @a:b",
        "select x::y, a :b, a::",
        "lbl: select 1; select: x; #lbl:y; [q]:z; \"d\":w",
        "select N'x' n, n'y', N\"z\", Nx'a', x'a', N'' from t",
        "select a$b, x#y, x@y, _x, #t, ##g from t",
        "select CURRENT_USER, current_date, Session_User, try_convert(int, 1), SeLeCt from Disk, Precision, Dump, Load",
        "select * from caf\u00E9.\u00FCber, \u8868.\u00B2, a\u0660b, \u00AA, a\u02B0, \u2160x, \u00C0x",
        "select a\u200Bb, x\u3000y, a\u00A0b, a\u0085b, \u0001x from t",
        "select \uFEFFa, a\uFEFFb, a\uD83D\uDE00b, \uD83D\uDE00 from t",
        "select \u017Felect, \u212Aey from t",
        "select {fn now()}, {oj x}, a!=b, a!<b, a*=b, a=*b, a||b, a<<b from t",
        "exec p;1",
        "select '\r\n' x, [a\r\nb] from t /*\r\n*/ -- \u0085 c",
    ];

    // text that ScriptDom rejects
    private static readonly string[] InvalidText =
    [
        "select * from [t",
        "select 'abc",
        "select \"abc",
        "select \"a\"\"",
        "select 1 /*/**/",
        "select [] from t",
        "select a]b",
        "select ?",
        "select a\\b",
        "select `a`",
        "select a\u007Fb",
        "select a\u167Ab",
        "select $",
        "select $+1",
        "select $.5",
        "select $(x",
    ];
}
