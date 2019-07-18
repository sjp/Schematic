using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations
{
    public class RelationalDatabaseDiffer : IRelationalDatabaseDiffer
    {
        public RelationalDatabaseDiffer(IRelationalDatabase database, IMigrationBuilder migrationBuilder)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            MigrationBuilder = migrationBuilder ?? throw new ArgumentNullException(nameof(migrationBuilder));
        }

        public IRelationalDatabase Database { get; }

        protected IMigrationBuilder MigrationBuilder { get; }

        public Task<IReadOnlyCollection<IMigrationOperation>> GetDifferences(IRelationalDatabase comparison, CancellationToken cancellationToken = default)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            return GetDifferencesCore(comparison, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IMigrationOperation>> GetDifferencesCore(IRelationalDatabase comparison, CancellationToken cancellationToken = default)
        {
            var tables = await Database.GetAllTables(cancellationToken).ConfigureAwait(false);
            var comparisonTables = await comparison.GetAllTables(cancellationToken).ConfigureAwait(false);
            CompareTables(tables, comparisonTables);

            var views = await Database.GetAllViews(cancellationToken).ConfigureAwait(false);
            var comparisonViews = await comparison.GetAllViews(cancellationToken).ConfigureAwait(false);
            CompareViews(views, comparisonViews);

            var sequences = await Database.GetAllSequences(cancellationToken).ConfigureAwait(false);
            var comparisonSequences = await Database.GetAllSequences(cancellationToken).ConfigureAwait(false);
            CompareSequences(sequences, comparisonSequences);

            var synonyms = await Database.GetAllSynonyms(cancellationToken).ConfigureAwait(false);
            var comparisonSynonyms = await Database.GetAllSynonyms(cancellationToken).ConfigureAwait(false);
            CompareSynonyms(synonyms, comparisonSynonyms);

            var routines = await Database.GetAllRoutines(cancellationToken).ConfigureAwait(false);
            var comparisonRoutines = await Database.GetAllRoutines(cancellationToken).ConfigureAwait(false);
            CompareRoutines(routines, comparisonRoutines);

            return await MigrationBuilder.BuildMigrations(cancellationToken).ConfigureAwait(false);
        }

        public Task<bool> HasDifferences(IRelationalDatabase comparison, CancellationToken cancellationToken = default)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            return HasDifferencesCore(comparison, cancellationToken);
        }

        private Task<bool> HasDifferencesCore(IRelationalDatabase comparison, CancellationToken cancellationToken = default)
        {
            if (comparison == null)
                throw new ArgumentNullException(nameof(comparison));

            return Task.FromResult(false);
        }

        protected virtual void CompareTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, IReadOnlyCollection<IRelationalDatabaseTable> comparisonTables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (comparisonTables == null)
                throw new ArgumentNullException(nameof(comparisonTables));

            CompareTablesCore(tables, comparisonTables);
        }

        private void CompareTablesCore(IReadOnlyCollection<IRelationalDatabaseTable> tables, IReadOnlyCollection<IRelationalDatabaseTable> comparisonTables)
        {
            var checkComparer = new Comparers.DatabaseCheckComparer();
            var dbTypeComparer = new Comparers.DbTypeComparer();
            var columnComparer = new Comparers.DatabaseColumnComparer(dbTypeComparer);
            var indexComparer = new Comparers.DatabaseIndexComparer(columnComparer);
            var keyComparer = new Comparers.DatabaseKeyComparer(columnComparer);
            var triggerComparer = new Comparers.DatabaseTriggerComparer();
            var relationalKeyComparer = new Comparers.DatabaseRelationalKeyComparer(keyComparer);

            var comparer = new Comparers.DatabaseTableComparer(
                checkComparer,
                columnComparer,
                indexComparer,
                relationalKeyComparer,
                keyComparer,
                triggerComparer
            );
            var tableLookup = BuildLookup(tables);
            var comparisonLookup = BuildLookup(comparisonTables);

            var toCreate = comparisonTables.Where(t => !tableLookup.ContainsKey(t.Name)).ToList();
            var toDrop = tables.Where(t => !comparisonLookup.ContainsKey(t.Name)).ToList();

            var sharedNames = tables
                .Where(t => comparisonLookup.ContainsKey(t.Name))
                .Select(t => t.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = tableLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !comparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.CreateTable(add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropTable(drop);

            foreach (var alter in alters)
            {
                CompareChecks(alter.Existing, alter.Existing.Checks, alter.Comparison.Checks);
                CompareColumns(alter.Existing, alter.Existing.Columns, alter.Comparison.Columns);
                CompareForeignKeys(alter.Existing, alter.Existing.ParentKeys, alter.Comparison.ParentKeys);
                CompareIndexes(alter.Existing, alter.Existing.Indexes, alter.Comparison.Indexes);
                ComparePrimaryKeys(alter.Existing, alter.Existing.PrimaryKey, alter.Comparison.PrimaryKey);
                CompareTriggers(alter.Existing, alter.Existing.Triggers, alter.Comparison.Triggers);
                CompareUniqueKeys(alter.Existing, alter.Existing.UniqueKeys, alter.Comparison.UniqueKeys);
            }
        }

        protected virtual void CompareChecks(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseCheckConstraint> checks, IReadOnlyCollection<IDatabaseCheckConstraint> comparisonChecks)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (checks == null)
                throw new ArgumentNullException(nameof(checks));
            if (comparisonChecks == null)
                throw new ArgumentNullException(nameof(comparisonChecks));

            CompareChecksCore(table, checks, comparisonChecks);
        }

        private void CompareChecksCore(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseCheckConstraint> checks, IReadOnlyCollection<IDatabaseCheckConstraint> comparisonChecks)
        {
            var comparer = new Comparers.DatabaseCheckComparer();

            var toCreate = comparisonChecks.Where(cc => !checks.Any(c => cc.Name == c.Name || cc.Definition == c.Definition)).ToList();
            var toDrop = checks.Where(cc => !comparisonChecks.Any(c => cc.Name == c.Name || cc.Definition == c.Definition)).ToList();

            var sharedNames = checks
                .Where(c => c.Name.IsSome && comparisonChecks.Any(cc => c.Name == cc.Name))
                .Select(c => (Identifier)c.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new
                {
                    Existing = checks.First(c => c.Name == name),
                    Comparison = comparisonChecks.First(c => c.Name == name)
                })
                .Where(compare => !comparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.AddCheck(table, add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropCheck(table, drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropCheck(table, alter.Existing);
                MigrationBuilder.AddCheck(table, alter.Comparison);
            }
        }

        protected virtual void CompareColumns(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseColumn> columns, IReadOnlyCollection<IDatabaseColumn> comparisonColumns)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (comparisonColumns == null)
                throw new ArgumentNullException(nameof(comparisonColumns));

            CompareColumnsCore(table, columns, comparisonColumns);
        }

        private void CompareColumnsCore(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseColumn> columns, IReadOnlyCollection<IDatabaseColumn> comparisonColumns)
        {
            var dbTypeComparer = new Comparers.DbTypeComparer();
            var columnComparer = new Comparers.DatabaseColumnComparer(dbTypeComparer);

            var columnLookup = BuildColumnLookup(columns);
            var comparisonLookup = BuildColumnLookup(comparisonColumns);

            var toCreate = comparisonColumns.Where(c => !columnLookup.ContainsKey(c.Name)).ToList();
            var toDrop = columns.Where(c => !comparisonLookup.ContainsKey(c.Name)).ToList();

            var sharedNames = columns
                .Where(c => comparisonLookup.ContainsKey(c.Name))
                .Select(c => c.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = columnLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !columnComparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.AddColumn(table, add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropColumn(table, drop);

            foreach (var alter in alters)
                MigrationBuilder.AlterColumn(table, alter.Existing, alter.Comparison);
        }

        protected virtual void CompareForeignKeys(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseRelationalKey> foreignKeys, IReadOnlyCollection<IDatabaseRelationalKey> comparisonForeignKeys)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (foreignKeys == null)
                throw new ArgumentNullException(nameof(foreignKeys));
            if (comparisonForeignKeys == null)
                throw new ArgumentNullException(nameof(comparisonForeignKeys));

            CompareForeignKeysCore(table, foreignKeys, comparisonForeignKeys);
        }

        private void CompareForeignKeysCore(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseRelationalKey> foreignKeys, IReadOnlyCollection<IDatabaseRelationalKey> comparisonForeignKeys)
        {
            var dbTypeComparer = new Comparers.DbTypeComparer();
            var columnComparer = new Comparers.DatabaseColumnComparer(dbTypeComparer);
            var keyComparer = new Comparers.DatabaseKeyComparer(columnComparer);
            var relationalKeyComparer = new Comparers.DatabaseRelationalKeyComparer(keyComparer);

            var toCreate = comparisonForeignKeys
                .Where(cfk => !foreignKeys.Any(fk =>
                    cfk.ChildKey.Name == fk.ChildKey.Name
                    || fk.ChildKey.Columns.Select(c => c.Name)
                        .SequenceEqual(cfk.ChildKey.Columns.Select(cc => cc.Name)))).ToList();
            var toDrop = foreignKeys
                .Where(fk => !comparisonForeignKeys.Any(cfk =>
                    fk.ChildKey.Name == cfk.ChildKey.Name
                    || cfk.ChildKey.Columns.Select(c => c.Name)
                        .SequenceEqual(fk.ChildKey.Columns.Select(cc => cc.Name)))).ToList();

            var sharedNames = foreignKeys
                .Where(fk => fk.ChildKey.Name.IsSome && comparisonForeignKeys.Any(cfk => fk.ChildKey.Name == cfk.ChildKey.Name))
                .Select(fk => (Identifier)fk.ChildKey.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new
                {
                    Existing = foreignKeys.First(fk => fk.ChildKey.Name == name),
                    Comparison = comparisonForeignKeys.First(fk => fk.ChildKey.Name == name)
                })
                .Where(compare => !relationalKeyComparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.AddForeignKey(table, add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropForeignKey(table, drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropForeignKey(table, alter.Existing);
                MigrationBuilder.AddForeignKey(table, alter.Comparison);
            }
        }

        protected virtual void CompareIndexes(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseIndex> indexes, IReadOnlyCollection<IDatabaseIndex> comparisonIndexes)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (indexes == null)
                throw new ArgumentNullException(nameof(indexes));
            if (comparisonIndexes == null)
                throw new ArgumentNullException(nameof(comparisonIndexes));

            CompareIndexesCore(table, indexes, comparisonIndexes);
        }

        private void CompareIndexesCore(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseIndex> indexes, IReadOnlyCollection<IDatabaseIndex> comparisonIndexes)
        {
            var dbTypeComparer = new Comparers.DbTypeComparer();
            var columnComparer = new Comparers.DatabaseColumnComparer(dbTypeComparer);
            var indexComparer = new Comparers.DatabaseIndexComparer(columnComparer);

            var indexLookup = BuildIndexLookup(indexes);
            var comparisonLookup = BuildIndexLookup(comparisonIndexes);

            var toCreate = comparisonIndexes.Where(i => !indexLookup.ContainsKey(i.Name)).ToList();
            var toDrop = indexes.Where(i => !comparisonLookup.ContainsKey(i.Name)).ToList();

            var sharedNames = indexes
                .Where(i => comparisonLookup.ContainsKey(i.Name))
                .Select(i => i.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = indexLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !indexComparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.CreateIndex(table, add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropIndex(table, drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropIndex(table, alter.Existing);
                MigrationBuilder.CreateIndex(table, alter.Comparison);
            }
        }

        protected virtual void ComparePrimaryKeys(IRelationalDatabaseTable table, Option<IDatabaseKey> primaryKey, Option<IDatabaseKey> comparisonPrimaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            ComparePrimaryKeysCore(table, primaryKey, comparisonPrimaryKey);
        }

        private void ComparePrimaryKeysCore(IRelationalDatabaseTable table, Option<IDatabaseKey> primaryKey, Option<IDatabaseKey> comparisonPrimaryKey)
        {
            var dbTypeComparer = new Comparers.DbTypeComparer();
            var columnComparer = new Comparers.DatabaseColumnComparer(dbTypeComparer);
            var keyComparer = new Comparers.DatabaseKeyComparer(columnComparer);

            if (primaryKey.IsNone)
                comparisonPrimaryKey.IfSome(pk => MigrationBuilder.AddPrimaryKey(table, pk));
            if (comparisonPrimaryKey.IsNone)
                primaryKey.IfSome(pk => MigrationBuilder.DropPrimaryKey(table, pk));

            if (primaryKey.IsNone || comparisonPrimaryKey.IsNone)
                return;

            var existingKey = primaryKey.IfNoneUnsafe(() => null);
            var comparisonKey = comparisonPrimaryKey.IfNoneUnsafe(() => null);
            if (!keyComparer.Equals(existingKey, comparisonKey))
            {
                MigrationBuilder.DropPrimaryKey(table, existingKey);
                MigrationBuilder.AddPrimaryKey(table, comparisonKey);
            }
        }

        protected virtual void CompareUniqueKeys(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseKey> uniqueKeys, IReadOnlyCollection<IDatabaseKey> comparisonUniqueKeys)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKeys == null)
                throw new ArgumentNullException(nameof(uniqueKeys));
            if (comparisonUniqueKeys == null)
                throw new ArgumentNullException(nameof(comparisonUniqueKeys));

            CompareUniqueKeysCore(table, uniqueKeys, comparisonUniqueKeys);
        }

        private void CompareUniqueKeysCore(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseKey> uniqueKeys, IReadOnlyCollection<IDatabaseKey> comparisonUniqueKeys)
        {
            var dbTypeComparer = new Comparers.DbTypeComparer();
            var columnComparer = new Comparers.DatabaseColumnComparer(dbTypeComparer);
            var keyComparer = new Comparers.DatabaseKeyComparer(columnComparer);

            var toCreate = comparisonUniqueKeys.Where(cuk => !uniqueKeys.Any(uk => cuk.Name == uk.Name || uk.Columns.Select(c => c.Name).SequenceEqual(cuk.Columns.Select(cc => cc.Name)))).ToList();
            var toDrop = uniqueKeys.Where(uk => !comparisonUniqueKeys.Any(cuk => uk.Name == cuk.Name || cuk.Columns.Select(c => c.Name).SequenceEqual(uk.Columns.Select(cc => cc.Name)))).ToList();

            var sharedNames = uniqueKeys
                .Where(uk => uk.Name.IsSome && comparisonUniqueKeys.Any(cuk => uk.Name == cuk.Name))
                .Select(uk => (Identifier)uk.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new
                {
                    Existing = uniqueKeys.First(uk => uk.Name == name),
                    Comparison = comparisonUniqueKeys.First(uk => uk.Name == name)
                })
                .Where(compare => !keyComparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.AddUniqueKey(table, add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropUniqueKey(table, drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropUniqueKey(table, alter.Existing);
                MigrationBuilder.AddUniqueKey(table, alter.Comparison);
            }
        }

        protected virtual void CompareTriggers(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseTrigger> triggers, IReadOnlyCollection<IDatabaseTrigger> comparisonTriggers)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (triggers == null)
                throw new ArgumentNullException(nameof(triggers));
            if (comparisonTriggers == null)
                throw new ArgumentNullException(nameof(comparisonTriggers));

            CompareTriggersCore(table, triggers, comparisonTriggers);
        }

        private void CompareTriggersCore(IRelationalDatabaseTable table, IReadOnlyCollection<IDatabaseTrigger> triggers, IReadOnlyCollection<IDatabaseTrigger> comparisonTriggers)
        {
            var comparer = new Comparers.DatabaseTriggerComparer();
            var triggerLookup = BuildTriggerLookup(triggers);
            var comparisonLookup = BuildTriggerLookup(comparisonTriggers);

            var toCreate = comparisonTriggers.Where(t => !triggerLookup.ContainsKey(t.Name)).ToList();
            var toDrop = triggers.Where(t => !comparisonLookup.ContainsKey(t.Name)).ToList();

            var sharedNames = triggers
                .Where(t => comparisonLookup.ContainsKey(t.Name))
                .Select(t => t.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = triggerLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !comparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.AddTrigger(table, add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropTrigger(table, drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropTrigger(table, alter.Existing);
                MigrationBuilder.AddTrigger(table, alter.Comparison);
            }
        }

        protected virtual void CompareViews(IReadOnlyCollection<IDatabaseView> views, IReadOnlyCollection<IDatabaseView> comparisonViews)
        {
            if (views == null)
                throw new ArgumentNullException(nameof(views));
            if (comparisonViews == null)
                throw new ArgumentNullException(nameof(comparisonViews));

            CompareViewsCore(views, comparisonViews);
        }

        private void CompareViewsCore(IReadOnlyCollection<IDatabaseView> views, IReadOnlyCollection<IDatabaseView> comparisonViews)
        {
            var comparer = new Comparers.DatabaseViewComparer();
            var viewLookup = BuildLookup(views);
            var comparisonLookup = BuildLookup(comparisonViews);

            var toCreate = comparisonViews.Where(v => !viewLookup.ContainsKey(v.Name)).ToList();
            var toDrop = views.Where(v => !comparisonLookup.ContainsKey(v.Name)).ToList();

            var sharedNames = views
                .Where(v => comparisonLookup.ContainsKey(v.Name))
                .Select(v => v.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = viewLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !comparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.CreateView(add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropView(drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropView(alter.Existing);
                MigrationBuilder.CreateView(alter.Comparison);
            }
        }

        protected virtual void CompareSequences(IReadOnlyCollection<IDatabaseSequence> sequences, IReadOnlyCollection<IDatabaseSequence> comparisonSequences)
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));
            if (comparisonSequences == null)
                throw new ArgumentNullException(nameof(comparisonSequences));

            CompareSequencesCore(sequences, comparisonSequences);
        }

        private void CompareSequencesCore(IReadOnlyCollection<IDatabaseSequence> sequences, IReadOnlyCollection<IDatabaseSequence> comparisonSequences)
        {
            var comparer = new Comparers.DatabaseSequenceComparer();
            var sequenceLookup = BuildLookup(sequences);
            var comparisonLookup = BuildLookup(comparisonSequences);

            var toCreate = comparisonSequences.Where(s => !sequenceLookup.ContainsKey(s.Name)).ToList();
            var toDrop = sequences.Where(s => !comparisonLookup.ContainsKey(s.Name)).ToList();

            var sharedNames = sequences
                .Where(s => comparisonLookup.ContainsKey(s.Name))
                .Select(s => s.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = sequenceLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !comparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.CreateSequence(add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropSequence(drop);

            foreach (var alter in alters)
                MigrationBuilder.AlterSequence(alter.Existing, alter.Comparison);
        }

        protected virtual void CompareSynonyms(IReadOnlyCollection<IDatabaseSynonym> synonyms, IReadOnlyCollection<IDatabaseSynonym> comparisonSynonyms)
        {
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));
            if (comparisonSynonyms == null)
                throw new ArgumentNullException(nameof(comparisonSynonyms));

            CompareSynonymsCore(synonyms, comparisonSynonyms);
        }

        private void CompareSynonymsCore(IReadOnlyCollection<IDatabaseSynonym> synonyms, IReadOnlyCollection<IDatabaseSynonym> comparisonSynonyms)
        {
            var comparer = new Comparers.DatabaseSynonymComparer();
            var synonymLookup = BuildLookup(synonyms);
            var comparisonLookup = BuildLookup(comparisonSynonyms);

            var toCreate = comparisonSynonyms.Where(s => !synonymLookup.ContainsKey(s.Name)).ToList();
            var toDrop = synonyms.Where(s => !comparisonLookup.ContainsKey(s.Name)).ToList();

            var sharedNames = synonyms
                .Where(s => comparisonLookup.ContainsKey(s.Name))
                .Select(s => s.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = synonymLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !comparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.CreateSynonym(add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropSynonym(drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropSynonym(alter.Existing);
                MigrationBuilder.CreateSynonym(alter.Comparison);
            }
        }

        protected virtual void CompareRoutines(IReadOnlyCollection<IDatabaseRoutine> routines, IReadOnlyCollection<IDatabaseRoutine> comparisonRoutines)
        {
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));
            if (comparisonRoutines == null)
                throw new ArgumentNullException(nameof(comparisonRoutines));

            CompareRoutinesCore(routines, comparisonRoutines);
        }

        private void CompareRoutinesCore(IReadOnlyCollection<IDatabaseRoutine> routines, IReadOnlyCollection<IDatabaseRoutine> comparisonRoutines)
        {
            var comparer = new Comparers.DatabaseRoutineComparer();
            var routineLookup = BuildLookup(routines);
            var comparisonLookup = BuildLookup(comparisonRoutines);

            var toCreate = comparisonRoutines.Where(r => !routineLookup.ContainsKey(r.Name)).ToList();
            var toDrop = routines.Where(r => !comparisonLookup.ContainsKey(r.Name)).ToList();

            var sharedNames = routines
                .Where(r => comparisonLookup.ContainsKey(r.Name))
                .Select(r => r.Name)
                .ToList();
            var alters = sharedNames
                .Select(name => new { Existing = routineLookup[name], Comparison = comparisonLookup[name] })
                .Where(compare => !comparer.Equals(compare.Existing, compare.Comparison))
                .ToList();

            foreach (var add in toCreate)
                MigrationBuilder.CreateRoutine(add);

            foreach (var drop in toDrop)
                MigrationBuilder.DropRoutine(drop);

            foreach (var alter in alters)
            {
                MigrationBuilder.DropRoutine(alter.Existing);
                MigrationBuilder.CreateRoutine(alter.Comparison);
            }
        }

        private static IReadOnlyDictionary<Identifier, T> BuildLookup<T>(IReadOnlyCollection<T> values) where T : IDatabaseEntity
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var result = new Dictionary<Identifier, T>(values.Count);

            foreach (var value in values)
                result[value.Name] = value;

            return result;
        }

        private static IReadOnlyDictionary<Identifier, IDatabaseColumn> BuildColumnLookup(IReadOnlyCollection<IDatabaseColumn> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var result = new Dictionary<Identifier, IDatabaseColumn>(values.Count);

            foreach (var value in values)
                result[value.Name] = value;

            return result;
        }

        private static IReadOnlyDictionary<Identifier, IDatabaseIndex> BuildIndexLookup(IReadOnlyCollection<IDatabaseIndex> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var result = new Dictionary<Identifier, IDatabaseIndex>(values.Count);

            foreach (var value in values)
                result[value.Name] = value;

            return result;
        }

        private static IReadOnlyDictionary<Identifier, IDatabaseTrigger> BuildTriggerLookup(IReadOnlyCollection<IDatabaseTrigger> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var result = new Dictionary<Identifier, IDatabaseTrigger>(values.Count);

            foreach (var value in values)
                result[value.Name] = value;

            return result;
        }
    }
}
