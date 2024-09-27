using System;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Contains information about database routines.
/// </summary>
/// <seealso cref="IDatabaseRoutine" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseRoutine : IDatabaseRoutine
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRoutine"/> class.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="definition">The routine definition.</param>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>. Alternatively, if <paramref name="definition"/> is <c>null</c>, empty or whitespace.</exception>
    public DatabaseRoutine(Identifier routineName, string definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        Name = routineName ?? throw new ArgumentNullException(nameof(routineName));
        Definition = definition;
    }

    /// <summary>
    /// The name of the database routine.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// The definition of the routine.
    /// </summary>
    /// <value>A textual routine definition.</value>
    public string Definition { get; }

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

            builder.Append("Routine: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}