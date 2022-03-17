using System.IO.Abstractions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.Tests;

internal sealed class FakeDatabaseViewGenerator : DatabaseViewGenerator
{
    public FakeDatabaseViewGenerator(IFileSystem fileSystem, INameTranslator nameTranslator)
        : base(fileSystem, nameTranslator)
    {
    }

    public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment) => string.Empty;

    public IFileInfo InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
}
