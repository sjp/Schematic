using System;
using System.Reflection;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class ModelledColumn : IModelledColumn
    {
        public ModelledColumn(Type dbType, bool isNullable)
        {
            DeclaredDbType = dbType ?? throw new ArgumentNullException(nameof(dbType));
            IsNullable = isNullable;
        }

        public virtual Type DeclaredDbType { get; }

        public virtual bool IsComputed { get; }

        public virtual bool IsNullable { get; }

        public PropertyInfo Property { get; set; }
    }
}
