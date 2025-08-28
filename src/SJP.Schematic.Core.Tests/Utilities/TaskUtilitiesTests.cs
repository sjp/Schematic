using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class TaskUtilitiesTests
{
    [Test]
    public static async Task WhenAll2_GivenArgs_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2")
        );

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
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3)
        );

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
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4")
        );

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
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5)
        );

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
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6")
        );

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
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7)
        );

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
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8")
        );

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
        ) = await TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult("2"),
            Task.FromResult(3),
            Task.FromResult("4"),
            Task.FromResult(5),
            Task.FromResult("6"),
            Task.FromResult(7),
            Task.FromResult("8"),
            Task.FromResult(9)
        );

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
        ) = await TaskUtilities.WhenAll(
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
        );

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
        ) = await TaskUtilities.WhenAll(
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
        );

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
        ) = await TaskUtilities.WhenAll(
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
        );

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
        ) = await TaskUtilities.WhenAll(
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
        );

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
        ) = await TaskUtilities.WhenAll(
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
        );

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
        ) = await TaskUtilities.WhenAll(
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
        );

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
        ) = await TaskUtilities.WhenAll(
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
        );

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
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll2_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll3_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll3_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll3_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll4_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll4_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll4_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll4_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll5_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll5_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll5_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll5_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll5_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll6_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll6_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll6_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll6_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll6_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll6_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll7_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll7_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll7_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll7_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll7_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll7_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll7_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll8_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            (Task<string>)null,
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            (Task<string>)null,
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            (Task<string>)null,
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            (Task<string>)null,
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            (Task<string>)null,
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            (Task<string>)null,
            Task.FromResult(7),
            Task.FromResult(8),
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            (Task<string>)null,
            Task.FromResult(8),
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            (Task<string>)null,
            Task.FromResult(9)
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll9_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
            Task.FromResult(1),
            Task.FromResult(2),
            Task.FromResult(3),
            Task.FromResult(4),
            Task.FromResult(5),
            Task.FromResult(6),
            Task.FromResult(7),
            Task.FromResult(8),
            (Task<string>)null
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll10_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll11_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll12_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll13_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll14_GivenNullArg14_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg14_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll15_GivenNullArg15_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg1_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg2_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg3_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg4_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg5_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg6_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg7_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg8_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg9_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg10_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg11_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg12_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg13_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg14_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg15_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }

    [Test]
    public static void WhenAll16_GivenNullArg16_ThrowsArgNullException()
    {
        Assert.That(() => TaskUtilities.WhenAll(
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
        ), Throws.ArgumentNullException);
    }
}