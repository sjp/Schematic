using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    internal sealed class FakeDatabaseTableGenerator : DatabaseTableGenerator
    {
        public FakeDatabaseTableGenerator(INameProvider nameProvider, string indent)
            : base(nameProvider, indent)
        {
        }

        public override string Generate(IRelationalDatabaseTable table) => string.Empty;

        public FileInfoBase InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
