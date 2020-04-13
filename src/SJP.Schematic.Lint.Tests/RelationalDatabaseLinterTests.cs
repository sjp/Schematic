using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests
{
    [TestFixture]
    internal static class RelationalDatabaseLinterTests
    {
        private static IRelationalDatabaseLinter Linter { get; } = new RelationalDatabaseLinter(Array.Empty<IRule>());

        private static EmptyRelationalDatabase EmptyDatabase { get; } = new EmptyRelationalDatabase(new IdentifierDefaults(null, null, null));

        [Test]
        public static void Ctor_GivenNullRules_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseLinter(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            Assert.That(() => Linter.AnalyseDatabase(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseDatabase_GivenEmptyDatabase_ReturnsEmptyMessages()
        {
            var messages = await Linter.AnalyseDatabase(EmptyDatabase).ToListAsync().ConfigureAwait(false);

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            Assert.That(() => Linter.AnalyseTables(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseTables_GivenEmptyTables_ReturnsEmptyMessages()
        {
            var messages = await Linter.AnalyseTables(Array.Empty<IRelationalDatabaseTable>()).ToListAsync().ConfigureAwait(false);

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public static void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
        {
            Assert.That(() => Linter.AnalyseViews(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseViews_GivenEmptyViews_ReturnsEmptyMessages()
        {
            var messages = await Linter.AnalyseViews(Array.Empty<IDatabaseView>()).ToListAsync().ConfigureAwait(false);

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public static void AnalyseSequences_GivenNullSequences_ThrowsArgumentNullException()
        {
            Assert.That(() => Linter.AnalyseSequences(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseSequences_GivenEmptySequences_ReturnsEmptyMessages()
        {
            var messages = await Linter.AnalyseSequences(Array.Empty<IDatabaseSequence>()).ToListAsync().ConfigureAwait(false);

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public static void AnalyseSynonyms_GivenNullSynonyms_ThrowsArgumentNullException()
        {
            Assert.That(() => Linter.AnalyseSynonyms(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseSynonyms_GivenEmptySynonyms_ReturnsEmptyMessages()
        {
            var messages = await Linter.AnalyseSynonyms(Array.Empty<IDatabaseSynonym>()).ToListAsync().ConfigureAwait(false);

            Assert.That(messages, Is.Empty);
        }

        [Test]
        public static void AnalyseRoutines_GivenNullRoutines_ThrowsArgumentNullException()
        {
            Assert.That(() => Linter.AnalyseRoutines(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseRoutines_GivenEmptyRoutines_ReturnsEmptyMessages()
        {
            var messages = await Linter.AnalyseRoutines(Array.Empty<IDatabaseRoutine>()).ToListAsync().ConfigureAwait(false);

            Assert.That(messages, Is.Empty);
        }
    }
}
