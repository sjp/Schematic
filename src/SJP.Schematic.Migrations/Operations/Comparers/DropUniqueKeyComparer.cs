using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class DropUniqueKeyComparer : EqualityComparer<DropUniqueKeyOperation>
    {
        public override bool Equals(DropUniqueKeyOperation x, DropUniqueKeyOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            var xColumnNames = x.UniqueKey.Columns.Select(c => c.Name).ToList();
            var yColumnNames = y.UniqueKey.Columns.Select(c => c.Name).ToList();

            return x.Table.Name == y.Table.Name
                && NameComparer.Equals(x.UniqueKey.Name, y.UniqueKey.Name)
                && xColumnNames.SequenceEqual(yColumnNames);
        }

        public override int GetHashCode(DropUniqueKeyOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCodeBuilder();
            builder.Add(obj.Table.Name);
            builder.Add(obj.UniqueKey.Name);

            var columnNames = obj.UniqueKey.Columns.Select(c => c.Name).ToList();
            foreach (var columnName in columnNames)
                builder.Add(columnName);

            return builder.ToHashCode();
        }

        private static readonly IEqualityComparer<Option<Identifier>> NameComparer = new OptionalNameComparer();
    }
}
