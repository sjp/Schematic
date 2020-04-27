using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseColumnTests
    {
        [Test]
        public static void Ctor_GivenNullColumnName_ThrowsArgumentNullException()
        {
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            var defaultValue = Option<string>.Some("test_default_value");
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            Assert.That(() => new DatabaseColumn(null, dbType, isNullable, defaultValue, autoIncrement), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDbType_ThrowsArgumentNullException()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            const bool isNullable = false;
            var defaultValue = Option<string>.Some("test_default_value");
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            Assert.That(() => new DatabaseColumn(columnName, null, isNullable, defaultValue, autoIncrement), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            var defaultValue = Option<string>.Some("test_default_value");
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.That(column.Name, Is.EqualTo(columnName));
        }

        [Test]
        public static void Type_PropertyGet_EqualsCtorArg()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            var defaultValue = Option<string>.Some("test_default_value");
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.That(column.Type, Is.EqualTo(dbType));
        }

        [TestCase(true)]
        [TestCase(false)]
        public static void IsNullable_PropertyGet_EqualsCtorArg(bool isNullable)
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            var defaultValue = Option<string>.Some("test_default_value");
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.That(column.IsNullable, Is.EqualTo(isNullable));
        }

        [Test]
        public static void DefaultValue_GivenNoneDefaultValue_EqualsNone()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            var defaultValue = Option<string>.None;
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.That(column.DefaultValue, OptionIs.None);
        }

        [Test]
        public static void DefaultValue_GivenSomeDefaultValue_EqualsCtorArg()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            const string defaultExpression = "test_default_value";
            var defaultValue = Option<string>.Some(defaultExpression);
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.That(column.DefaultValue.UnwrapSome(), Is.EqualTo(defaultExpression));
        }

        [Test]
        public static void AutoIncrement_GivenNoneAutoIncrement_EqualsNone()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            var defaultValue = Option<string>.None;
            var autoIncrement = Option<IAutoIncrement>.None;

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.That(column.AutoIncrement, OptionIs.None);
        }

        [Test]
        public static void AutoIncrement_GivenSomeAutoIncrement_EqualsCtorArg()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            var defaultValue = Option<string>.Some("test_default_value");

            const decimal initialValue = 123m;
            const decimal increment = 456m;
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(initialValue, increment));

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.Multiple(() =>
            {
                Assert.That(column.AutoIncrement.UnwrapSome().Increment, Is.EqualTo(increment));
                Assert.That(column.AutoIncrement.UnwrapSome().InitialValue, Is.EqualTo(initialValue));
            });
        }

        [Test]
        public static void IsComputed_PropertyGet_EqualsFalse()
        {
            var columnName = Identifier.CreateQualifiedIdentifier("test_column_name");
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            const string defaultExpression = "test_default_value";
            var defaultValue = Option<string>.Some(defaultExpression);
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            var column = new DatabaseColumn(columnName, dbType, isNullable, defaultValue, autoIncrement);

            Assert.That(column.IsComputed, Is.False);
        }

        [TestCase("test_column_1", "Column: test_column_1")]
        [TestCase("test_column_2", "Column: test_column_2")]
        public static void ToString_WhenInvoked_ReturnsExpectedValues(string columnName, string expectedResult)
        {
            var dbType = Mock.Of<IDbType>();
            const bool isNullable = false;
            var defaultValue = Option<string>.Some("test_default_value");
            var autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(123, 456));

            var column = new DatabaseColumn(Identifier.CreateQualifiedIdentifier(columnName), dbType, isNullable, defaultValue, autoIncrement);
            var result = column.ToString();

            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
