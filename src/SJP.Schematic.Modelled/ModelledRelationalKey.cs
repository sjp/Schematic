using System;
using System.Data;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled
{
    public class ModelledRelationalKey : IDatabaseRelationalKey
    {
        public ModelledRelationalKey(IDatabaseKey childKey, IDatabaseKey parentKey, Rule deleteRule, Rule updateRule)
        {
            if (!deleteRule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(deleteRule));
            if (!updateRule.IsValid())
                throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(updateRule));

            ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
            ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
            DeleteRule = deleteRule;
            UpdateRule = updateRule;
        }

        public IDatabaseKey ChildKey { get; }

        public IDatabaseKey ParentKey { get; }

        public Rule DeleteRule { get; }

        public Rule UpdateRule { get; }
    }
}
