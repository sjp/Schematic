namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// The kind of a serialized column default, recording which case of <see cref="Core.DatabaseDefaultValue"/> it was written from.
/// </summary>
public enum DefaultValueKind
{
    /// <summary>
    /// The expression has not been classified, either because the database does not report enough
    /// to do so, or because the dialect provider does not recognise its shape.
    /// </summary>
    Unknown,

    /// <summary>
    /// The expression is a constant, e.g. <c>0</c>, <c>'unassigned'</c> or <c>true</c>.
    /// </summary>
    Literal,

    /// <summary>
    /// The expression is the <c>NULL</c> literal. Kept apart from <see cref="Literal"/> because a
    /// default of <c>NULL</c> is the absence of a value rather than a value.
    /// </summary>
    Null,

    /// <summary>
    /// The expression is evaluated by the database for each row that omits the column, e.g.
    /// <c>getdate()</c> or <c>lower(current_user)</c>.
    /// </summary>
    Expression,

    /// <summary>
    /// The expression draws the next value of a sequence, e.g. SQL Server
    /// <c>NEXT VALUE FOR</c>, PostgreSQL <c>nextval(...)</c> or Oracle <c>seq.NEXTVAL</c>.
    /// </summary>
    SequenceNextValue,
}
