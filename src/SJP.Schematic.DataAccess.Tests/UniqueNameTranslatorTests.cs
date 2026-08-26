using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests;

[TestFixture]
internal static class UniqueNameTranslatorTests
{
    [Test]
    public static void Ctor_GivenNullTranslator_ThrowsArgumentNullException()
    {
        Assert.That(() => new UniqueNameTranslator(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void TableToClassName_GivenUncontestedName_ReturnsTranslatedName()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());

        Assert.That(translator.TableToClassName("test_table"), Is.EqualTo("test_table"));
    }

    [Test]
    public static void TableToClassName_GivenTablesTranslatingToTheSameName_ReturnsDistinctNames()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.TableToClassName("first second"), Is.EqualTo("firstsecond"));
            Assert.That(translator.TableToClassName("firstsecond"), Is.EqualTo("firstsecond_1"));
        }
    }

    [Test]
    public static void TableToClassName_GivenNamesDifferingOnlyByCase_ReturnsDistinctNames()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.TableToClassName("test_table"), Is.EqualTo("test_table"));
            Assert.That(translator.TableToClassName("TEST_TABLE"), Is.EqualTo("TEST_TABLE_1"));
        }
    }

    [Test]
    public static void TableToClassName_GivenSameTableTwice_ReturnsSameName()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());
        var firstName = translator.TableToClassName("test_table");

        Assert.That(translator.TableToClassName("test_table"), Is.EqualTo(firstName));
    }

    [Test]
    public static void TableToClassName_GivenSameTableQualifiedDifferently_ReturnsSameName()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());
        var qualifiedName = Identifier.CreateQualifiedIdentifier("test_database", "test_schema", "test_table");
        var schemaQualifiedName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var firstName = translator.TableToClassName(qualifiedName);

        Assert.That(translator.TableToClassName(schemaQualifiedName), Is.EqualTo(firstName));
    }

    [Test]
    public static void ViewToClassName_GivenViewNamedAsAnExistingTable_ReturnsDistinctName()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.TableToClassName("shared_name"), Is.EqualTo("shared_name"));
            Assert.That(translator.ViewToClassName("shared_name"), Is.EqualTo("shared_name_1"));
        }
    }

    [Test]
    public static void TableToClassName_GivenSameNameInDifferentSchemas_ReturnsTranslatedNameForBoth()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());
        var firstName = Identifier.CreateQualifiedIdentifier("first", "test_table");
        var secondName = Identifier.CreateQualifiedIdentifier("second", "test_table");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.TableToClassName(firstName), Is.EqualTo("test_table"));
            Assert.That(translator.TableToClassName(secondName), Is.EqualTo("test_table"));
        }
    }

    [Test]
    public static void ReserveClassNames_GivenNamesTranslatingToTheSameName_AssignsTranslatedNameInReservedOrder()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());
        translator.ReserveClassNames(["firstsecond"], ["first second"]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.TableToClassName("firstsecond"), Is.EqualTo("firstsecond"));
            Assert.That(translator.ViewToClassName("first second"), Is.EqualTo("firstsecond_1"));
        }
    }

    [Test]
    public static void SchemaToNamespace_GivenSchemaQualifiedName_ReturnsTranslatedNamespace()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        Assert.That(translator.SchemaToNamespace(tableName), Is.EqualTo("test_schema"));
    }

    [Test]
    public static void ColumnToPropertyName_GivenValidNames_ReturnsTranslatedPropertyName()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());

        Assert.That(translator.ColumnToPropertyName("test_table", "test_column"), Is.EqualTo("test_column"));
    }
}
