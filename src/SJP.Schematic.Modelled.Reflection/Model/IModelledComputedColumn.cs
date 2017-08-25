using System.Reflection;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledComputedColumn : IModelledColumn
    {
        IModelledSqlExpression Expression { get; }
    }
}
