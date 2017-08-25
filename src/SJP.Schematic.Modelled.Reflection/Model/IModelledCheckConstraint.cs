using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public interface IModelledCheckConstraint
    {
        PropertyInfo Property { get; set; }

        IModelledSqlExpression Expression { get; }
    }
}
