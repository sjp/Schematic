using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// Defines a source code project generator that enables interoperability with a database.
/// </summary>
public interface IDataAccessGenerator
{
    /// <summary>
    /// Generates a source code project.
    /// </summary>
    /// <param name="projectPath">A path that determines where the generated C# project file should be stored.</param>
    /// <param name="baseNamespace">The base C# namespace to use for generated files.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task indicating the completion of the source code generation.</returns>
    Task GenerateAsync(string projectPath, string baseNamespace, CancellationToken cancellationToken = default);
}