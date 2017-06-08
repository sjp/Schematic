using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public interface IModelledIndexColumn
    {
        IndexColumnOrder Order { get; }

        ISqlExpression Expression { get; }
    }
}
