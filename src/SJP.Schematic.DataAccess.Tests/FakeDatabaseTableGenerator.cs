using System.IO.Abstractions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.Tests
{
    internal sealed class FakeDatabaseTableGenerator : DatabaseTableGenerator
    {
        public FakeDatabaseTableGenerator(INameTranslator nameTranslator, string indent)
            : base(nameTranslator, indent)
        {
        }

        public override string Generate(IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment) => string.Empty;

        public FileInfoBase InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
