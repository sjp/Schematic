using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests;

internal static class UniqueNameTranslatorTests
{
    [Test]
    public static void Ctor_GivenNullTranslator_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UniqueNameTranslator(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("translator")
        );
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

    [Test]
    public static void SchemaToNamespace_GivenNullName_ThrowsArgumentNullException()
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());

        Assert.That(
            () => translator.SchemaToNamespace(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("objectName")
        );
    }

    [Test]
    public static void SchemaToNamespace_GivenSameNameTwice_TranslatesOnce()
    {
        var innerTranslator = new CountingNameTranslator();
        var translator = new UniqueNameTranslator(innerTranslator);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var firstNamespace = translator.SchemaToNamespace(tableName);
        var secondNamespace = translator.SchemaToNamespace(tableName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(secondNamespace, Is.EqualTo(firstNamespace));
            Assert.That(innerTranslator.SchemaToNamespaceCalls, Is.EqualTo(1));
        }
    }

    [Test]
    public static void SchemaToNamespace_GivenNameWithoutSchemaTwice_ReturnsNullAndTranslatesOnce()
    {
        var innerTranslator = new CountingNameTranslator();
        var translator = new UniqueNameTranslator(innerTranslator);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.SchemaToNamespace("test_table"), Is.Null);
            Assert.That(translator.SchemaToNamespace("test_table"), Is.Null);
            Assert.That(innerTranslator.SchemaToNamespaceCalls, Is.EqualTo(1));
        }
    }

    [Test]
    public static void TableToClassName_AfterNamespaceTranslated_DoesNotTranslateNamespaceAgain()
    {
        var innerTranslator = new CountingNameTranslator();
        var translator = new UniqueNameTranslator(innerTranslator);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        _ = translator.SchemaToNamespace(tableName);
        _ = translator.TableToClassName(tableName);

        Assert.That(innerTranslator.SchemaToNamespaceCalls, Is.EqualTo(1));
    }

    [Test]
    public static void ColumnToPropertyName_GivenSameColumnTwice_TranslatesOnce()
    {
        var innerTranslator = new CountingNameTranslator();
        var translator = new UniqueNameTranslator(innerTranslator);

        var firstName = translator.ColumnToPropertyName("test_table", "test_column");
        var secondName = translator.ColumnToPropertyName("test_table", "test_column");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(secondName, Is.EqualTo(firstName));
            Assert.That(innerTranslator.ColumnToPropertyNameCalls, Is.EqualTo(1));
        }
    }

    [Test]
    public static void ColumnToPropertyName_GivenSameColumnInDifferentClasses_TranslatesEach()
    {
        var innerTranslator = new CountingNameTranslator();
        var translator = new UniqueNameTranslator(innerTranslator);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.ColumnToPropertyName("test_table", "test_table"), Is.EqualTo("test_table_"));
            Assert.That(translator.ColumnToPropertyName("other_table", "test_table"), Is.EqualTo("test_table"));
            Assert.That(innerTranslator.ColumnToPropertyNameCalls, Is.EqualTo(2));
        }
    }

    [Test]
    public static void ColumnToPropertyName_GivenColumnNamesDifferingOnlyByCase_TranslatesEach()
    {
        var innerTranslator = new CountingNameTranslator();
        var translator = new UniqueNameTranslator(innerTranslator);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(translator.ColumnToPropertyName("test_table", "test_column"), Is.EqualTo("test_column"));
            Assert.That(translator.ColumnToPropertyName("test_table", "TEST_COLUMN"), Is.EqualTo("TEST_COLUMN"));
            Assert.That(innerTranslator.ColumnToPropertyNameCalls, Is.EqualTo(2));
        }
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ColumnToPropertyName_GivenNullOrWhiteSpaceColumnName_ThrowsArgumentExceptionFromTranslator(string columnName)
    {
        var translator = new UniqueNameTranslator(new VerbatimNameTranslator());

        Assert.That(
            () => translator.ColumnToPropertyName("test_table", columnName),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("columnName")
        );
    }

    private sealed class CountingNameTranslator : INameTranslator
    {
        private readonly VerbatimNameTranslator _translator = new();

        public int SchemaToNamespaceCalls { get; private set; }

        public int ColumnToPropertyNameCalls { get; private set; }

        public string SchemaToNamespace(Identifier objectName)
        {
            SchemaToNamespaceCalls++;
            return _translator.SchemaToNamespace(objectName);
        }

        public string TableToClassName(Identifier tableName) => _translator.TableToClassName(tableName);

        public string ViewToClassName(Identifier viewName) => _translator.ViewToClassName(viewName);

        public string ColumnToPropertyName(string className, string columnName)
        {
            ColumnToPropertyNameCalls++;
            return _translator.ColumnToPropertyName(className, columnName);
        }
    }
}
