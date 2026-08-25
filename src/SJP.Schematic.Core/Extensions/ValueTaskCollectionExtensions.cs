using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Convenience extension methods for working with tuples of <see cref="ValueTask{TResult}"/> objects.
/// </summary>
public static class ValueTaskCollectionExtensions
{
    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2)> WhenAll<T1, T2>(this (ValueTask<T1> task1, ValueTask<T2> task2) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2)>((
                tasks.task1.Result,
                tasks.task2.Result
            ));
        }

        return new ValueTask<(T1, T2)>(WhenAllCore(
            tasks.task1,
            tasks.task2
        ));
    }

    private static async Task<(T1, T2)> WhenAllCore<T1, T2>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();

        var tasks = new Task[] { task1, task2 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3)> WhenAll<T1, T2, T3>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result
            ));
        }

        return new ValueTask<(T1, T2, T3)>(WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3
        ));
    }

    private static async Task<(T1, T2, T3)> WhenAllCore<T1, T2, T3>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();

        var tasks = new Task[] { task1, task2, task3 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4)>(WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4
        ));
    }

    private static async Task<(T1, T2, T3, T4)> WhenAllCore<T1, T2, T3, T4>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5)>(WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5)> WhenAllCore<T1, T2, T3, T4, T5>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6)>(WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6)> WhenAllCore<T1, T2, T3, T4, T5, T6>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
    /// The evaluated results are available in a tuple.
    /// </summary>
    /// <typeparam name="T1">The type of the result produced by the first task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T2">The type of the result produced by the second task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T3">The type of the result produced by the third task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T4">The type of the result produced by the fourth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T5">The type of the result produced by the fifth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T6">The type of the result produced by the sixth task in <paramref name="tasks"/>.</typeparam>
    /// <typeparam name="T7">The type of the result produced by the seventh task in <paramref name="tasks"/>.</typeparam>
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7)>(WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8)>(WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>(WhenAllCore(
            tasks.task1,
            tasks.task2,
            tasks.task3,
            tasks.task4,
            tasks.task5,
            tasks.task6,
            tasks.task7,
            tasks.task8,
            tasks.task9
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9, ValueTask<T10> task10) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully
            && tasks.task10.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result,
                tasks.task10.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>(WhenAllCore(
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
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9, ValueTask<T10> valueTask10)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();
        var task10 = valueTask10.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9,
            await task10
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9, ValueTask<T10> task10, ValueTask<T11> task11) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully
            && tasks.task10.IsCompletedSuccessfully
            && tasks.task11.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result,
                tasks.task10.Result,
                tasks.task11.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>(WhenAllCore(
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
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9, ValueTask<T10> valueTask10, ValueTask<T11> valueTask11)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();
        var task10 = valueTask10.AsTask();
        var task11 = valueTask11.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9,
            await task10,
            await task11
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9, ValueTask<T10> task10, ValueTask<T11> task11, ValueTask<T12> task12) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully
            && tasks.task10.IsCompletedSuccessfully
            && tasks.task11.IsCompletedSuccessfully
            && tasks.task12.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result,
                tasks.task10.Result,
                tasks.task11.Result,
                tasks.task12.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>(WhenAllCore(
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
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9, ValueTask<T10> valueTask10, ValueTask<T11> valueTask11, ValueTask<T12> valueTask12)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();
        var task10 = valueTask10.AsTask();
        var task11 = valueTask11.AsTask();
        var task12 = valueTask12.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9,
            await task10,
            await task11,
            await task12
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9, ValueTask<T10> task10, ValueTask<T11> task11, ValueTask<T12> task12, ValueTask<T13> task13) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully
            && tasks.task10.IsCompletedSuccessfully
            && tasks.task11.IsCompletedSuccessfully
            && tasks.task12.IsCompletedSuccessfully
            && tasks.task13.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result,
                tasks.task10.Result,
                tasks.task11.Result,
                tasks.task12.Result,
                tasks.task13.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>(WhenAllCore(
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
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9, ValueTask<T10> valueTask10, ValueTask<T11> valueTask11, ValueTask<T12> valueTask12, ValueTask<T13> valueTask13)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();
        var task10 = valueTask10.AsTask();
        var task11 = valueTask11.AsTask();
        var task12 = valueTask12.AsTask();
        var task13 = valueTask13.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9,
            await task10,
            await task11,
            await task12,
            await task13
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9, ValueTask<T10> task10, ValueTask<T11> task11, ValueTask<T12> task12, ValueTask<T13> task13, ValueTask<T14> task14) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully
            && tasks.task10.IsCompletedSuccessfully
            && tasks.task11.IsCompletedSuccessfully
            && tasks.task12.IsCompletedSuccessfully
            && tasks.task13.IsCompletedSuccessfully
            && tasks.task14.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result,
                tasks.task10.Result,
                tasks.task11.Result,
                tasks.task12.Result,
                tasks.task13.Result,
                tasks.task14.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>(WhenAllCore(
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
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9, ValueTask<T10> valueTask10, ValueTask<T11> valueTask11, ValueTask<T12> valueTask12, ValueTask<T13> valueTask13, ValueTask<T14> valueTask14)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();
        var task10 = valueTask10.AsTask();
        var task11 = valueTask11.AsTask();
        var task12 = valueTask12.AsTask();
        var task13 = valueTask13.AsTask();
        var task14 = valueTask14.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9,
            await task10,
            await task11,
            await task12,
            await task13,
            await task14
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9, ValueTask<T10> task10, ValueTask<T11> task11, ValueTask<T12> task12, ValueTask<T13> task13, ValueTask<T14> task14, ValueTask<T15> task15) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully
            && tasks.task10.IsCompletedSuccessfully
            && tasks.task11.IsCompletedSuccessfully
            && tasks.task12.IsCompletedSuccessfully
            && tasks.task13.IsCompletedSuccessfully
            && tasks.task14.IsCompletedSuccessfully
            && tasks.task15.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result,
                tasks.task10.Result,
                tasks.task11.Result,
                tasks.task12.Result,
                tasks.task13.Result,
                tasks.task14.Result,
                tasks.task15.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)>(WhenAllCore(
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
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9, ValueTask<T10> valueTask10, ValueTask<T11> valueTask11, ValueTask<T12> valueTask12, ValueTask<T13> valueTask13, ValueTask<T14> valueTask14, ValueTask<T15> valueTask15)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();
        var task10 = valueTask10.AsTask();
        var task11 = valueTask11.AsTask();
        var task12 = valueTask12.AsTask();
        var task13 = valueTask13.AsTask();
        var task14 = valueTask14.AsTask();
        var task15 = valueTask15.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14, task15 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9,
            await task10,
            await task11,
            await task12,
            await task13,
            await task14,
            await task15
        );
    }

    /// <summary>
    /// Creates a value task that will complete when all of the provided <see cref="ValueTask{TResult}"/> objects have completed.
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
    /// <param name="tasks">A tuple of <see cref="ValueTask{TResult}"/> objects.</param>
    /// <returns>A tuple containing the resulting values from all of the completed <see cref="ValueTask{TResult}"/> operations.</returns>
    public static ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this (ValueTask<T1> task1, ValueTask<T2> task2, ValueTask<T3> task3, ValueTask<T4> task4, ValueTask<T5> task5, ValueTask<T6> task6, ValueTask<T7> task7, ValueTask<T8> task8, ValueTask<T9> task9, ValueTask<T10> task10, ValueTask<T11> task11, ValueTask<T12> task12, ValueTask<T13> task13, ValueTask<T14> task14, ValueTask<T15> task15, ValueTask<T16> task16) tasks)
    {
        if (tasks.task1.IsCompletedSuccessfully
            && tasks.task2.IsCompletedSuccessfully
            && tasks.task3.IsCompletedSuccessfully
            && tasks.task4.IsCompletedSuccessfully
            && tasks.task5.IsCompletedSuccessfully
            && tasks.task6.IsCompletedSuccessfully
            && tasks.task7.IsCompletedSuccessfully
            && tasks.task8.IsCompletedSuccessfully
            && tasks.task9.IsCompletedSuccessfully
            && tasks.task10.IsCompletedSuccessfully
            && tasks.task11.IsCompletedSuccessfully
            && tasks.task12.IsCompletedSuccessfully
            && tasks.task13.IsCompletedSuccessfully
            && tasks.task14.IsCompletedSuccessfully
            && tasks.task15.IsCompletedSuccessfully
            && tasks.task16.IsCompletedSuccessfully)
        {
            return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)>((
                tasks.task1.Result,
                tasks.task2.Result,
                tasks.task3.Result,
                tasks.task4.Result,
                tasks.task5.Result,
                tasks.task6.Result,
                tasks.task7.Result,
                tasks.task8.Result,
                tasks.task9.Result,
                tasks.task10.Result,
                tasks.task11.Result,
                tasks.task12.Result,
                tasks.task13.Result,
                tasks.task14.Result,
                tasks.task15.Result,
                tasks.task16.Result
            ));
        }

        return new ValueTask<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)>(WhenAllCore(
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
        ));
    }

    private static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> WhenAllCore<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(ValueTask<T1> valueTask1, ValueTask<T2> valueTask2, ValueTask<T3> valueTask3, ValueTask<T4> valueTask4, ValueTask<T5> valueTask5, ValueTask<T6> valueTask6, ValueTask<T7> valueTask7, ValueTask<T8> valueTask8, ValueTask<T9> valueTask9, ValueTask<T10> valueTask10, ValueTask<T11> valueTask11, ValueTask<T12> valueTask12, ValueTask<T13> valueTask13, ValueTask<T14> valueTask14, ValueTask<T15> valueTask15, ValueTask<T16> valueTask16)
    {
        var task1 = valueTask1.AsTask();
        var task2 = valueTask2.AsTask();
        var task3 = valueTask3.AsTask();
        var task4 = valueTask4.AsTask();
        var task5 = valueTask5.AsTask();
        var task6 = valueTask6.AsTask();
        var task7 = valueTask7.AsTask();
        var task8 = valueTask8.AsTask();
        var task9 = valueTask9.AsTask();
        var task10 = valueTask10.AsTask();
        var task11 = valueTask11.AsTask();
        var task12 = valueTask12.AsTask();
        var task13 = valueTask13.AsTask();
        var task14 = valueTask14.AsTask();
        var task15 = valueTask15.AsTask();
        var task16 = valueTask16.AsTask();

        var tasks = new Task[] { task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12, task13, task14, task15, task16 };

        await Task.WhenAll(tasks);

        return (
            await task1,
            await task2,
            await task3,
            await task4,
            await task5,
            await task6,
            await task7,
            await task8,
            await task9,
            await task10,
            await task11,
            await task12,
            await task13,
            await task14,
            await task15,
            await task16
        );
    }
}
