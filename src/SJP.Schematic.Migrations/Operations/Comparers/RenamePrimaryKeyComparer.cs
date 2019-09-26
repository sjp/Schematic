﻿using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class RenamePrimaryKeyComparer : EqualityComparer<RenamePrimaryKeyOperation>
    {
        public override bool Equals(RenamePrimaryKeyOperation x, RenamePrimaryKeyOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null || y is null)
                return false;

            var xColumnNames = x.PrimaryKey.Columns.Select(c => c.Name).ToList();
            var yColumnNames = y.PrimaryKey.Columns.Select(c => c.Name).ToList();

            return x.TargetName == y.TargetName
                && NameComparer.Equals(x.PrimaryKey.Name, y.PrimaryKey.Name)
                && xColumnNames.SequenceEqual(yColumnNames);
        }

        public override int GetHashCode(RenamePrimaryKeyOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCode();
            builder.Add(obj.TargetName);
            builder.Add(obj.PrimaryKey.Name);

            var columnNames = obj.PrimaryKey.Columns.Select(c => c.Name).ToList();
            foreach (var columnName in columnNames)
                builder.Add(columnName);

            return builder.ToHashCode();
        }

        private static readonly IEqualityComparer<Option<Identifier>> NameComparer = new OptionalNameComparer();
    }
}
