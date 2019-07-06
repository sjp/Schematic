using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Migrations;
using SJP.Schematic.Migrations.Operations;
using SJP.Schematic.Migrations.Operations.Comparers;

namespace SJP.Schematic.SqlServer.Migrations.Reducers
{
    public class DedupReducer : IMigrationOperationReducer
    {
        public IEnumerable<IMigrationOperation> Reduce(IEnumerable<IMigrationOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            var addCheckOperations = operations.OfType<AddCheckOperation>().Distinct(GetComparer<AddCheckOperation>());
            var addColumnOperations = operations.OfType<AddColumnOperation>().Distinct(GetComparer<AddColumnOperation>());
            var addForeignKeyOperations = operations.OfType<AddForeignKeyOperation>().Distinct(GetComparer<AddForeignKeyOperation>());
            var addPrimaryKeyOperations = operations.OfType<AddPrimaryKeyOperation>().Distinct(GetComparer<AddPrimaryKeyOperation>());
            var addUniqueKeyOperations = operations.OfType<AddUniqueKeyOperation>().Distinct(GetComparer<AddUniqueKeyOperation>());
            var alterColumnOperations = operations.OfType<AlterColumnOperation>().Distinct(GetComparer<AlterColumnOperation>());
            var alterSequenceOperations = operations.OfType<AlterSequenceOperation>().Distinct(GetComparer<AlterSequenceOperation>());
            var alterTableOperations = operations.OfType<AlterTableOperation>().Distinct(GetComparer<AlterTableOperation>());
            var createIndexOperations = operations.OfType<CreateIndexOperation>().Distinct(GetComparer<CreateIndexOperation>());
            var createRoutineOperations = operations.OfType<CreateRoutineOperation>().Distinct(GetComparer<CreateRoutineOperation>());
            var createSequenceOperations = operations.OfType<CreateSequenceOperation>().Distinct(GetComparer<CreateSequenceOperation>());
            var createSynonymOperations = operations.OfType<CreateSynonymOperation>().Distinct(GetComparer<CreateSynonymOperation>());
            var createTableOperations = operations.OfType<CreateTableOperation>().Distinct(GetComparer<CreateTableOperation>());
            var createViewOperations = operations.OfType<CreateViewOperation>().Distinct(GetComparer<CreateViewOperation>());
            var dropCheckOperations = operations.OfType<DropCheckOperation>().Distinct(GetComparer<DropCheckOperation>());
            var dropColumnOperations = operations.OfType<DropColumnOperation>().Distinct(GetComparer<DropColumnOperation>());
            var dropForeignKeyOperations = operations.OfType<DropForeignKeyOperation>().Distinct(GetComparer<DropForeignKeyOperation>());
            var dropIndexOperations = operations.OfType<DropIndexOperation>().Distinct(GetComparer<DropIndexOperation>());
            var dropPrimaryKeyOperations = operations.OfType<DropPrimaryKeyOperation>().Distinct(GetComparer<DropPrimaryKeyOperation>());
            var dropRoutineOperations = operations.OfType<DropRoutineOperation>().Distinct(GetComparer<DropRoutineOperation>());
            var dropSequenceOperations = operations.OfType<DropSequenceOperation>().Distinct(GetComparer<DropSequenceOperation>());
            var dropSynonymOperations = operations.OfType<DropSynonymOperation>().Distinct(GetComparer<DropSynonymOperation>());
            var dropTableOperations = operations.OfType<DropTableOperation>().Distinct(GetComparer<DropTableOperation>());
            var dropTriggerOperations = operations.OfType<DropTriggerOperation>().Distinct(GetComparer<DropTriggerOperation>());
            var dropUniqueKeyOperations = operations.OfType<DropUniqueKeyOperation>().Distinct(GetComparer<DropUniqueKeyOperation>());
            var dropViewOperations = operations.OfType<DropViewOperation>().Distinct(GetComparer<DropViewOperation>());
            var renameCheckOperations = operations.OfType<RenameCheckOperation>().Distinct(GetComparer<RenameCheckOperation>());
            var renameColumnOperations = operations.OfType<RenameColumnOperation>().Distinct(GetComparer<RenameColumnOperation>());
            var renameForeignKeyOperations = operations.OfType<RenameForeignKeyOperation>().Distinct(GetComparer<RenameForeignKeyOperation>());
            var renameIndexOperations = operations.OfType<RenameIndexOperation>().Distinct(GetComparer<RenameIndexOperation>());
            var renamePrimaryKeyOperations = operations.OfType<RenamePrimaryKeyOperation>().Distinct(GetComparer<RenamePrimaryKeyOperation>());
            var renameRoutineOperations = operations.OfType<RenameRoutineOperation>().Distinct(GetComparer<RenameRoutineOperation>());
            var renameSequenceOperations = operations.OfType<RenameSequenceOperation>().Distinct(GetComparer<RenameSequenceOperation>());
            var renameSynonymOperations = operations.OfType<RenameSynonymOperation>().Distinct(GetComparer<RenameSynonymOperation>());
            var renameTableOperations = operations.OfType<RenameTableOperation>().Distinct(GetComparer<RenameTableOperation>());
            var renameTriggerOperations = operations.OfType<RenameTriggerOperation>().Distinct(GetComparer<RenameTriggerOperation>());
            var renameUniqueKeyOperations = operations.OfType<RenameUniqueKeyOperation>().Distinct(GetComparer<RenameUniqueKeyOperation>());
            var renameViewOperations = operations.OfType<RenameViewOperation>().Distinct(GetComparer<RenameViewOperation>());

            var combinedDistinctOperations = Enumerable.Empty<IMigrationOperation>()
                .Concat(addCheckOperations)
                .Concat(addColumnOperations)
                .Concat(addForeignKeyOperations)
                .Concat(addPrimaryKeyOperations)
                .Concat(addUniqueKeyOperations)
                .Concat(alterColumnOperations)
                .Concat(alterSequenceOperations)
                .Concat(alterTableOperations)
                .Concat(createIndexOperations)
                .Concat(createRoutineOperations)
                .Concat(createSequenceOperations)
                .Concat(createSynonymOperations)
                .Concat(createTableOperations)
                .Concat(createViewOperations)
                .Concat(dropCheckOperations)
                .Concat(dropColumnOperations)
                .Concat(dropForeignKeyOperations)
                .Concat(dropIndexOperations)
                .Concat(dropPrimaryKeyOperations)
                .Concat(dropRoutineOperations)
                .Concat(dropSequenceOperations)
                .Concat(dropSynonymOperations)
                .Concat(dropTableOperations)
                .Concat(dropTriggerOperations)
                .Concat(dropUniqueKeyOperations)
                .Concat(dropViewOperations)
                .Concat(renameCheckOperations)
                .Concat(renameColumnOperations)
                .Concat(renameForeignKeyOperations)
                .Concat(renameIndexOperations)
                .Concat(renamePrimaryKeyOperations)
                .Concat(renameRoutineOperations)
                .Concat(renameSequenceOperations)
                .Concat(renameSynonymOperations)
                .Concat(renameTableOperations)
                .Concat(renameTriggerOperations)
                .Concat(renameUniqueKeyOperations)
                .Concat(renameViewOperations)
                .ToList();

            var result = new List<IMigrationOperation>();

            foreach (var operation in operations)
            {
                var operationType = operation.GetType();
                if (!Comparers.ContainsKey(operationType))
                {
                    result.Add(operation);
                    continue;
                }

                var toKeep = combinedDistinctOperations.Any(op => ReferenceEquals(op, operation));
                if (toKeep)
                    result.Add(operation);
            }

            return result;
        }

        private static IEqualityComparer<T> GetComparer<T>()
        {
            return Comparers.TryGetValue(typeof(T), out var comparer)
                ? comparer as IEqualityComparer<T> ?? EqualityComparer<T>.Default
                : EqualityComparer<T>.Default;
        }

        private static readonly IReadOnlyDictionary<Type, object> Comparers = new Dictionary<Type, object>
        {
            [typeof(AddCheckOperation)] = new AddCheckComparer(),
            [typeof(AddColumnOperation)] = new AddColumnComparer(),
            [typeof(AddForeignKeyOperation)] = new AddForeignKeyComparer(),
            [typeof(AddPrimaryKeyOperation)] = new AddPrimaryKeyComparer(),
            [typeof(AddTriggerOperation)] = new AddTriggerComparer(),
            [typeof(AddUniqueKeyOperation)] = new AddUniqueKeyComparer(),
            [typeof(AlterColumnOperation)] = new AlterColumnComparer(),
            [typeof(AlterSequenceOperation)] = new AlterSequenceComparer(),
            [typeof(AlterTableOperation)] = new AlterTableComparer(),
            [typeof(CreateIndexOperation)] = new CreateIndexComparer(),
            [typeof(CreateRoutineOperation)] = new CreateRoutineComparer(),
            [typeof(CreateSequenceOperation)] = new CreateSequenceComparer(),
            [typeof(CreateSynonymOperation)] = new CreateSynonymComparer(),
            [typeof(CreateTableOperation)] = new CreateTableComparer(),
            [typeof(CreateViewOperation)] = new CreateViewComparer(),
            [typeof(DropCheckOperation)] = new DropCheckComparer(),
            [typeof(DropColumnOperation)] = new DropColumnComparer(),
            [typeof(DropForeignKeyOperation)] = new DropForeignKeyComparer(),
            [typeof(DropIndexOperation)] = new DropIndexComparer(),
            [typeof(DropPrimaryKeyOperation)] = new DropPrimaryKeyComparer(),
            [typeof(DropRoutineOperation)] = new DropRoutineComparer(),
            [typeof(DropSequenceOperation)] = new DropSequenceComparer(),
            [typeof(DropSynonymOperation)] = new DropSynonymComparer(),
            [typeof(DropTableOperation)] = new DropTableComparer(),
            [typeof(DropTriggerOperation)] = new DropTriggerComparer(),
            [typeof(DropUniqueKeyOperation)] = new DropUniqueKeyComparer(),
            [typeof(DropViewOperation)] = new DropViewComparer(),
            [typeof(RenameCheckOperation)] = new RenameCheckComparer(),
            [typeof(RenameColumnOperation)] = new RenameColumnComparer(),
            [typeof(RenameForeignKeyOperation)] = new RenameForeignKeyComparer(),
            [typeof(RenameIndexOperation)] = new RenameIndexComparer(),
            [typeof(RenamePrimaryKeyOperation)] = new RenamePrimaryKeyComparer(),
            [typeof(RenameRoutineOperation)] = new RenameRoutineComparer(),
            [typeof(RenameSequenceOperation)] = new RenameSequenceComparer(),
            [typeof(RenameSynonymOperation)] = new RenameSynonymComparer(),
            [typeof(RenameTableOperation)] = new RenameTableComparer(),
            [typeof(RenameTriggerOperation)] = new RenameTriggerComparer(),
            [typeof(RenameUniqueKeyOperation)] = new RenameUniqueKeyComparer(),
            [typeof(RenameViewOperation)] = new RenameViewComparer()
        };
    }
}
