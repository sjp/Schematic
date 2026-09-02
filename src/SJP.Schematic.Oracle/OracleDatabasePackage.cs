using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle;

/// <summary>
/// An Oracle package definition.
/// </summary>
/// <seealso cref="IOracleDatabasePackage" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class OracleDatabasePackage : IOracleDatabasePackage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabasePackage"/> class.
    /// </summary>
    /// <param name="name">The package name.</param>
    /// <param name="specification">The package specification.</param>
    /// <param name="body">The package body, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="specification"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="specification"/> is empty or whitespace.</exception>
    public OracleDatabasePackage(Identifier name, string specification, Option<string> body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(specification);

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Specification = specification;
        Body = body;
    }

    /// <summary>
    /// The name of the database package.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// Gets the specification of the package. Roughly equivalent to an interface or headers.
    /// </summary>
    /// <value>The specification.</value>
    public string Specification { get; }

    /// <summary>
    /// The body of the package, i.e. the implementation.
    /// </summary>
    /// <value>The package body.</value>
    public Option<string> Body { get; }

    /// <summary>
    /// The definition of the package.
    /// </summary>
    /// <value>A textual package definition.</value>
    public string Definition
    {
        get
        {
            var bodyText = Body.Match(static b => Environment.NewLine + b, static () => string.Empty);
            return Specification + bodyText;
        }
    }

    /// <summary>
    /// The kind of routine a package represents.
    /// </summary>
    /// <value>Always <see cref="Core.RoutineType.Package"/>.</value>
    public RoutineType RoutineType { get; } = RoutineType.Package;

    /// <summary>
    /// The language the package is written in.
    /// </summary>
    /// <value>Always <c>PL/SQL</c>. Oracle exposes package source through <c>SYS.ALL_SOURCE</c>, which only holds PL/SQL.</value>
    public Option<string> Language { get; } = Option<string>.Some("PL/SQL");

    /// <summary>
    /// The parameters the package declares.
    /// </summary>
    /// <value>
    /// Always empty. A package is a container for subprograms, each with its own signature, and
    /// Oracle does not attribute parameters to the package itself.
    /// </value>
    public IReadOnlyList<IDatabaseRoutineParameter> Parameters { get; } = [];

    /// <summary>
    /// The type of value the package returns.
    /// </summary>
    /// <value>Always none. A package is not invoked and so returns nothing.</value>
    public Option<IDbType> ReturnType { get; } = Option<IDbType>.None;

    /// <summary>
    /// The signatures declared under the package's name.
    /// </summary>
    /// <value>Always empty. Oracle does not permit two packages in a schema to share a name.</value>
    public IReadOnlyList<IDatabaseRoutineOverload> Overloads { get; } = [];

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Package: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}