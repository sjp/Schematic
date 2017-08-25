using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public interface IModelledIndexColumn
    {
        IndexColumnOrder Order { get; }

        IModelledSqlExpression Expression { get; }
    }
}
