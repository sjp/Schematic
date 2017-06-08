using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public interface IModelledColumn
    {
        PropertyInfo Property { get; }

        bool IsNullable { get; }

        IDbType DbType { get; }
    }
}
