using System;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// Convenience extension methods for working with <see cref="Task"/> objects.
/// </summary>
public static class TaskUtilities
{
    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2)> WhenAll<T1, T2>(this (Task<T1> task1, Task<T2> task2) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);

        return WhenAllCore(
            tasks.task1,
            tasks.task2
        );
    }

    private static async Task<(T1, T2)> WhenAllCore<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
        var tasks = new Task[] { task1, task2 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3
        );
    }

    private static async Task<(T1, T2, T3)> WhenAllCore<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
    {
        var tasks = new Task[] { task1, task2, task3 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4
        );
    }

    private static async Task<(T1, T2, T3, T4)> WhenAllCore<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
    {
        var tasks = new Task[] { task1, task2, task3, task4 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5
        );
    }

    private static async Task<(T1, T2, T3, T4, T5)> WhenAllCore<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6)> WhenAllCore<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the tenth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);
        ArgumentNullException.ThrowIfNull(tasks.task10);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9,
            tasks.task10
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false),
            await task10.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the tenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the eleventh task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);
        ArgumentNullException.ThrowIfNull(tasks.task10);
        ArgumentNullException.ThrowIfNull(tasks.task11);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9,
            tasks.task10,
            tasks.task11
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false),
            await task10.ConfigureAwait(false),
            await task11.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the tenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the eleventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the twelfth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);
        ArgumentNullException.ThrowIfNull(tasks.task10);
        ArgumentNullException.ThrowIfNull(tasks.task11);
        ArgumentNullException.ThrowIfNull(tasks.task12);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9,
            tasks.task10,
            tasks.task11,
            tasks.task12
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false),
            await task10.ConfigureAwait(false),
            await task11.ConfigureAwait(false),
            await task12.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the tenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the eleventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the twelfth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the thirteenth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);
        ArgumentNullException.ThrowIfNull(tasks.task10);
        ArgumentNullException.ThrowIfNull(tasks.task11);
        ArgumentNullException.ThrowIfNull(tasks.task12);
        ArgumentNullException.ThrowIfNull(tasks.task13);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9,
            tasks.task10,
            tasks.task11,
            tasks.task12,
            tasks.task13
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false),
            await task10.ConfigureAwait(false),
            await task11.ConfigureAwait(false),
            await task12.ConfigureAwait(false),
            await task13.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the tenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the eleventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the twelfth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the thirteenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T14">The type of the result produced by the fourteenth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);
        ArgumentNullException.ThrowIfNull(tasks.task10);
        ArgumentNullException.ThrowIfNull(tasks.task11);
        ArgumentNullException.ThrowIfNull(tasks.task12);
        ArgumentNullException.ThrowIfNull(tasks.task13);
        ArgumentNullException.ThrowIfNull(tasks.task14);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9,
            tasks.task10,
            tasks.task11,
            tasks.task12,
            tasks.task13,
            tasks.task14
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false),
            await task10.ConfigureAwait(false),
            await task11.ConfigureAwait(false),
            await task12.ConfigureAwait(false),
            await task13.ConfigureAwait(false),
            await task14.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the tenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the eleventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the twelfth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the thirteenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T14">The type of the result produced by the fourteenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T15">The type of the result produced by the fifteenth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14, Task<T15> task15) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);
        ArgumentNullException.ThrowIfNull(tasks.task10);
        ArgumentNullException.ThrowIfNull(tasks.task11);
        ArgumentNullException.ThrowIfNull(tasks.task12);
        ArgumentNullException.ThrowIfNull(tasks.task13);
        ArgumentNullException.ThrowIfNull(tasks.task14);
        ArgumentNullException.ThrowIfNull(tasks.task15);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9,
            tasks.task10,
            tasks.task11,
            tasks.task12,
            tasks.task13,
            tasks.task14,
            tasks.task15
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14, Task<T15> task15)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14, task15 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false),
            await task10.ConfigureAwait(false),
            await task11.ConfigureAwait(false),
            await task12.ConfigureAwait(false),
            await task13.ConfigureAwait(false),
            await task14.ConfigureAwait(false),
            await task15.ConfigureAwait(false)
        );
    }

    /// <summary>
    /// Creates a task that will complete when all of the provided <see cref="Task"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the eighth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the ninth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the tenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the eleventh task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the twelfth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the thirteenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T14">The type of the result produced by the fourteenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T15">The type of the result produced by the fifteenth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T16">The type of the result produced by the sixteenth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="Task"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of the tasks in the <paramref name="tasks"/> tuple is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14, Task<T15> task15, Task<T16> task16) tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks.task1);
        ArgumentNullException.ThrowIfNull(tasks.task2);
        ArgumentNullException.ThrowIfNull(tasks.task3);
        ArgumentNullException.ThrowIfNull(tasks.task4);
        ArgumentNullException.ThrowIfNull(tasks.task5);
        ArgumentNullException.ThrowIfNull(tasks.task6);
        ArgumentNullException.ThrowIfNull(tasks.task7);
        ArgumentNullException.ThrowIfNull(tasks.task8);
        ArgumentNullException.ThrowIfNull(tasks.task9);
        ArgumentNullException.ThrowIfNull(tasks.task10);
        ArgumentNullException.ThrowIfNull(tasks.task11);
        ArgumentNullException.ThrowIfNull(tasks.task12);
        ArgumentNullException.ThrowIfNull(tasks.task13);
        ArgumentNullException.ThrowIfNull(tasks.task14);
        ArgumentNullException.ThrowIfNull(tasks.task15);
        ArgumentNullException.ThrowIfNull(tasks.task16);

        return WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9,
            tasks.task10,
            tasks.task11,
            tasks.task12,
            tasks.task13,
            tasks.task14,
            tasks.task15,
            tasks.task16
        );
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14, Task<T15> task15, Task<T16> task16)
    {
        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14, task15, task16 };

        await Task.WhenAll(tasks).ConfigureAwait(false);

        return (
            await task1.ConfigureAwait(false),
            await task2.ConfigureAwait(false),
            await task3.ConfigureAwait(false),
            await task4.ConfigureAwait(false),
            await task5.ConfigureAwait(false),
            await task6.ConfigureAwait(false),
            await task7.ConfigureAwait(false),
            await task8.ConfigureAwait(false),
            await task9.ConfigureAwait(false),
            await task10.ConfigureAwait(false),
            await task11.ConfigureAwait(false),
            await task12.ConfigureAwait(false),
            await task13.ConfigureAwait(false),
            await task14.ConfigureAwait(false),
            await task15.ConfigureAwait(false),
            await task16.ConfigureAwait(false)
        );
    }
}