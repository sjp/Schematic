using System;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Implements a database synonym, i.e. an alias from one database object to another.
/// </summary>
/// <seealso cref="IDatabaseSynonym" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseSynonym : IDatabaseSynonym
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSynonym"/> class.
    /// </summary>
    /// <param name="synonymName">The synonym name.</param>
    /// <param name="targetName">The target of the synonym alias.</param>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> or <paramref name="targetName"/> is <c>null</c>.</exception>
    public DatabaseSynonym(Identifier synonymName, Identifier targetName)
    {
        Name = synonymName ?? throw new ArgumentNullException(nameof(synonymName));
        Target = targetName ?? throw new ArgumentNullException(nameof(targetName)); // don't check for validity of target, could be a broken synonym
    }

    /// <summary>
    /// The name of the database synonym.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// The target of the synonym, i.e. the name of the object being aliased.
    /// </summary>
    /// <value>The synonym target name.</value>
    /// <remarks>The target does not have to be an object present in the database.</remarks>
    public Identifier Target { get; }

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

            builder.Append("Synonym: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            builder.Append(" -> ");

            if (!Target.Schema.IsNullOrWhiteSpace())
                builder.Append(Target.Schema).Append('.');

            builder.Append(Target.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
