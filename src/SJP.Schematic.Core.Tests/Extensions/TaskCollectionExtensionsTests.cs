using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

internal static class TaskCollectionExtensionsTests
{
    [Test]
    public static async Task WhenAll2_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
        }
    }

    [Test]
    public static async Task WhenAll3_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
        }
    }

    [Test]
    public static async Task WhenAll4_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
        }
    }

    [Test]
    public static async Task WhenAll5_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
        }
    }

    [Test]
    public static async Task WhenAll6_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
        }
    }

    [Test]
    public static async Task WhenAll7_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
        }
    }

    [Test]
    public static async Task WhenAll8_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
        }
    }

    [Test]
    public static async Task WhenAll9_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
        }
    }

    [Test]
    public static async Task WhenAll10_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
        }
    }

    [Test]
    public static async Task WhenAll11_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
        }
    }

    [Test]
    public static async Task WhenAll12_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
        }
    }

    [Test]
    public static async Task WhenAll13_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
        }
    }

    [Test]
    public static async Task WhenAll14_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result,
            task14Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromResult("14")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
            Assert.That(task14Result, Is.EqualTo("14"));
        }
    }

    [Test]
    public static async Task WhenAll15_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result,
            task14Result,
            task15Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromResult("14"),
            Task.FromResult(15)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
            Assert.That(task14Result, Is.EqualTo("14"));
            Assert.That(task15Result, Is.EqualTo(15));
        }
    }

    [Test]
    public static async Task WhenAll16_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result,
            task14Result,
            task15Result,
            task16Result
        ) = await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromResult("14"),
            Task.FromResult(15),
            Task.FromResult("16")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
            Assert.That(task14Result, Is.EqualTo("14"));
            Assert.That(task15Result, Is.EqualTo(15));
            Assert.That(task16Result, Is.EqualTo("16"));
        }
    }

    [Test]
    public static void WhenAll2_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll2_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll3_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll3_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll3_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll4_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll4_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll4_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll4_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll5_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll5_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll5_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll5_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll5_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll6_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll6_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll6_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll6_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll6_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll6_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll7_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll7_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll7_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll7_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll7_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll7_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll7_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll8_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll9_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9),
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null,
            Task.FromResult(10)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll10_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task10"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null,
            Task.FromResult(10),
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            (Task<string>)null,
            Task.FromResult(11)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task10"));
    }

    [Test]
    public static void WhenAll11_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task11"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null,
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            (Task<string>)null,
            Task.FromResult(11),
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task10"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            (Task<string>)null,
            Task.FromResult(12)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task11"));
    }

    [Test]
    public static void WhenAll12_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task12"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null,
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            (Task<string>)null,
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task10"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            (Task<string>)null,
            Task.FromResult(12),
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task11"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            (Task<string>)null,
            Task.FromResult(13)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task12"));
    }

    [Test]
    public static void WhenAll13_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task13"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null,
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            (Task<string>)null,
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task10"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            (Task<string>)null,
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task11"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            (Task<string>)null,
            Task.FromResult(13),
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task12"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            (Task<string>)null,
            Task.FromResult(14)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task13"));
    }

    [Test]
    public static void WhenAll14_GivenNullArg14_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task14"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null,
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            (Task<string>)null,
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task10"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            (Task<string>)null,
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task11"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            (Task<string>)null,
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task12"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            (Task<string>)null,
            Task.FromResult(14),
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task13"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg14_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            (Task<string>)null,
            Task.FromResult(15)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task14"));
    }

    [Test]
    public static void WhenAll15_GivenNullArg15_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task15"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => (
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task1"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task2"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task3"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task4"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task5"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task6"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task7"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task8"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null,
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task9"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            (Task<string>)null,
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task10"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            (Task<string>)null,
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task11"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            (Task<string>)null,
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task12"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            (Task<string>)null,
            Task.FromResult(14),
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task13"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg14_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            (Task<string>)null,
            Task.FromResult(15),
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task14"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg15_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            (Task<string>)null,
            Task.FromResult(16)
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task15"));
    }

    [Test]
    public static void WhenAll16_GivenNullArg16_ThrowsArgNullException()
    {
        Assert.That(() => (
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9),
            Task.FromResult(10),
            Task.FromResult(11),
            Task.FromResult(12),
            Task.FromResult(13),
            Task.FromResult(14),
            Task.FromResult(15),
            (Task<string>)null
        ).WhenAll(), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tasks.task16"));
    }

    [Test]
    public static void WhenAll_GivenCompletedTasks_CompletesSynchronously()
    {
        var result = (
            Task.FromResult(1),
            Task.FromResult("2")
        ).WhenAll();

        Assert.That(result.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public static async Task WhenAll_GivenIncompleteTasks_DoesNotCompleteSynchronously()
    {
        var source1 = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var source2 = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        var result = (
            source1.Task,
            source2.Task
        ).WhenAll();

        Assert.That(result.IsCompleted, Is.False);

        source1.SetResult(1);
        source2.SetResult("2");

        Assert.That(await result, Is.EqualTo((1, "2")));
    }

    [Test]
    public static async Task WhenAll2_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result
        ) = await (
            Incomplete(1),
            Incomplete("2")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
        }
    }

    [Test]
    public static void WhenAll2_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll2_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll3_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
        }
    }

    [Test]
    public static void WhenAll3_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll3_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll4_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
        }
    }

    [Test]
    public static void WhenAll4_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll4_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll5_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
        }
    }

    [Test]
    public static void WhenAll5_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll5_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll6_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
        }
    }

    [Test]
    public static void WhenAll6_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll6_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll7_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
        }
    }

    [Test]
    public static void WhenAll7_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll7_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll8_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
        }
    }

    [Test]
    public static void WhenAll8_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll8_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll9_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
        }
    }

    [Test]
    public static void WhenAll9_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll9_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll10_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9),
            Incomplete("10")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
        }
    }

    [Test]
    public static void WhenAll10_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll10_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll11_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9),
            Incomplete("10"),
            Incomplete(11)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
        }
    }

    [Test]
    public static void WhenAll11_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll11_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll12_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9),
            Incomplete("10"),
            Incomplete(11),
            Incomplete("12")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
        }
    }

    [Test]
    public static void WhenAll12_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll12_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll13_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9),
            Incomplete("10"),
            Incomplete(11),
            Incomplete("12"),
            Incomplete(13)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
        }
    }

    [Test]
    public static void WhenAll13_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll13_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll14_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result,
            task14Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9),
            Incomplete("10"),
            Incomplete(11),
            Incomplete("12"),
            Incomplete(13),
            Incomplete("14")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
            Assert.That(task14Result, Is.EqualTo("14"));
        }
    }

    [Test]
    public static void WhenAll14_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll14_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll15_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result,
            task14Result,
            task15Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9),
            Incomplete("10"),
            Incomplete(11),
            Incomplete("12"),
            Incomplete(13),
            Incomplete("14"),
            Incomplete(15)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
            Assert.That(task14Result, Is.EqualTo("14"));
            Assert.That(task15Result, Is.EqualTo(15));
        }
    }

    [Test]
    public static void WhenAll15_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromResult("14"),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll15_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromResult("14"),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    [Test]
    public static async Task WhenAll16_GivenIncompleteTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result,
            task7Result,
            task8Result,
            task9Result,
            task10Result,
            task11Result,
            task12Result,
            task13Result,
            task14Result,
            task15Result,
            task16Result
        ) = await (
            Incomplete(1),
            Incomplete("2"),
            Incomplete(3),
            Incomplete("4"),
            Incomplete(5),
            Incomplete("6"),
            Incomplete(7),
            Incomplete("8"),
            Incomplete(9),
            Incomplete("10"),
            Incomplete(11),
            Incomplete("12"),
            Incomplete(13),
            Incomplete("14"),
            Incomplete(15),
            Incomplete("16")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
            Assert.That(task4Result, Is.EqualTo("4"));
            Assert.That(task5Result, Is.EqualTo(5));
            Assert.That(task6Result, Is.EqualTo("6"));
            Assert.That(task7Result, Is.EqualTo(7));
            Assert.That(task8Result, Is.EqualTo("8"));
            Assert.That(task9Result, Is.EqualTo(9));
            Assert.That(task10Result, Is.EqualTo("10"));
            Assert.That(task11Result, Is.EqualTo(11));
            Assert.That(task12Result, Is.EqualTo("12"));
            Assert.That(task13Result, Is.EqualTo(13));
            Assert.That(task14Result, Is.EqualTo("14"));
            Assert.That(task15Result, Is.EqualTo(15));
            Assert.That(task16Result, Is.EqualTo("16"));
        }
    }

    [Test]
    public static void WhenAll16_GivenFaultedTask_ThrowsException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromResult("14"),
            Task.FromResult(15),
            Task.FromException<int>(new InvalidOperationException())
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static void WhenAll16_GivenCancelledTask_ThrowsTaskCanceledException()
    {
        Assert.That(async () => await (
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9),
            Task.FromResult("10"),
            Task.FromResult(11),
            Task.FromResult("12"),
            Task.FromResult(13),
            Task.FromResult("14"),
            Task.FromResult(15),
            Task.FromCanceled<int>(new CancellationToken(true))
        ).WhenAll(), Throws.InstanceOf<TaskCanceledException>());
    }

    private static async Task<T> Incomplete<T>(T value)
    {
        await Task.Yield();

        return value;
    }
}
