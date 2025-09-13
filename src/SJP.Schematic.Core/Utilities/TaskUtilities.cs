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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);

        return WhenAllCore(
            task1,
            task2
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);

        return WhenAllCore(
            task1,
            task2,
            task3
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the <see cref="Task"/> for <paramref name="task10"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <param name="task10">A <see cref="Task"/> that produces a value of type <typeparamref name="T10"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/>, <paramref name="task10"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9,
            task10
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the <see cref="Task"/> for <paramref name="task10"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the <see cref="Task"/> for <paramref name="task11"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <param name="task10">A <see cref="Task"/> that produces a value of type <typeparamref name="T10"/>.</param>
    /// <param name="task11">A <see cref="Task"/> that produces a value of type <typeparamref name="T11"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/>, <paramref name="task10"/>, <paramref name="task11"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);
        ArgumentNullException.ThrowIfNull(task11);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9,
            task10,
            task11
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the <see cref="Task"/> for <paramref name="task10"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the <see cref="Task"/> for <paramref name="task11"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the <see cref="Task"/> for <paramref name="task12"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <param name="task10">A <see cref="Task"/> that produces a value of type <typeparamref name="T10"/>.</param>
    /// <param name="task11">A <see cref="Task"/> that produces a value of type <typeparamref name="T11"/>.</param>
    /// <param name="task12">A <see cref="Task"/> that produces a value of type <typeparamref name="T12"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/>, <paramref name="task10"/>, <paramref name="task11"/>, <paramref name="task12"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);
        ArgumentNullException.ThrowIfNull(task11);
        ArgumentNullException.ThrowIfNull(task12);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9,
            task10,
            task11,
            task12
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the <see cref="Task"/> for <paramref name="task10"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the <see cref="Task"/> for <paramref name="task11"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the <see cref="Task"/> for <paramref name="task12"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the <see cref="Task"/> for <paramref name="task13"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <param name="task10">A <see cref="Task"/> that produces a value of type <typeparamref name="T10"/>.</param>
    /// <param name="task11">A <see cref="Task"/> that produces a value of type <typeparamref name="T11"/>.</param>
    /// <param name="task12">A <see cref="Task"/> that produces a value of type <typeparamref name="T12"/>.</param>
    /// <param name="task13">A <see cref="Task"/> that produces a value of type <typeparamref name="T13"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/>, <paramref name="task10"/>, <paramref name="task11"/>, <paramref name="task12"/>, <paramref name="task13"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);
        ArgumentNullException.ThrowIfNull(task11);
        ArgumentNullException.ThrowIfNull(task12);
        ArgumentNullException.ThrowIfNull(task13);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9,
            task10,
            task11,
            task12,
            task13
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the <see cref="Task"/> for <paramref name="task10"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the <see cref="Task"/> for <paramref name="task11"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the <see cref="Task"/> for <paramref name="task12"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the <see cref="Task"/> for <paramref name="task13"/>.</typeparam>
    /// <typeparam name="T14">The type of the result produced by the <see cref="Task"/> for <paramref name="task14"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <param name="task10">A <see cref="Task"/> that produces a value of type <typeparamref name="T10"/>.</param>
    /// <param name="task11">A <see cref="Task"/> that produces a value of type <typeparamref name="T11"/>.</param>
    /// <param name="task12">A <see cref="Task"/> that produces a value of type <typeparamref name="T12"/>.</param>
    /// <param name="task13">A <see cref="Task"/> that produces a value of type <typeparamref name="T13"/>.</param>
    /// <param name="task14">A <see cref="Task"/> that produces a value of type <typeparamref name="T14"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/>, <paramref name="task10"/>, <paramref name="task11"/>, <paramref name="task12"/>, <paramref name="task13"/>, <paramref name="task14"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);
        ArgumentNullException.ThrowIfNull(task11);
        ArgumentNullException.ThrowIfNull(task12);
        ArgumentNullException.ThrowIfNull(task13);
        ArgumentNullException.ThrowIfNull(task14);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9,
            task10,
            task11,
            task12,
            task13,
            task14
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the <see cref="Task"/> for <paramref name="task10"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the <see cref="Task"/> for <paramref name="task11"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the <see cref="Task"/> for <paramref name="task12"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the <see cref="Task"/> for <paramref name="task13"/>.</typeparam>
    /// <typeparam name="T14">The type of the result produced by the <see cref="Task"/> for <paramref name="task14"/>.</typeparam>
    /// <typeparam name="T15">The type of the result produced by the <see cref="Task"/> for <paramref name="task15"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <param name="task10">A <see cref="Task"/> that produces a value of type <typeparamref name="T10"/>.</param>
    /// <param name="task11">A <see cref="Task"/> that produces a value of type <typeparamref name="T11"/>.</param>
    /// <param name="task12">A <see cref="Task"/> that produces a value of type <typeparamref name="T12"/>.</param>
    /// <param name="task13">A <see cref="Task"/> that produces a value of type <typeparamref name="T13"/>.</param>
    /// <param name="task14">A <see cref="Task"/> that produces a value of type <typeparamref name="T14"/>.</param>
    /// <param name="task15">A <see cref="Task"/> that produces a value of type <typeparamref name="T15"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/>, <paramref name="task10"/>, <paramref name="task11"/>, <paramref name="task12"/>, <paramref name="task13"/>, <paramref name="task14"/>, <paramref name="task15"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14, Task<T15> task15)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);
        ArgumentNullException.ThrowIfNull(task11);
        ArgumentNullException.ThrowIfNull(task12);
        ArgumentNullException.ThrowIfNull(task13);
        ArgumentNullException.ThrowIfNull(task14);
        ArgumentNullException.ThrowIfNull(task15);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9,
            task10,
            task11,
            task12,
            task13,
            task14,
            task15
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
    /// <typeparam name="T1">The type of the result produced by the <see cref="Task"/> for <paramref name="task1"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the <see cref="Task"/> for <paramref name="task2"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the <see cref="Task"/> for <paramref name="task3"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the <see cref="Task"/> for <paramref name="task4"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the <see cref="Task"/> for <paramref name="task5"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the <see cref="Task"/> for <paramref name="task6"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the <see cref="Task"/> for <paramref name="task7"/>.</typeparam>
    /// <typeparam name="T8">The type of the result produced by the <see cref="Task"/> for <paramref name="task8"/>.</typeparam>
    /// <typeparam name="T9">The type of the result produced by the <see cref="Task"/> for <paramref name="task9"/>.</typeparam>
    /// <typeparam name="T10">The type of the result produced by the <see cref="Task"/> for <paramref name="task10"/>.</typeparam>
    /// <typeparam name="T11">The type of the result produced by the <see cref="Task"/> for <paramref name="task11"/>.</typeparam>
    /// <typeparam name="T12">The type of the result produced by the <see cref="Task"/> for <paramref name="task12"/>.</typeparam>
    /// <typeparam name="T13">The type of the result produced by the <see cref="Task"/> for <paramref name="task13"/>.</typeparam>
    /// <typeparam name="T14">The type of the result produced by the <see cref="Task"/> for <paramref name="task14"/>.</typeparam>
    /// <typeparam name="T15">The type of the result produced by the <see cref="Task"/> for <paramref name="task15"/>.</typeparam>
    /// <typeparam name="T16">The type of the result produced by the <see cref="Task"/> for <paramref name="task16"/>.</typeparam>
    /// <param name="task1">A <see cref="Task"/> that produces a value of type <typeparamref name="T1"/>.</param>
    /// <param name="task2">A <see cref="Task"/> that produces a value of type <typeparamref name="T2"/>.</param>
    /// <param name="task3">A <see cref="Task"/> that produces a value of type <typeparamref name="T3"/>.</param>
    /// <param name="task4">A <see cref="Task"/> that produces a value of type <typeparamref name="T4"/>.</param>
    /// <param name="task5">A <see cref="Task"/> that produces a value of type <typeparamref name="T5"/>.</param>
    /// <param name="task6">A <see cref="Task"/> that produces a value of type <typeparamref name="T6"/>.</param>
    /// <param name="task7">A <see cref="Task"/> that produces a value of type <typeparamref name="T7"/>.</param>
    /// <param name="task8">A <see cref="Task"/> that produces a value of type <typeparamref name="T8"/>.</param>
    /// <param name="task9">A <see cref="Task"/> that produces a value of type <typeparamref name="T9"/>.</param>
    /// <param name="task10">A <see cref="Task"/> that produces a value of type <typeparamref name="T10"/>.</param>
    /// <param name="task11">A <see cref="Task"/> that produces a value of type <typeparamref name="T11"/>.</param>
    /// <param name="task12">A <see cref="Task"/> that produces a value of type <typeparamref name="T12"/>.</param>
    /// <param name="task13">A <see cref="Task"/> that produces a value of type <typeparamref name="T13"/>.</param>
    /// <param name="task14">A <see cref="Task"/> that produces a value of type <typeparamref name="T14"/>.</param>
    /// <param name="task15">A <see cref="Task"/> that produces a value of type <typeparamref name="T15"/>.</param>
    /// <param name="task16">A <see cref="Task"/> that produces a value of type <typeparamref name="T16"/>.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="Task"/> operations.</returns>
    /// <exception cref="ArgumentNullException">One of <paramref name="task1"/>, <paramref name="task2"/>, <paramref name="task3"/>, <paramref name="task4"/>, <paramref name="task5"/>, <paramref name="task6"/>, <paramref name="task7"/>, <paramref name="task8"/>, <paramref name="task9"/>, <paramref name="task10"/>, <paramref name="task11"/>, <paramref name="task12"/>, <paramref name="task13"/>, <paramref name="task14"/>, <paramref name="task15"/>, <paramref name="task16"/> is <see langword="null" />.</exception>
    public static Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9, Task<T10> task10, Task<T11> task11, Task<T12> task12, Task<T13> task13, Task<T14> task14, Task<T15> task15, Task<T16> task16)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);
        ArgumentNullException.ThrowIfNull(task11);
        ArgumentNullException.ThrowIfNull(task12);
        ArgumentNullException.ThrowIfNull(task13);
        ArgumentNullException.ThrowIfNull(task14);
        ArgumentNullException.ThrowIfNull(task15);
        ArgumentNullException.ThrowIfNull(task16);

        return WhenAllCore(
            task1,
            task2,
            task3,
            task4,
            task5,
            task6,
            task7,
            task8,
            task9,
            task10,
            task11,
            task12,
            task13,
            task14,
            task15,
            task16
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