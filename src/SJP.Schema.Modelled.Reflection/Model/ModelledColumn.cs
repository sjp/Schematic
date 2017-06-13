using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public class ModelledColumn : IModelledColumn
    {
        public ModelledColumn(IDbType dbType, bool isNullable)
        {
            DbType = dbType ?? throw new ArgumentNullException(nameof(dbType));
            IsNullable = isNullable;
        }

        public PropertyInfo Property { get; internal set; }

        public virtual IDbType DbType { get; }

        public virtual bool IsNullable { get; }
    }
}
