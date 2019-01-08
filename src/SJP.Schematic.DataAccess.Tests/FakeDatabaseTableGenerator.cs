using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    internal sealed class FakeDatabaseTableGenerator : DatabaseTableGenerator
    {
        public FakeDatabaseTableGenerator(INameTranslator nameTranslator, string indent)
            : base(nameTranslator, indent)
        {
        }

        public override string Generate(IRelationalDatabaseTable table) => string.Empty;

        public FileInfoBase InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
