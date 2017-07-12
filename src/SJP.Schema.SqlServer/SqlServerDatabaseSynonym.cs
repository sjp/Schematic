using System;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerDatabaseSynonym : IDatabaseSynonym
    {
        public SqlServerDatabaseSynonym(IRelationalDatabase database, Identifier name, Identifier targetName)
        {
            if (name == null || name.LocalName == null)
                throw new ArgumentNullException(nameof(name));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = name.LocalName;
            Target = targetName; // don't check for validity of target, could be a broken synonym
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        public Identifier Target { get; }
    }
}
