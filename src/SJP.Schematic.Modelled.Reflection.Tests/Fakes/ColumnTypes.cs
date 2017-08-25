using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes.ColumnTypes
{
    [ColumnType.BigInteger]
    public struct BigInteger : IDbType<decimal> { }

    [ColumnType.Integer]
    public struct Integer : IDbType<int> { }

    [ColumnType.String(200)]
    public struct Varchar200 : IDbType<string> { }

    [ColumnType.Unicode(400)]
    public struct NVarchar400 : IDbType<string> { }

    [ColumnType.LargeBinary]
    public struct Blob : IDbType<byte[]> { }
}
