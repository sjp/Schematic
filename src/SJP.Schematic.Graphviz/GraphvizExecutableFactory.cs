using System;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Graphviz;

/// <summary>
/// A factory for obtaining a graphviz executable to render DOT images.
/// </summary>
public sealed class GraphvizExecutableFactory
{
    private readonly string? _configuredPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizExecutableFactory"/> class.
    /// </summary>
    public GraphvizExecutableFactory()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphvizExecutableFactory"/> class.
    /// </summary>
    /// <param name="configuration">A configuration provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>.</exception>
    public GraphvizExecutableFactory(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuredPath = configuration["Graphviz:Dot"];
    }

    /// <summary>
    /// Retrieves a graphviz executable.
    /// </summary>
    /// <returns>A graphviz executable.</returns>
    public IGraphvizExecutable GetExecutable()
    {
        var envPath = Environment.GetEnvironmentVariable("SCHEMATIC_GRAPHVIZ_DOT");
        if (!envPath.IsNullOrWhiteSpace())
            return new GraphvizSystemExecutable(envPath);

        if (!_configuredPath.IsNullOrEmpty())
            return new GraphvizSystemExecutable(_configuredPath);

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            return new GraphvizTemporaryExecutable();

        // just try running 'dot', assume there's a system executable
        return new GraphvizSystemExecutable("dot");
    }
}