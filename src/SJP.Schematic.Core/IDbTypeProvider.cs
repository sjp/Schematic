namespace SJP.Schematic.Core
{
    public interface IDbTypeProvider
    {
        IDbType CreateColumnType(ColumnTypeMetadata typeMetadata);

        IDbType GetComparableColumnType(IDbType otherType);
    }
}
