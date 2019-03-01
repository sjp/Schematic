using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyRelationalDatabaseTableCommentProvider : IRelationalDatabaseTableCommentProvider
    {
        public Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> GetAllTableComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Empty.TableComments;
        }

        public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return OptionAsync<IRelationalDatabaseTableComments>.None;
        }
    }
}
