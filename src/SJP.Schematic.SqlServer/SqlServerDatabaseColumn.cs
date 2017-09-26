using System;
using System.Linq;
using System.Text;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public abstract class SqlServerDatabaseColumn : IDatabaseColumn
    {
        protected SqlServerDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
        {
            if (columnName == null || columnName.LocalName == null)
                throw new ArgumentNullException(nameof(columnName));

            Name = columnName.LocalName;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsNullable = isNullable;
            DefaultValue = StripParentheses(defaultValue);
            IsAutoIncrement = isAutoIncrement;
        }

        public string DefaultValue { get; }

        public virtual bool IsComputed { get; }

        public Identifier Name { get; }

        public IDbType Type { get; }

        public bool IsNullable { get; }

        public bool IsAutoIncrement { get; }

        protected static string StripParentheses(string defaultValue)
        {
            if (defaultValue == null)
                return null;

            var openParenChars = defaultValue.TakeWhile(c => c == '(' || char.IsWhiteSpace(c)).ToList();
            var openParenCount = openParenChars.Count(c => c == '(');
            if (openParenCount == 0)
                return defaultValue;

            var prefixIndex = openParenChars.Count;

            var revDefault = defaultValue.Reverse().ToList();

            var parenCount = 0;
            var closedParenBuffer = new StringBuilder();
            for (var i = 0; i < revDefault.Count; i++)
            {
                var c = revDefault[i];
                if (c != ')' && parenCount == openParenCount)
                    break;

                if (c == ')')
                    parenCount++;
                if (c == ')' || char.IsWhiteSpace(c))
                    closedParenBuffer.Append(c);
            }

            var suffix = closedParenBuffer.ToString();
            var resultLength = defaultValue.Length - suffix.Length - prefixIndex;
            return defaultValue.Substring(prefixIndex, resultLength);
        }
    }

    public class SqlServerDatabaseTableColumn : SqlServerDatabaseColumn, IDatabaseTableColumn
    {
        public SqlServerDatabaseTableColumn(IRelationalDatabaseTable table, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public IRelationalDatabaseTable Table { get; }
    }

    public class SqlServerDatabaseViewColumn : SqlServerDatabaseColumn, IDatabaseViewColumn
    {
        public SqlServerDatabaseViewColumn(IRelationalDatabaseView view, Identifier columnName, IDbType type, bool isNullable, string defaultValue, bool isAutoIncrement)
            : base(columnName, type, isNullable, defaultValue, isAutoIncrement)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IRelationalDatabaseView View { get; }
    }
}
