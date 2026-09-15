using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class ViewModelMapperTests
{
    [Test]
    public static void Map_GivenColumnsOfUserDefinedAndBuiltInTypes_LinksOnlyTheUserDefinedOne()
    {
        var moodName = Identifier.CreateQualifiedIdentifier("test_schema", "mood");
        IDatabaseColumn[] columns =
        [
            new DatabaseColumn("id", TestDbTypes.BigInteger, false, null, null),
            new DatabaseColumn("current_mood", UserDefinedDbTypes.Named(moodName), true, null, null),
        ];
        var view = new DatabaseView(
            Identifier.CreateQualifiedIdentifier("test_schema", "person_moods"),
            "select id, current_mood from person",
            columns);

        var model = new ViewModelMapper().Map(
            view,
            new ReferencedObjectTargets(new EmptyDependencyProvider(), [], [], [], [], [], []),
            new UserDefinedTypeTargets([moodName]));

        var viewColumns = model.Columns.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewColumns[0].TypeUrl, Is.Null);
            Assert.That(viewColumns[1].TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(moodName)));
        }
    }
}
