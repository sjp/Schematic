using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledIndexColumn
    {
        IndexColumnOrder Order { get; }

        IModelledSqlExpression Expression { get; }
    }
}
