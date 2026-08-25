using System;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Helper extensions for working with collections of value tasks.
/// </summary>
public static class ValueTaskArrayExtensions
{
    /// <summary>
    /// Evaluates and unwraps all of the value tasks so they can be read immediately.
    /// </summary>
    /// <typeparam name="T">The type of the result of all of the value task operations.</typeparam>
    /// <param name="tasks">A collection of value tasks.</param>
    /// <returns>A set of results that are returned from all of the value tasks. The ordering of the results matches the ordering of the tasks themselves.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    public static ValueTask<T[]> WhenAll<T>(this ValueTask<T>[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (Array.TrueForAll(tasks, static t => t.IsCompletedSuccessfully))
        {
            var results = new T[tasks.Length];
            for (var i = 0; i < tasks.Length; i++)
                results[i] = tasks[i].Result;

            return new ValueTask<T[]>(results);
        }

        return new ValueTask<T[]>(WhenAllCore(tasks));
    }

    private static async Task<T[]> WhenAllCore<T>(ValueTask<T>[] valueTasks)
    {
        var tasks = Array.ConvertAll(valueTasks, static t => t.AsTask());

        return await Task.WhenAll(tasks);
    }
}
