using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyRelationalDatabaseTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<IRelationalDatabaseTableComments>();

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return OptionAsync<IRelationalDatabaseTableComments>.None;
        }
    }
}
