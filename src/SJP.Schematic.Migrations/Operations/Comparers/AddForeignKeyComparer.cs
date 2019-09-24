using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class AddForeignKeyComparer : EqualityComparer<AddForeignKeyOperation>
    {
        public override bool Equals(AddForeignKeyOperation x, AddForeignKeyOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            var xChildColumnNames = x.ForeignKey.ChildKey.Columns.Select(c => c.Name).ToList();
            var yChildColumnNames = y.ForeignKey.ChildKey.Columns.Select(c => c.Name).ToList();
            var xParentColumnNames = x.ForeignKey.ParentKey.Columns.Select(c => c.Name).ToList();
            var yParentColumnNames = y.ForeignKey.ParentKey.Columns.Select(c => c.Name).ToList();

            return x.Table.Name == y.Table.Name
                && NameComparer.Equals(x.ForeignKey.ChildKey.Name, y.ForeignKey.ChildKey.Name)
                && NameComparer.Equals(x.ForeignKey.ParentKey.Name, y.ForeignKey.ParentKey.Name)
                && x.ForeignKey.ChildTable == y.ForeignKey.ChildTable
                && x.ForeignKey.ParentTable == y.ForeignKey.ParentTable
                && xChildColumnNames.SequenceEqual(yChildColumnNames)
                && xParentColumnNames.SequenceEqual(yParentColumnNames);
        }

        public override int GetHashCode(AddForeignKeyOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCodeBuilder();
            builder.Add(obj.Table.Name);
            builder.Add(obj.ForeignKey.ChildKey.Name);
            builder.Add(obj.ForeignKey.ParentKey.Name);
            builder.Add(obj.ForeignKey.ChildTable);
            builder.Add(obj.ForeignKey.ParentTable);

            var childColumnNames = obj.ForeignKey.ChildKey.Columns.Select(c => c.Name).ToList();
            foreach (var childColumnName in childColumnNames)
                builder.Add(childColumnName);

            var parentColumnNames = obj.ForeignKey.ParentKey.Columns.Select(c => c.Name).ToList();
            foreach (var parentColumnName in parentColumnNames)
                builder.Add(parentColumnName);

            return builder.ToHashCode();
        }

        private static readonly IEqualityComparer<Option<Identifier>> NameComparer = new OptionalNameComparer();
    }
}
