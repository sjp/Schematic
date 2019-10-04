using System.Collections.Generic;
using System.Threading;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations
{
    public interface IMigrationBuilder
    {
        IAsyncEnumerable<IMigrationOperation> BuildMigrations(CancellationToken cancellationToken = default);

        void AddCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check);

        void AddColumn(IRelationalDatabaseTable table, IDatabaseColumn column);

        void AddForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey);

        void AddPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey);

        void AddTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger);

        void AddUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey);

        void AlterColumn(IRelationalDatabaseTable table, IDatabaseColumn existingColumn, IDatabaseColumn targetColumn);

        void AlterSequence(IDatabaseSequence existingSequence, IDatabaseSequence targetSequence);

        void AlterTable(IRelationalDatabaseTable existingTable, IRelationalDatabaseTable targetTable);

        void CreateIndex(IRelationalDatabaseTable table, IDatabaseIndex index);

        void CreateRoutine(IDatabaseRoutine routine);

        void CreateSequence(IDatabaseSequence sequence);

        void CreateSynonym(IDatabaseSynonym synonym);

        void CreateTable(IRelationalDatabaseTable table);

        void CreateView(IDatabaseView view);

        void DropCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check);

        void DropColumn(IRelationalDatabaseTable table, IDatabaseColumn column);

        void DropForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey);

        void DropIndex(IRelationalDatabaseTable table, IDatabaseIndex index);

        void DropPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey);

        void DropRoutine(IDatabaseRoutine routine);

        void DropSequence(IDatabaseSequence sequence);

        void DropSynonym(IDatabaseSynonym synonym);

        void DropTable(IRelationalDatabaseTable table);

        void DropTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger);

        void DropUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey);

        void DropView(IDatabaseView view);

        void RenameCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check, Identifier targetName);

        void RenameColumn(IRelationalDatabaseTable table, IDatabaseColumn column, Identifier targetName);

        void RenameForeignKey(IRelationalDatabaseTable childTable, IRelationalDatabaseTable parentTable, IDatabaseRelationalKey foreignKey, Identifier targetName);

        void RenameIndex(IRelationalDatabaseTable table, IDatabaseIndex index, Identifier targetName);

        void RenamePrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey, Identifier targetName);

        void RenameRoutine(IDatabaseRoutine routine, Identifier targetName);

        void RenameSequence(IDatabaseSequence sequence, Identifier targetName);

        void RenameSynonym(IDatabaseSynonym synonym, Identifier targetName);

        void RenameTable(IRelationalDatabaseTable table, Identifier targetName);

        void RenameTrigger(IRelationalDatabaseTable table, IDatabaseTrigger trigger, Identifier targetName);

        void RenameUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey, Identifier targetName);

        void RenameView(IDatabaseView view, Identifier targetName);

        void Sql(ISqlCommand command);
    }
}
