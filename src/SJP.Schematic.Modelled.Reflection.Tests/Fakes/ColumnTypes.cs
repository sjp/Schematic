using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes.ColumnTypes
{
    [ColumnType.BigInteger]
    internal struct BigInteger : IDbType<decimal>
    {
    }

    [ColumnType.Integer]
    internal struct Integer : IDbType<int>
    {
    }

    [ColumnType.String(200)]
    internal struct Varchar200 : IDbType<string>
    {
    }

    [ColumnType.Unicode(400)]
    internal struct NVarchar400 : IDbType<string>
    {
    }

    [ColumnType.LargeBinary]
    internal struct Blob : IDbType<byte[]>
    {
    }

    [ColumnType.Custom(DataType = DataType.Unicode, Length = 255, TypeName = "nvarchar(255)")]
    internal struct NVarchar255 : IDbType<string>
    {
    }
}
