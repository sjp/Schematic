using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
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

        protected IList<IMigrationOperation> Operations { get; } = new List<IMigrationOperation>();

        protected IList<IMigrationError> Errors { get; } = new List<IMigrationError>();

        public Task<IReadOnlyCollection<IMigrationOperation>> BuildMigrations(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<IMigrationOperation>>(Array.Empty<IMigrationOperation>());
        }

        public void AddCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            var operation = new AddCheckOperation(table, check);
            Operations.Add(operation);
        }

        public void AddColumn(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var operation = new AddColumnOperation(table, column);
            Operations.Add(operation);
        }

        public void AddForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            var operation = new AddForeignKeyOperation(table, foreignKey);
            Operations.Add(operation);
        }

        public void AddPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            var operation = new AddPrimaryKeyOperation(table, primaryKey);
            Operations.Add(operation);
        }

        public void AddTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            var operation = new AddTriggerOperation(table, trigger);
            Operations.Add(operation);
        }

        public void AddUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            var operation = new AddUniqueKeyOperation(table, uniqueKey);
            Operations.Add(operation);
        }

        public void AlterColumn(IRelationalDatabaseTable table, IDatabaseColumn existingColumn, IDatabaseColumn targetColumn)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (existingColumn == null)
                throw new ArgumentNullException(nameof(existingColumn));
            if (targetColumn == null)
                throw new ArgumentNullException(nameof(targetColumn));

            var operation = new AlterColumnOperation(table, existingColumn, targetColumn);
            Operations.Add(operation);
        }

        public void AlterSequence(IDatabaseSequence existingSequence, IDatabaseSequence targetSequence)
        {
            if (existingSequence == null)
                throw new ArgumentNullException(nameof(existingSequence));
            if (targetSequence == null)
                throw new ArgumentNullException(nameof(targetSequence));

            throw new NotSupportedException(SequencesNotSupported);
        }

        public void AlterTable(IRelationalDatabaseTable existingTable, IRelationalDatabaseTable targetTable)
        {
            if (existingTable == null)
                throw new ArgumentNullException(nameof(existingTable));
            if (targetTable == null)
                throw new ArgumentNullException(nameof(targetTable));

            var operation = new AlterTableOperation(existingTable, targetTable);
            Operations.Add(operation);
        }

        public void CreateIndex(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            var operation = new CreateIndexOperation(table, index);
            Operations.Add(operation);
        }

        public void CreateRoutine(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            throw new NotSupportedException(RoutinesNotSupported);
        }

        public void CreateSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            throw new NotSupportedException(SequencesNotSupported);
        }

        public void CreateSynonym(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            throw new NotSupportedException(SynonymsNotSupported);
        }

        public void CreateTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var operation = new CreateTableOperation(table);
            Operations.Add(operation);
        }

        public void CreateView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var operation = new CreateViewOperation(view);
            Operations.Add(operation);
        }

        public void DropCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            var operation = new DropCheckOperation(table, check);
            Operations.Add(operation);
        }

        public void DropColumn(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var operation = new DropColumnOperation(table, column);
            Operations.Add(operation);
        }

        public void DropForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            var operation = new DropForeignKeyOperation(table, foreignKey);
            Operations.Add(operation);
        }

        public void DropIndex(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            var operation = new DropIndexOperation(table, index);
            Operations.Add(operation);
        }

        public void DropPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            var operation = new DropPrimaryKeyOperation(table, primaryKey);
            Operations.Add(operation);
        }

        public void DropRoutine(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            throw new NotSupportedException(RoutinesNotSupported);
        }

        public void DropSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            throw new NotSupportedException(SequencesNotSupported);
        }

        public void DropSynonym(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            throw new NotSupportedException(SynonymsNotSupported);
        }

        public void DropTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var operation = new DropTableOperation(table);
            Operations.Add(operation);
        }

        public void DropTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            var operation = new DropTriggerOperation(table, trigger);
            Operations.Add(operation);
        }

        public void DropUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            var operation = new DropUniqueKeyOperation(table, uniqueKey);
            Operations.Add(operation);
        }

        public void DropView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var operation = new DropViewOperation(view);
            Operations.Add(operation);
        }

        public void RenameCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameCheckOperation(table, check, targetName);
            Operations.Add(operation);
        }

        public void RenameColumn(IRelationalDatabaseTable table, IDatabaseColumn column, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameColumnOperation(table, column, targetName);
            Operations.Add(operation);
        }

        public void RenameForeignKey(IRelationalDatabaseTable childTable, IRelationalDatabaseTable parentTable, IDatabaseRelationalKey foreignKey, Identifier targetName)
        {
            if (childTable == null)
                throw new ArgumentNullException(nameof(childTable));
            if (parentTable == null)
                throw new ArgumentNullException(nameof(parentTable));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameForeignKeyOperation(childTable, parentTable, foreignKey, targetName);
            Operations.Add(operation);
        }

        public void RenameIndex(IRelationalDatabaseTable table, IDatabaseIndex index, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameIndexOperation(table, index, targetName);
            Operations.Add(operation);
        }

        public void RenamePrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenamePrimaryKeyOperation(table, primaryKey, targetName);
            Operations.Add(operation);
        }

        public void RenameRoutine(IDatabaseRoutine routine, Identifier targetName)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotSupportedException(RoutinesNotSupported);
        }

        public void RenameSequence(IDatabaseSequence sequence, Identifier targetName)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotSupportedException(SequencesNotSupported);
        }

        public void RenameSynonym(IDatabaseSynonym synonym, Identifier targetName)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotSupportedException(SynonymsNotSupported);
        }

        public void RenameTable(IRelationalDatabaseTable table, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameTableOperation(table, targetName);
            Operations.Add(operation);
        }

        public void RenameTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameTriggerOperation(table, trigger, targetName);
            Operations.Add(operation);
        }

        public void RenameUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameUniqueKeyOperation(table, uniqueKey, targetName);
            Operations.Add(operation);
        }

        public void RenameView(IDatabaseView view, Identifier targetName)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var operation = new RenameViewOperation(view, targetName);
            Operations.Add(operation);
        }

        public void Sql(ISqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var operation = new SqlOperation(command);
            Operations.Add(operation);
        }

        private const string SequencesNotSupported = "SQLite does not support sequences.";
        private const string SynonymsNotSupported = "SQLite does not support synonyms.";
        private const string RoutinesNotSupported = "SQLite does not support routines.";
    }
}
