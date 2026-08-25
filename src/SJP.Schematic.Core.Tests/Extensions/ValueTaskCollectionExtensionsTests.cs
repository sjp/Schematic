using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class ValueTaskCollectionExtensionsTests
{
    [Test]
    public static void WhenAll_GivenCompletedValueTasks_CompletesSynchronously()
    {
        var result = (
            ValueTask.FromResult(1),
            ValueTask.FromResult("2")
        ).WhenAll();

        Assert.That(result.IsCompletedSuccessfully, Is.True);
    }

    [Test]
    public static void WhenAll_GivenIncompleteValueTasks_DoesNotCompleteSynchronously()
    {
        var result = (
            Incomplete(1),
            Incomplete("2")
        ).WhenAll();

        Assert.That(result.IsCompleted, Is.False);
    }

    [Test]
    public static void WhenAll_GivenFaultedValueTask_ThrowsException()
    {
        var faulted = new ValueTask<int>(Task.FromException<int>(new InvalidOperationException()));

        Assert.That(async () => await (
            faulted,
            ValueTask.FromResult("2")
        ).WhenAll(), Throws.InvalidOperationException);
    }

    [Test]
    public static async Task WhenAll2_GivenCompletedValueTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result
        ) = await (
            ValueTask.FromResult(1),
            ValueTask.FromResult("2")
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
        }
    }

    [Test]
    public static async Task WhenAll2_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll3_GivenCompletedValueTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result
        ) = await (
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3)
        ).WhenAll();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(task1Result, Is.EqualTo(1));
            Assert.That(task2Result, Is.EqualTo("2"));
            Assert.That(task3Result, Is.EqualTo(3));
        }
    }

    [Test]
    public static async Task WhenAll3_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll4_GivenCompletedValueTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result
        ) = await (
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4")
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
    public static async Task WhenAll4_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll5_GivenCompletedValueTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result
        ) = await (
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5)
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
    public static async Task WhenAll5_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll6_GivenCompletedValueTasks_ReturnsExpectedValues()
    {
        var (
            task1Result,
            task2Result,
            task3Result,
            task4Result,
            task5Result,
            task6Result
        ) = await (
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6")
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
    public static async Task WhenAll6_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll7_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7)
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
    public static async Task WhenAll7_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll8_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8")
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
    public static async Task WhenAll8_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll9_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9)
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
    public static async Task WhenAll9_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll10_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9),
            ValueTask.FromResult("10")
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
    public static async Task WhenAll10_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll11_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9),
            ValueTask.FromResult("10"),
            ValueTask.FromResult(11)
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
    public static async Task WhenAll11_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll12_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9),
            ValueTask.FromResult("10"),
            ValueTask.FromResult(11),
            ValueTask.FromResult("12")
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
    public static async Task WhenAll12_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll13_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9),
            ValueTask.FromResult("10"),
            ValueTask.FromResult(11),
            ValueTask.FromResult("12"),
            ValueTask.FromResult(13)
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
    public static async Task WhenAll13_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll14_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9),
            ValueTask.FromResult("10"),
            ValueTask.FromResult(11),
            ValueTask.FromResult("12"),
            ValueTask.FromResult(13),
            ValueTask.FromResult("14")
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
    public static async Task WhenAll14_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll15_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9),
            ValueTask.FromResult("10"),
            ValueTask.FromResult(11),
            ValueTask.FromResult("12"),
            ValueTask.FromResult(13),
            ValueTask.FromResult("14"),
            ValueTask.FromResult(15)
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
    public static async Task WhenAll15_GivenIncompleteValueTasks_ReturnsExpectedValues()
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
    public static async Task WhenAll16_GivenCompletedValueTasks_ReturnsExpectedValues()
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
            ValueTask.FromResult(1),
            ValueTask.FromResult("2"),
            ValueTask.FromResult(3),
            ValueTask.FromResult("4"),
            ValueTask.FromResult(5),
            ValueTask.FromResult("6"),
            ValueTask.FromResult(7),
            ValueTask.FromResult("8"),
            ValueTask.FromResult(9),
            ValueTask.FromResult("10"),
            ValueTask.FromResult(11),
            ValueTask.FromResult("12"),
            ValueTask.FromResult(13),
            ValueTask.FromResult("14"),
            ValueTask.FromResult(15),
            ValueTask.FromResult("16")
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
    public static async Task WhenAll16_GivenIncompleteValueTasks_ReturnsExpectedValues()
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

    private static async ValueTask<T> Incomplete<T>(T value)
    {
        await Task.Yield();

        return value;
    }
}
