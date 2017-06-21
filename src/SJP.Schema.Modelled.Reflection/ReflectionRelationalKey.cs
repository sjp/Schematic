using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionRelationalKey : IDatabaseRelationalKey
    {
        public ReflectionRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey, RelationalKeyUpdateAction deleteAction, RelationalKeyUpdateAction updateAction)
        {
            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
            DeleteAction = deleteAction;
            UpdateAction = updateAction;

            ValidateColumnSetsCompatible(childKey, parentKey);
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }

        public RelationalKeyUpdateAction DeleteAction { get; }

        public RelationalKeyUpdateAction UpdateAction { get; }

        // TODO: implement
        private static void ValidateColumnSetsCompatible(IDatabaseKey childKey, IDatabaseKey parentKey)
        {

        }

        private static bool ColumnTypesCompatible(IDatabaseColumn a, IDatabaseColumn b)
        {
            return true;
        }
    }
}
