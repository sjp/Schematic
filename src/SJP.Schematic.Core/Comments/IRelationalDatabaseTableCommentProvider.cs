﻿using System.Collections.Generic;
using System.Threading;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    public interface IRelationalDatabaseTableCommentProvider
    {
        OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default);
    }
}
