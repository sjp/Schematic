using System.IO.Abstractions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.Tests
{
    internal sealed class FakeDatabaseViewGenerator : DatabaseViewGenerator
    {
        public FakeDatabaseViewGenerator(INameTranslator nameTranslator, string indent)
            : base(nameTranslator, indent)
        {
        }

        public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment) => string.Empty;

        public FileInfoBase InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
    }
}
