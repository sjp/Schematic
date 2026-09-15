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
    public static void GetReferencedObjects_GivenNameInSeveralKinds_ListsLinksInKindOrder()
    {
        var orders = Identifier.CreateQualifiedIdentifier("app", "orders");
        var targets = CreateTargets(
            [Identifier.CreateQualifiedIdentifier("orders")],
            tableNames: [orders],
            viewNames: [orders],
            sequenceNames: [orders],
            synonymNames: [orders],
            routineNames: [orders]
        );

        var urls = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Url);

        Assert.That(urls, Is.EqualTo(new[]
        {
            UrlRouter.GetTableUrl(orders),
            UrlRouter.GetViewUrl(orders),
            UrlRouter.GetSequenceUrl(orders),
            UrlRouter.GetSynonymUrl(orders),
            UrlRouter.GetRoutineUrl(orders),
        }));
    }

    [Test]
    public static void GetReferencedObjects_GivenReferenceInDifferentCasing_MatchesEveryCasingOfTheName()
    {
        var lower = Identifier.CreateQualifiedIdentifier("app", "orders");
        var upper = Identifier.CreateQualifiedIdentifier("APP", "ORDERS");
        var targets = CreateTargets([Identifier.CreateQualifiedIdentifier("App", "Orders")], tableNames: [lower, upper, lower]);

        var names = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Name);

        Assert.That(names, Is.EqualTo(new[] { "app.orders", "APP.ORDERS" }));
    }

    [Test]
    public static void GetReferencedObjects_GivenUnqualifiedReference_ResolvesInReferencingObjectSchema()
    {
        var appOrders = Identifier.CreateQualifiedIdentifier("app", "orders");
        var otherOrders = Identifier.CreateQualifiedIdentifier("other", "orders");
        var targets = CreateTargets([Identifier.CreateQualifiedIdentifier("orders")], tableNames: [otherOrders, appOrders]);

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
            tableNames: [orders, Identifier.CreateQualifiedIdentifier("app", "orders")]
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
            tableNames: [orders],
            viewNames: [ViewName]
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
            tableNames: [orders, customers]
        );

        var names = targets.GetReferencedObjects(ViewName, "select 1").Select(static o => o.Name);

        Assert.That(names, Is.EqualTo(new[] { "app.customers", "app.orders" }));
    }

    private static ReferencedObjectTargets CreateTargets(
        IReadOnlyCollection<Identifier> dependencies,
        IEnumerable<Identifier> tableNames = null,
        IEnumerable<Identifier> viewNames = null,
        IEnumerable<Identifier> sequenceNames = null,
        IEnumerable<Identifier> synonymNames = null,
        IEnumerable<Identifier> routineNames = null)
    {
        var dependencyProvider = new Mock<IDependencyProvider>();
        dependencyProvider
            .Setup(static p => p.GetDependencies(It.IsAny<Identifier>(), It.IsAny<string>()))
            .Returns(dependencies);

        return new ReferencedObjectTargets(
            dependencyProvider.Object,
            tableNames ?? [],
            viewNames ?? [],
            sequenceNames ?? [],
            synonymNames ?? [],
            routineNames ?? []
        );
    }
}
