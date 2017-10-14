using System;

namespace SJP.Schematic.Core
{
    public class DatabaseSynonym : IDatabaseSynonym
    {
        public DatabaseSynonym(IRelationalDatabase database, Identifier synonymName, Identifier targetName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));
            if (targetName == null || targetName.LocalName == null)
                throw new ArgumentNullException(nameof(targetName));

            Database = database ?? throw new ArgumentNullException(nameof(database));

            var serverName = synonymName.Server ?? database.ServerName;
            var databaseName = synonymName.Database ?? database.DatabaseName;
            var schemaName = synonymName.Schema ?? database.DefaultSchema;

            Name = new Identifier(serverName, databaseName, schemaName, synonymName.LocalName);

            var targetServerName = targetName.Server ?? database.ServerName;
            var targetDatabaseName = targetName.Database ?? database.DatabaseName;
            var targetSchemaName = targetName.Schema ?? database.DefaultSchema;

            Target = new Identifier(targetServerName, targetDatabaseName, targetSchemaName, targetName.LocalName); // don't check for validity of target, could be a broken synonym
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        public Identifier Target { get; }
    }
}
