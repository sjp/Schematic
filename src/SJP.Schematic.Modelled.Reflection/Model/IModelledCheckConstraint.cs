using System.Reflection;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public interface IModelledCheckConstraint
    {
        PropertyInfo Property { get; set; }

        IModelledSqlExpression Expression { get; }
    }
}
