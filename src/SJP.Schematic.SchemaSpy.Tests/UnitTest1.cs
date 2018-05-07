using NUnit.Framework;
using SJP.Schematic.SchemaSpy.Html;
using SJP.Schematic.SchemaSpy.Html.ViewModels;

namespace SJP.Schematic.SchemaSpy.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void Test1()
        {
            var provider = new TemplateProvider();
            var formatter = new HtmlFormatter(provider);

            var p = new Column
            {
                TableName = "ASD & ASD",
                ColumnName = "ColumnTest & < asdklj",
                DefaultValue = "asd",
                IsNullable = true,
                IsForeignKeyColumn = true,
                IsUniqueKeyColumn = true,
                ParentType = Column.ParentObjectType.Table,
                Type = "nvarchar(234)"
            };
            var cols = new Columns
            {
                TableColumns = new[] { p }
            };

            var template = formatter.RenderTemplate(cols);

            Assert.Pass();
        }
    }
}
