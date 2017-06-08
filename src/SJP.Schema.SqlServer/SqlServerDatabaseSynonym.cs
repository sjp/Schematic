using System;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerDatabaseSynonym : IDatabaseSynonym
    {
        public SqlServerDatabaseSynonym(IRelationalDatabase database, Identifier name, Identifier targetName)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Target = targetName; // don't check for validity of target, could be a broken synonym
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        public Identifier Target { get; }
    }
}
