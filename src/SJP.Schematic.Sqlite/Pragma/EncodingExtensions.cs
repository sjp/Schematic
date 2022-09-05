using System;
using EnumsNET;
using SysTextEncoding = System.Text.Encoding;

namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Convenience methods for working with SQLite encoding definitions.
/// </summary>
public static class EncodingExtensions
{
    /// <summary>
    /// Translates a SQLite encoding to a .NET encoding.
    /// </summary>
    /// <param name="encoding">The SQLite encoding type.</param>
    /// <returns>A .NET encoding definition.</returns>
    /// <exception cref="ArgumentException"><paramref name="encoding"/> is an invalid enum value.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="encoding"/> is an unknown and unsupported enum value.</exception>
    public static SysTextEncoding AsTextEncoding(this Encoding encoding)
    {
        if (!encoding.IsValid())
            throw new ArgumentException($"The {nameof(Encoding)} provided must be a valid enum.", nameof(encoding));

        return encoding switch
        {
            Encoding.Utf8 => SysTextEncoding.UTF8,
            Encoding.Utf16 or Encoding.Utf16le => SysTextEncoding.Unicode,
            Encoding.Utf16be => SysTextEncoding.BigEndianUnicode,
            _ => throw new InvalidOperationException("Unknown and unsupported encoding found: " + encoding.ToString()),
        };
    }
}