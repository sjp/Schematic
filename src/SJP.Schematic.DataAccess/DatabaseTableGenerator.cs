using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// Common functionality for generating database tables.
/// </summary>
/// <seealso cref="IDatabaseTableGenerator" />
public abstract class DatabaseTableGenerator : IDatabaseTableGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTableGenerator"/> class.
    /// </summary>
    /// <param name="fileSystem">A file system to generate paths for.</param>
    /// <param name="nameTranslator">A name translator.</param>
    /// <exception cref="ArgumentNullException"><paramref name="nameTranslator"/> is <c>null</c>.</exception>
    protected DatabaseTableGenerator(IFileSystem fileSystem, INameTranslator nameTranslator)
    {
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
    }

    /// <summary>
    /// A file system.
    /// </summary>
    /// <value>The file system.</value>
    protected IFileSystem FileSystem { get; }

    /// <summary>
    /// A name translator.
    /// </summary>
    /// <value>The name translator.</value>
    protected INameTranslator NameTranslator { get; }

    /// <summary>
    /// Generates source code that enables interoperability with a given database table.
    /// </summary>
    /// <param name="tables">The database tables in the database.</param>
    /// <param name="table">A database table.</param>
    /// <param name="comment">Comment information for the given table.</param>
    /// <returns>A string containing source code to interact with the table.</returns>
    public abstract string Generate(IReadOnlyCollection<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment);

    /// <summary>
    /// Gets the file path that the source code should be generated to.
    /// </summary>
    /// <param name="baseDirectory">The base directory.</param>
    /// <param name="objectName">The name of the database object.</param>
    /// <returns>A file path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="baseDirectory"/> or <paramref name="objectName"/> is <c>null</c>.</exception>
    public virtual IFileInfo GetFilePath(IDirectoryInfo baseDirectory, Identifier objectName)
    {
        ArgumentNullException.ThrowIfNull(baseDirectory);
        ArgumentNullException.ThrowIfNull(objectName);

        var paths = new List<string> { baseDirectory.FullName, "Tables" };
        if (objectName.Schema != null)
        {
            var schemaName = NameTranslator.SchemaToNamespace(objectName);
            if (schemaName != null)
                paths.Add(schemaName);
        }

        var tableName = NameTranslator.TableToClassName(objectName);
        paths.Add(tableName + ".cs");

        var tablePath = FileSystem.Path.Combine(paths.ToArray());
        return FileSystem.FileInfo.FromFileName(tablePath);
    }
}