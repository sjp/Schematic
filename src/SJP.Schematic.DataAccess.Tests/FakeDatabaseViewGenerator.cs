using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    internal sealed class FakeDatabaseViewGenerator : DatabaseViewGenerator
    {
        public FakeDatabaseViewGenerator(INameTranslator nameTranslator, string indent)
            : base(nameTranslator, indent)
        {
        }

        public override string Generate(IDatabaseView view) => string.Empty;

        public FileInfoBase InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
