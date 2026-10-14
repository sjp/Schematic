using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Column type metadata that also records whether a character length was declared in characters.
/// </summary>
internal sealed class OracleColumnTypeMetadata : ColumnTypeMetadata
{
    /// <summary>
    /// Indicates whether the length was declared in characters, as by <c>VARCHAR2(10 CHAR)</c>,
    /// rather than in bytes. The length of an <c>NVARCHAR2</c> or <c>NCHAR</c> always counts characters.
    /// </summary>
    /// <value><see langword="true" /> if the length counts characters; otherwise, <see langword="false" />.</value>
    public bool HasCharacterLengthSemantics { get; init; }
}
