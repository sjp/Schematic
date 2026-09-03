using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// Column data types for tests that need a type but do not exercise one, so that a test says what
/// it is about rather than repeating a type definition.
/// </summary>
public static class TestDbTypes
{
    /// <summary>
    /// A 64-bit integer type, the widest type a sequence commonly generates.
    /// </summary>
    public static IDbType BigInteger { get; } = new ColumnDataType(
        "bigint",
        DataType.BigInteger,
        "bigint",
        typeof(long),
        false,
        -1,
        Option<INumericPrecision>.None,
        Option<Identifier>.None
    );
}
