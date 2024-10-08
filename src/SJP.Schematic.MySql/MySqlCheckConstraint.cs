﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql;

/// <summary>
/// A check constraint definition.
/// </summary>
/// <seealso cref="IDatabaseCheckConstraint" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class MySqlCheckConstraint : IDatabaseCheckConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlCheckConstraint"/> class.
    /// </summary>
    /// <param name="checkName">The name of the check constraint.</param>
    /// <param name="definition">The constraint definition.</param>
    /// <param name="isEnabled">Determines whether the check constraint is enabled.</param>
    /// <exception cref="ArgumentNullException"><paramref name="checkName"/> is <c>null</c>. Alternatively if <paramref name="definition"/> is <c>null</c>, empty or whitespace.</exception>
    public MySqlCheckConstraint(Identifier checkName, string definition, bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(checkName);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        Name = Option<Identifier>.Some(checkName.LocalName);
        Definition = definition;
        IsEnabled = isEnabled;
    }

    /// <summary>
    /// The check constraint name.
    /// </summary>
    /// <value>A constraint name, if available.</value>
    public Option<Identifier> Name { get; }

    /// <summary>
    /// The definition of the check constraint.
    /// </summary>
    /// <value>The check constraint definition.</value>
    public string Definition { get; }

    /// <summary>
    /// Indicates whether this constraint is enabled.
    /// </summary>
    /// <value><c>true</c> if this check constraint is enabled; otherwise, <c>false</c>.</value>
    public bool IsEnabled { get; }

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

            builder.Append("Check");

            Name.IfSome(name =>
            {
                builder.Append(": ")
                    .Append(name.LocalName);
            });

            return builder.GetStringAndRelease();
        }
    }
}