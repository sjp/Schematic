using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public interface IModelledCheckConstraint
    {
        PropertyInfo Property { get; set; }

        ISqlExpression Expression { get; }
    }
}
