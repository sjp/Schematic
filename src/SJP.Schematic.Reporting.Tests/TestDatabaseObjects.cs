using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Tests;

// Builds a database object of each kind that has only a name, for tests that resolve objects by
// name and kind and read nothing else from them.
internal static class TestDatabaseObjects
{
    public static DatabaseObject Table(Identifier name) => new(Mock.Of<IRelationalDatabaseTable>(t => t.Name == name));

    public static DatabaseObject View(Identifier name) => new(Mock.Of<IDatabaseView>(v => v.Name == name));

    public static DatabaseObject Sequence(Identifier name) => new(Mock.Of<IDatabaseSequence>(s => s.Name == name));

    public static DatabaseObject Synonym(Identifier name) => new(Mock.Of<IDatabaseSynonym>(s => s.Name == name));

    public static DatabaseObject Routine(Identifier name) => new(Mock.Of<IDatabaseRoutine>(r => r.Name == name));

    public static DatabaseObject UserDefinedType(Identifier name) => new((DatabaseUserDefinedType)new DatabaseUserDefinedType.Unknown(name));
}
