using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;

namespace SJP.Schematic.Sqlite.Migrations
{
    public class SqliteMigrationBuilder : IMigrationBuilder
    {
        public SqliteMigrationBuilder(IDbConnection connection, IDatabaseDialect dialect, IRelationalDatabase database)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IRelationalDatabase Database { get; }

        protected IList<IMigrationOperation> Operations => _operations;

        public IReadOnlyCollection<IMigrationOperation> GetMigrations()
        {
            return _operations;
        }

        public Task AddCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            throw new NotImplementedException();
        }

        public Task AddColumn(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            throw new NotImplementedException();
        }

        public Task AddForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            throw new NotImplementedException();
        }

        public Task AddPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            throw new NotImplementedException();
        }

        public Task AddUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            throw new NotImplementedException();
        }

        public Task AlterColumn(IRelationalDatabaseTable table, IDatabaseColumn existingColumn, IDatabaseColumn targetColumn)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (existingColumn == null)
                throw new ArgumentNullException(nameof(existingColumn));
            if (targetColumn == null)
                throw new ArgumentNullException(nameof(targetColumn));

            throw new NotImplementedException();
        }

        public Task AlterSequence(IDatabaseSequence existingSequence, IDatabaseSequence targetSequence)
        {
            if (existingSequence == null)
                throw new ArgumentNullException(nameof(existingSequence));
            if (targetSequence == null)
                throw new ArgumentNullException(nameof(targetSequence));

            throw new NotImplementedException();
        }

        public Task AlterTable(IRelationalDatabaseTable existingTable, IRelationalDatabaseTable targetTable)
        {
            if (existingTable == null)
                throw new ArgumentNullException(nameof(existingTable));
            if (targetTable == null)
                throw new ArgumentNullException(nameof(targetTable));

            throw new NotImplementedException();
        }

        public Task CreateIndex(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            throw new NotImplementedException();
        }

        public Task CreateRoutine(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            throw new NotImplementedException();
        }

        public Task CreateSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            throw new NotImplementedException();
        }

        public Task CreateSynonym(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            throw new NotImplementedException();
        }

        public Task CreateTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            throw new NotImplementedException();
        }

        public Task CreateView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            throw new NotImplementedException();
        }

        public Task DropCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            throw new NotImplementedException();
        }

        public Task DropColumn(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            throw new NotImplementedException();
        }

        public Task DropForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            throw new NotImplementedException();
        }

        public Task DropIndex(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            throw new NotImplementedException();
        }

        public Task DropPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            throw new NotImplementedException();
        }

        public Task DropRoutine(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            throw new NotImplementedException();
        }

        public Task DropSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            throw new NotImplementedException();
        }

        public Task DropSynonym(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            throw new NotImplementedException();
        }

        public Task DropTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            throw new NotImplementedException();
        }

        public Task DropUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            throw new NotImplementedException();
        }

        public Task DropView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            throw new NotImplementedException();
        }

        public Task RenameCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameColumn(IRelationalDatabaseTable table, IDatabaseColumn column, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameForeignKey(IRelationalDatabaseTable childTable, IRelationalDatabaseTable parentTable, IDatabaseRelationalKey foreignKey, Identifier targetName)
        {
            if (childTable == null)
                throw new ArgumentNullException(nameof(childTable));
            if (parentTable == null)
                throw new ArgumentNullException(nameof(parentTable));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameIndex(IRelationalDatabaseTable table, IDatabaseIndex index, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenamePrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameRoutine(IDatabaseRoutine routine, Identifier targetName)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameSequence(IDatabaseSequence sequence, Identifier targetName)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameSynonym(IDatabaseSynonym synonym, Identifier targetName)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameTable(IRelationalDatabaseTable table, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task RenameView(IDatabaseView view, Identifier targetName)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public Task Sql(ISqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            throw new NotImplementedException();
        }

        private const string SequencesNotSupported = "SQLite does not support sequences.";
        private const string SynonymsNotSupported = "SQLite does not support synonyms.";
        private const string RoutinesNotSupported = "SQLite does not support routines.";

        private readonly List<IMigrationOperation> _operations = new List<IMigrationOperation>();
    }
}
