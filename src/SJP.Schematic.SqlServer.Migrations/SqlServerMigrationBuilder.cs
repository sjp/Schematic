using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;
using SJP.Schematic.SqlServer.Migrations.Analyzers;

namespace SJP.Schematic.SqlServer.Migrations
{
    public class SqlServerMigrationBuilder : IMigrationBuilder
    {
        public SqlServerMigrationBuilder(IDbConnection connection, IDatabaseDialect dialect, IRelationalDatabase database)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            OperationRegistry = BuildRegistry(Connection, Dialect, Database);
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IRelationalDatabase Database { get; }

        protected IList<IMigrationOperation> Operations { get; } = new List<IMigrationOperation>();

        protected IList<IMigrationError> Errors { get; } = new List<IMigrationError>();

        protected MigrationOperationRegistry OperationRegistry { get; }

        private static MigrationOperationRegistry BuildRegistry(IDbConnection connection, IDatabaseDialect dialect, IRelationalDatabase database)
        {
            var registry = new MigrationOperationRegistry();
            registry.AddAnalyzer<AddCheckOperation>(new AddCheckAnalyzer());


            return registry;
        }

        public EitherAsync<IReadOnlyCollection<IMigrationError>, IReadOnlyCollection<IMigrationOperation>> BuildMigrations(CancellationToken cancellationToken = default(CancellationToken))
        {
            return EitherAsync<IReadOnlyCollection<IMigrationError>, IReadOnlyCollection<IMigrationOperation>>.Right(Array.Empty<IMigrationOperation>());
        }

        public void AddCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            throw new NotImplementedException();
        }

        public void AddColumn(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            throw new NotImplementedException();
        }

        public void AddForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            throw new NotImplementedException();
        }

        public void AddPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            throw new NotImplementedException();
        }

        public void AddTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            throw new NotImplementedException();
        }

        public void AddUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            throw new NotImplementedException();
        }

        public void AlterColumn(IRelationalDatabaseTable table, IDatabaseColumn existingColumn, IDatabaseColumn targetColumn)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (existingColumn == null)
                throw new ArgumentNullException(nameof(existingColumn));
            if (targetColumn == null)
                throw new ArgumentNullException(nameof(targetColumn));

            throw new NotImplementedException();
        }

        public void AlterSequence(IDatabaseSequence existingSequence, IDatabaseSequence targetSequence)
        {
            if (existingSequence == null)
                throw new ArgumentNullException(nameof(existingSequence));
            if (targetSequence == null)
                throw new ArgumentNullException(nameof(targetSequence));

            throw new NotImplementedException();
        }

        public void AlterTable(IRelationalDatabaseTable existingTable, IRelationalDatabaseTable targetTable)
        {
            if (existingTable == null)
                throw new ArgumentNullException(nameof(existingTable));
            if (targetTable == null)
                throw new ArgumentNullException(nameof(targetTable));

            throw new NotImplementedException();
        }

        public void CreateIndex(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            throw new NotImplementedException();
        }

        public void CreateRoutine(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            throw new NotImplementedException();
        }

        public void CreateSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            throw new NotImplementedException();
        }

        public void CreateSynonym(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            throw new NotImplementedException();
        }

        public void CreateTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            throw new NotImplementedException();
        }

        public void CreateView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            throw new NotImplementedException();
        }

        public void DropCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            throw new NotImplementedException();
        }

        public void DropColumn(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            throw new NotImplementedException();
        }

        public void DropForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            throw new NotImplementedException();
        }

        public void DropIndex(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            throw new NotImplementedException();
        }

        public void DropPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            throw new NotImplementedException();
        }

        public void DropRoutine(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            throw new NotImplementedException();
        }

        public void DropSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            throw new NotImplementedException();
        }

        public void DropSynonym(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            throw new NotImplementedException();
        }

        public void DropTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            throw new NotImplementedException();
        }

        public void DropTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            throw new NotImplementedException();
        }

        public void DropUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            throw new NotImplementedException();
        }

        public void DropView(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            throw new NotImplementedException();
        }

        public void RenameCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (check == null)
                throw new ArgumentNullException(nameof(check));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameColumn(IRelationalDatabaseTable table, IDatabaseColumn column, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
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

            throw new NotImplementedException();
        }

        public void RenameIndex(IRelationalDatabaseTable table, IDatabaseIndex index, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenamePrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameRoutine(IDatabaseRoutine routine, Identifier targetName)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameSequence(IDatabaseSequence sequence, Identifier targetName)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameSynonym(IDatabaseSynonym synonym, Identifier targetName)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameTable(IRelationalDatabaseTable table, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey, Identifier targetName)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void RenameView(IDatabaseView view, Identifier targetName)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            throw new NotImplementedException();
        }

        public void Sql(ISqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            throw new NotImplementedException();
        }
    }
}
