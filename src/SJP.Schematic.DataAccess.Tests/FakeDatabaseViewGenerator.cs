using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    internal sealed class FakeDatabaseViewGenerator : DatabaseViewGenerator
    {
        public FakeDatabaseViewGenerator(INameProvider nameProvider)
            : base(nameProvider)
        {
        }

        public override string Generate(IRelationalDatabaseView view) => string.Empty;

        public FileInfoBase InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
