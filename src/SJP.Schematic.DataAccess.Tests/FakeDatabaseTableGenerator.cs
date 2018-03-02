using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    internal class FakeDatabaseTableGenerator : DatabaseTableGenerator
    {
        public FakeDatabaseTableGenerator(INameProvider nameProvider)
            : base(nameProvider)
        {
        }

        public override string Generate(IRelationalDatabaseTable table) => string.Empty;

        public FileInfoBase InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
