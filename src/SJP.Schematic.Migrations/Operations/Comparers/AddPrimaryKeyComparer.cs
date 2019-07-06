using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class AddPrimaryKeyComparer : EqualityComparer<AddPrimaryKeyOperation>
    {
        public override bool Equals(AddPrimaryKeyOperation x, AddPrimaryKeyOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            var xColumnNames = x.PrimaryKey.Columns.Select(c => c.Name).ToList();
            var yColumnNames = y.PrimaryKey.Columns.Select(c => c.Name).ToList();

            return x.Table.Name == y.Table.Name
                && NameComparer.Equals(x.PrimaryKey.Name, y.PrimaryKey.Name)
                && xColumnNames.SequenceEqual(yColumnNames);
        }

        public override int GetHashCode(AddPrimaryKeyOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCodeBuilder();
            builder.Add(obj.Table.Name);
            builder.Add(obj.PrimaryKey.Name);

            var columnNames = obj.PrimaryKey.Columns.Select(c => c.Name).ToList();
            foreach (var columnName in columnNames)
                builder.Add(columnName);

            return builder.ToHashCode();
        }

        private static readonly IEqualityComparer<Option<Identifier>> NameComparer = new OptionalNameComparer();
    }
}
