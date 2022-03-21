using System.Collections.Generic;
using System.IO.Abstractions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess.Tests;

internal sealed class FakeDatabaseTableGenerator : DatabaseTableGenerator
{
    public FakeDatabaseTableGenerator(IFileSystem fileSystem, INameTranslator nameTranslator)
        : base(fileSystem, nameTranslator)
    {
    }

    public override string Generate(IReadOnlyCollection<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment) => string.Empty;

    public IFileInfo InnerGetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName) => GetFilePath(baseDirectory, objectName);
}