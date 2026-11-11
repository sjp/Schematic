using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class ReferencedObjectTargetsTests
{
    private static readonly Identifier ViewName = Identifier.CreateQualifiedIdentifier("app", "order_summary");

    [Test]
    public static void Ctor_GivenDefaultObject_ThrowsArgumentNullException()
    {
        Assert.That(() => new ReferencedObjectTargets(new Mock<IDependencyProvider>().Object, [default]), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetReferencedObjects_GivenNameInSeveralKinds_ListsLinksInKindOrder()
    {
        var orders = Identifier.CreateQualifiedIdentifier("app", "orders");
        var targets = CreateTargets(
            [Identifier.CreateQualifiedIdentifier("orders")],
            // Given in reverse, as links follow the precedence of each kind and not the order given.
            [
                TestDatabaseObjects.UserDefinedType(orders),
                TestDatabaseObjects.Routine(orders),
                TestDatabaseObjects.Synonym(orders),
                TestDatabaseObjects.Sequence(orders),
                TestDatabaseObjects.View(orders),
                TestDatabaseObjects.Table(orders),
            ]
        );

        var urls = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Url);

        Assert.That(urls, Is.EqualTo(new[]
        {
            UrlRouter.GetTableUrl(orders),
            UrlRouter.GetViewUrl(orders),
            UrlRouter.GetSequenceUrl(orders),
            UrlRouter.GetSynonymUrl(orders),
            UrlRouter.GetRoutineUrl(orders),
            UrlRouter.GetUserDefinedTypeUrl(orders),
        }));
    }

    [Test]
    public static void GetReferencedObjects_GivenReferenceInDifferentCasing_MatchesEveryCasingOfTheName()
    {
        var lower = Identifier.CreateQualifiedIdentifier("app", "orders");
        var upper = Identifier.CreateQualifiedIdentifier("APP", "ORDERS");
        var targets = CreateTargets([Identifier.CreateQualifiedIdentifier("App", "Orders")], [TestDatabaseObjects.Table(lower), TestDatabaseObjects.Table(upper), TestDatabaseObjects.Table(lower)]);

        var names = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Name);

        Assert.That(names, Is.EqualTo(new[] { "app.orders", "APP.ORDERS" }));
    }

    [Test]
    public static void GetReferencedObjects_GivenUnqualifiedReference_ResolvesInReferencingObjectSchema()
    {
        var appOrders = Identifier.CreateQualifiedIdentifier("app", "orders");
        var otherOrders = Identifier.CreateQualifiedIdentifier("other", "orders");
        var targets = CreateTargets([Identifier.CreateQualifiedIdentifier("orders")], [TestDatabaseObjects.Table(otherOrders), TestDatabaseObjects.Table(appOrders)]);

        var names = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Name);

        Assert.That(names, Is.EqualTo(new[] { "app.orders" }));
    }

    [Test]
    public static void GetReferencedObjects_GivenNamesWithoutSchema_MatchesOnLocalName()
    {
        var viewName = Identifier.CreateQualifiedIdentifier("order_summary");
        var orders = Identifier.CreateQualifiedIdentifier("orders");
        var targets = CreateTargets(
            [Identifier.CreateQualifiedIdentifier("ORDERS")],
            [TestDatabaseObjects.Table(orders), TestDatabaseObjects.Table(Identifier.CreateQualifiedIdentifier("app", "orders"))]
        );

        var names = targets.GetReferencedObjects(viewName, "select 1").Select(static o => o.Name);

        Assert.That(names, Is.EqualTo(new[] { "orders" }));
    }

    [Test]
    public static void GetReferencedObjects_GivenSelfReference_OmitsTheReferencingObject()
    {
        var orders = Identifier.CreateQualifiedIdentifier("app", "orders");
        var targets = CreateTargets(
            [Identifier.CreateQualifiedIdentifier("ORDER_SUMMARY"), orders],
            [TestDatabaseObjects.Table(orders), TestDatabaseObjects.View(ViewName)]
        );

        var names = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Name);

        Assert.That(names, Is.EqualTo(new[] { "app.orders" }));
    }

    [Test]
    public static void GetReferencedObjects_GivenRepeatedReferences_ListsEachTargetOnceInNameOrder()
    {
        var customers = Identifier.CreateQualifiedIdentifier("app", "customers");
        var orders = Identifier.CreateQualifiedIdentifier("app", "orders");
        var targets = CreateTargets(
            [Identifier.CreateQualifiedIdentifier("orders"), Identifier.CreateQualifiedIdentifier("column_a"), orders, Identifier.CreateQualifiedIdentifier("Customers")],
            [TestDatabaseObjects.Table(orders), TestDatabaseObjects.Table(customers)]
        );

        var names = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Name);

        Assert.That(names, Is.EqualTo(new[] { "app.customers", "app.orders" }));
    }

    [Test]
    public static void GetReferencedObjects_GivenReferenceToUserDefinedType_LinksToTypePage()
    {
        var mood = Identifier.CreateQualifiedIdentifier("app", "mood");
        var targets = CreateTargets([Identifier.CreateQualifiedIdentifier("app", "Mood")], [TestDatabaseObjects.UserDefinedType(mood)]);

        var links = targets.GetReferencedObjects(ViewName, "select 1").ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(links.Select(static o => o.Name), Is.EqualTo(new[] { "app.mood" }));
            Assert.That(links.Select(static o => o.Url), Is.EqualTo(new[] { UrlRouter.GetUserDefinedTypeUrl(mood) }));
        }
    }

    [Test]
    public static void GetReferencedObjects_GivenExpressionTheDialectCannotRead_ReturnsNoLinks()
    {
        var orders = Identifier.CreateQualifiedIdentifier("app", "orders");
        var dependencyProvider = new Mock<IDependencyProvider>();
        dependencyProvider
            .Setup(static p => p.GetDependencies(It.IsAny<Identifier>(), It.IsAny<string>()))
            .Throws(static () => new ArgumentException("Could not parse the given expression as a SQL expression.", "expression"));
        var targets = new ReferencedObjectTargets(dependencyProvider.Object, [TestDatabaseObjects.Table(orders)]);

        Assert.That(() => targets.GetReferencedObjects(ViewName, "not sql at all"), Is.Empty);
    }

    private static ReferencedObjectTargets CreateTargets(
        IReadOnlyCollection<Identifier> dependencies,
        IEnumerable<DatabaseObject> databaseObjects)
    {
        var dependencyProvider = new Mock<IDependencyProvider>();
        dependencyProvider
            .Setup(static p => p.GetDependencies(It.IsAny<Identifier>(), It.IsAny<string>()))
            .Returns(dependencies);

        return new ReferencedObjectTargets(dependencyProvider.Object, databaseObjects);
    }
}
