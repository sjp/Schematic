using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Comments
{
    public sealed class EmptyDatabaseSynonymCommentProvider : IDatabaseSynonymCommentProvider
    {
        public Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Empty.SynonymComments;
        }

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return OptionAsync<IDatabaseSynonymComments>.None;
        }
    }
}
