using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseSynonym : IDatabaseSynonym
    {
        public SqlServerDatabaseSynonym(IRelationalDatabase database, Identifier synonymName, Identifier targetName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            Database = database ?? throw new ArgumentNullException(nameof(database));

            var serverName = synonymName.Server ?? database.ServerName;
            var databaseName = synonymName.Database ?? database.DatabaseName;
            var schemaName = synonymName.Schema ?? database.DefaultSchema;

            Name = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, synonymName.LocalName);

            var targetServerName = targetName.Server ?? database.ServerName;
            var targetDatabaseName = targetName.Database ?? database.DatabaseName;
            var targetSchemaName = targetName.Schema ?? database.DefaultSchema;

            Target = Identifier.CreateQualifiedIdentifier(targetServerName, targetDatabaseName, targetSchemaName, targetName.LocalName); // don't check for validity of target, could be a broken synonym
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        public Identifier Target { get; }
    }
}
