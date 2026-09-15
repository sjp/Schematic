using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class RoutineModelMapperTests
{
    private static readonly Identifier MoodName = Identifier.CreateQualifiedIdentifier("test_schema", "mood");

    [Test]
    public static void Map_GivenParametersAndReturnTypeOfUserDefinedType_LinksEachToItsPage()
    {
        var moodType = UserDefinedDbTypes.Named(MoodName);
        IDatabaseRoutineParameter[] parameters =
        [
            new DatabaseRoutineParameter(Option<Identifier>.Some("person_id"), TestDbTypes.BigInteger, RoutineParameterDirection.Input, Option<string>.None, 1),
            new DatabaseRoutineParameter(Option<Identifier>.Some("fallback"), moodType, RoutineParameterDirection.Input, Option<string>.None, 2),
        ];
        var routine = new DatabaseRoutine(
            Identifier.CreateQualifiedIdentifier("test_schema", "mood_of"),
            "create function mood_of(...) returns test_schema.mood",
            RoutineType.Function,
            Option<string>.Some("sql"),
            parameters,
            Option<IDbType>.Some(moodType),
            []);

        var model = Map(routine);
        var mappedParameters = model.Parameters.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mappedParameters[0].TypeUrl, Is.Null);
            Assert.That(mappedParameters[1].TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
            Assert.That(model.ReturnTypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
        }
    }

    [Test]
    public static void Map_GivenOverloadReturningUserDefinedType_LinksOverloadReturnType()
    {
        var moodType = UserDefinedDbTypes.Named(MoodName);
        IDatabaseRoutineOverload[] overloads =
        [
            new DatabaseRoutineOverload("create function mood_of(bigint) returns test_schema.mood", [], Option<IDbType>.Some(moodType)),
        ];
        var routine = new DatabaseRoutine(
            Identifier.CreateQualifiedIdentifier("test_schema", "mood_of"),
            "create function mood_of(bigint) returns test_schema.mood",
            RoutineType.Function,
            Option<string>.None,
            [],
            Option<IDbType>.None,
            overloads);

        var model = Map(routine);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(model.ReturnTypeUrl, Is.Null);
            Assert.That(model.Overloads.Single().ReturnTypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
        }
    }

    private static Reporting.Html.ViewModels.Routine Map(IDatabaseRoutine routine)
    {
        return new RoutineModelMapper().Map(
            routine,
            new ReferencedObjectTargets(new EmptyDependencyProvider(), [], [], [], [], [], []),
            new UserDefinedTypeTargets([MoodName]));
    }
}
