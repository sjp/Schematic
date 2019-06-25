using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations
{
    public interface IMigrationBuilder
    {
        IReadOnlyCollection<IMigrationOperation> GetMigrations();

        Task AddCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check);

        Task AddColumn(IRelationalDatabaseTable table, IDatabaseColumn column);

        Task AddForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey);

        Task AddPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey);

        Task AddUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey);

        Task AlterColumn(IRelationalDatabaseTable table, IDatabaseColumn existingColumn, IDatabaseColumn targetColumn);

        Task AlterSequence(IDatabaseSequence existingSequence, IDatabaseSequence targetSequence);

        Task AlterTable(IRelationalDatabaseTable existingTable, IRelationalDatabaseTable targetTable);

        Task CreateIndex(IRelationalDatabaseTable table, IDatabaseIndex index);

        Task CreateRoutine(IDatabaseRoutine routine);

        Task CreateSequence(IDatabaseSequence sequence);

        Task CreateSynonym(IDatabaseSynonym synonym);

        Task CreateTable(IRelationalDatabaseTable table);

        Task CreateView(IDatabaseView view);

        Task DropCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check);

        Task DropColumn(IRelationalDatabaseTable table, IDatabaseColumn column);

        Task DropForeignKey(IRelationalDatabaseTable table, IDatabaseRelationalKey foreignKey);

        Task DropIndex(IRelationalDatabaseTable table, IDatabaseIndex index);

        Task DropPrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey);

        Task DropRoutine(IDatabaseRoutine routine);

        Task DropSequence(IDatabaseSequence sequence);

        Task DropSynonym(IDatabaseSynonym synonym);

        Task DropTable(IRelationalDatabaseTable table);

        Task DropUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey);

        Task DropView(IDatabaseView view);

        Task RenameCheck(IRelationalDatabaseTable table, IDatabaseCheckConstraint check, Identifier targetName);

        Task RenameColumn(IRelationalDatabaseTable table, IDatabaseColumn column, Identifier targetName);

        Task RenameForeignKey(IRelationalDatabaseTable childTable, IRelationalDatabaseTable parentTable, IDatabaseRelationalKey foreignKey, Identifier targetName);

        Task RenameIndex(IRelationalDatabaseTable table, IDatabaseIndex index, Identifier targetName);

        Task RenamePrimaryKey(IRelationalDatabaseTable table, IDatabaseKey primaryKey, Identifier targetName);

        Task RenameRoutine(IDatabaseRoutine routine, Identifier targetName);

        Task RenameSequence(IDatabaseSequence sequence, Identifier targetName);

        Task RenameSynonym(IDatabaseSynonym synonym, Identifier targetName);

        Task RenameTable(IRelationalDatabaseTable table, Identifier targetName);

        Task RenameUniqueKey(IRelationalDatabaseTable table, IDatabaseKey uniqueKey, Identifier targetName);

        Task RenameView(IDatabaseView view, Identifier targetName);

        Task Sql(ISqlCommand command);
    }
}
