namespace SJP.Schematic.Sqlite.Pragma;

/// <summary>
/// Represents the text encoding used by the main database.
/// </summary>
public enum Encoding
{
    /// <summary>
    /// UTF-8 Encoding.
    /// </summary>
    Utf8,

    /// <summary>
    /// UTF-16 using the native machine byte ordering.
    /// </summary>
    Utf16,

    /// <summary>
    /// Little endian UTF-16
    /// </summary>
    Utf16le,

    /// <summary>
    /// Big endian UTF-16
    /// </summary>
    Utf16be,
}