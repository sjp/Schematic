using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Tests;

[TestFixture]
internal static class UrlRouterTests
{
    [Test]
    public static void GetTableUrl_GivenNullTableName_ThrowsArgumentNullException()
    {
        Assert.That(() => UrlRouter.GetTableUrl(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTableUrl_GivenValidTableName_ReturnsExpectedRoute()
    {
        Identifier tableName = "test_table";
        var url = UrlRouter.GetTableUrl(tableName);

        Assert.That(url, Is.EqualTo("#/tables/" + tableName.ToSafeKey()));
    }

    [Test]
    public static void GetViewUrl_GivenNullViewName_ThrowsArgumentNullException()
    {
        Assert.That(() => UrlRouter.GetViewUrl(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetViewUrl_GivenValidViewName_ReturnsExpectedRoute()
    {
        Identifier viewName = "test_view";
        var url = UrlRouter.GetViewUrl(viewName);

        Assert.That(url, Is.EqualTo("#/views/" + viewName.ToSafeKey()));
    }

    [Test]
    public static void GetSequenceUrl_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(() => UrlRouter.GetSequenceUrl(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSequenceUrl_GivenValidSequenceName_ReturnsExpectedRoute()
    {
        Identifier sequenceName = "test_sequence";
        var url = UrlRouter.GetSequenceUrl(sequenceName);

        Assert.That(url, Is.EqualTo("#/sequences/" + sequenceName.ToSafeKey()));
    }

    [Test]
    public static void GetSynonymUrl_GivenNullSynonymName_ThrowsArgumentNullException()
    {
        Assert.That(() => UrlRouter.GetSynonymUrl(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSynonymUrl_GivenValidSynonymName_ReturnsExpectedRoute()
    {
        Identifier synonymName = "test_synonym";
        var url = UrlRouter.GetSynonymUrl(synonymName);

        Assert.That(url, Is.EqualTo("#/synonyms/" + synonymName.ToSafeKey()));
    }

    [Test]
    public static void GetRoutineUrl_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(() => UrlRouter.GetRoutineUrl(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutineUrl_GivenValidRoutineName_ReturnsExpectedRoute()
    {
        Identifier routineName = "test_routine";
        var url = UrlRouter.GetRoutineUrl(routineName);

        Assert.That(url, Is.EqualTo("#/routines/" + routineName.ToSafeKey()));
    }

    [Test]
    public static void GetTriggerUrl_GivenNullTableName_ThrowsArgumentNullException()
    {
        Identifier triggerName = "test_trigger";
        Assert.That(() => UrlRouter.GetTriggerUrl(null!, triggerName), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTriggerUrl_GivenNullTriggerName_ThrowsArgumentNullException()
    {
        Identifier tableName = "test_table";
        Assert.That(() => UrlRouter.GetTriggerUrl(tableName, null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTriggerUrl_GivenValidNames_FoldsIntoOwningTableUrl()
    {
        Identifier tableName = "test_table";
        Identifier triggerName = "test_trigger";

        var url = UrlRouter.GetTriggerUrl(tableName, triggerName);

        // Triggers have no per-trigger route -- they fold into the owning table's detail page,
        // so the trigger name plays no part in the resulting URL.
        Assert.That(url, Is.EqualTo("#/tables/" + tableName.ToSafeKey()));
    }

    [Test]
    public static void GetTriggerUrl_GivenDifferentTriggerNamesForSameTable_ReturnsSameUrl()
    {
        Identifier tableName = "test_table";
        Identifier firstTriggerName = "trigger_one";
        Identifier secondTriggerName = "trigger_two";

        var firstUrl = UrlRouter.GetTriggerUrl(tableName, firstTriggerName);
        var secondUrl = UrlRouter.GetTriggerUrl(tableName, secondTriggerName);

        Assert.That(firstUrl, Is.EqualTo(secondUrl));
    }
}
