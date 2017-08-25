using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public interface IModelledComputedColumn : IModelledColumn
    {
        IModelledSqlExpression Expression { get; }
    }
}
