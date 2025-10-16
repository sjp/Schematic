using System;
using System.Linq;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions;

/// <summary>
/// Helper extensions for working with collection of tasks.
/// </summary>
public static class TaskArrayExtensions
{
    /// <summary>
    /// Evaluates and unwraps all of the tasks so they can be read immediately.
    /// </summary>
    /// <typeparam name="T">The type of the result of all of the task operations.</typeparam>
    /// <param name="tasks">A collection of tasks.</param>
    /// <returns>A set of results that are returned from all of the tasks. The ordering of the results matches the ordering of the tasks themselves.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tasks"/> is <see langword="null"/>.</exception>
    public static async Task<T[]> WhenAll<T>(this Task<T>[] tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        await Task.WhenAll(tasks);

        return tasks
            .Select(t => t.Result)
            .ToArray();
    }
}