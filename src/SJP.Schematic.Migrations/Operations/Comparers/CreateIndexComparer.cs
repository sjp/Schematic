using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public class CreateIndexComparer : EqualityComparer<CreateIndexOperation>
    {
        public override bool Equals(CreateIndexOperation x, CreateIndexOperation y)
        {
            if (x is null && y is null)
                return true;

            if (x is null ^ y is null)
                return false;

            var xColumnExpressions = x.Index.Columns.Select(c => c.Expression).ToList();
            var yColumnExpressions = y.Index.Columns.Select(c => c.Expression).ToList();

            var xIncludedColumnNames = x.Index.IncludedColumns.Select(c => c.Name).ToList();
            var yIncludedColumnNames = y.Index.IncludedColumns.Select(c => c.Name).ToList();

            return x.Table.Name == y.Table.Name
                && x.Index.Name == y.Index.Name
                && xColumnExpressions.SequenceEqual(yColumnExpressions)
                && xIncludedColumnNames.SequenceEqual(yIncludedColumnNames);
        }

        public override int GetHashCode(CreateIndexOperation obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var builder = new HashCodeBuilder();
            builder.Add(obj.Table.Name);
            builder.Add(obj.Index.Name);

            var columnExpressions = obj.Index.Columns.Select(c => c.Expression).ToList();
            foreach (var columnExpression in columnExpressions)
                builder.Add(columnExpression);

            var includedColumnNames = obj.Index.IncludedColumns.Select(c => c.Name).ToList();
            foreach (var columnName in includedColumnNames)
                builder.Add(columnName);

            return builder.ToHashCode();
        }
    }
}
