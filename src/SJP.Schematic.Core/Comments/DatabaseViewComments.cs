using System;
using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public class DatabaseViewComments : IDatabaseViewComments
    {
        public DatabaseViewComments(
            Identifier viewName,
            Option<string> comment,
            IReadOnlyDictionary<Identifier, Option<string>> columnComments
        )
        {
            ViewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
            Comment = comment;
            ColumnComments = columnComments ?? throw new ArgumentNullException(nameof(columnComments));
        }

        public Identifier ViewName { get; }

        public Option<string> Comment { get; }

        public IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }
    }
}
