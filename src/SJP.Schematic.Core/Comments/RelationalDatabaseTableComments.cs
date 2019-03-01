using System;
using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public class RelationalDatabaseTableComments : IRelationalDatabaseTableComments
    {
        public RelationalDatabaseTableComments(
            Identifier tableName,
            Option<string> comment,
            Option<string> primaryKeyComment,
            IReadOnlyDictionary<Identifier, Option<string>> columnComments,
            IReadOnlyDictionary<Identifier, Option<string>> checkComments,
            IReadOnlyDictionary<Identifier, Option<string>> uniqueKeyComments,
            IReadOnlyDictionary<Identifier, Option<string>> foreignKeyComments,
            IReadOnlyDictionary<Identifier, Option<string>> indexComments,
            IReadOnlyDictionary<Identifier, Option<string>> triggerComments
        )
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            Comment = comment;
            PrimaryKeyComment = primaryKeyComment;
            ColumnComments = columnComments ?? throw new ArgumentNullException(nameof(columnComments));
            CheckComments = checkComments ?? throw new ArgumentNullException(nameof(checkComments));
            UniqueKeyComments = uniqueKeyComments ?? throw new ArgumentNullException(nameof(uniqueKeyComments));
            ForeignKeyComments = foreignKeyComments ?? throw new ArgumentNullException(nameof(foreignKeyComments));
            IndexComments = indexComments ?? throw new ArgumentNullException(nameof(indexComments));
            TriggerComments = triggerComments ?? throw new ArgumentNullException(nameof(triggerComments));
        }

        public Identifier TableName { get; }

        public Option<string> Comment { get; }

        public Option<string> PrimaryKeyComment { get; }

        public IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }

        public IReadOnlyDictionary<Identifier, Option<string>> CheckComments { get; }

        public IReadOnlyDictionary<Identifier, Option<string>> UniqueKeyComments { get; }

        public IReadOnlyDictionary<Identifier, Option<string>> ForeignKeyComments { get; }

        public IReadOnlyDictionary<Identifier, Option<string>> IndexComments { get; }

        public IReadOnlyDictionary<Identifier, Option<string>> TriggerComments { get; }
    }
}
