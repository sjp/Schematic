using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess
{
    /// <summary>
    /// Common functionality for generating database views.
    /// </summary>
    /// <seealso cref="IDatabaseViewGenerator" />
    public abstract class DatabaseViewGenerator : IDatabaseViewGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseViewGenerator"/> class.
        /// </summary>
        /// <param name="fileSystem">A file system to generate paths for.</param>
        /// <param name="nameTranslator">A name translator.</param>
        /// <exception cref="ArgumentNullException"><paramref name="nameTranslator"/> is <c>null</c>.</exception>
        protected DatabaseViewGenerator(IFileSystem fileSystem, INameTranslator nameTranslator)
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
        /// Generates source code that enables interoperability with a given database view.
        /// </summary>
        /// <param name="view">A database view.</param>
        /// <param name="comment">Comment information for the given view.</param>
        /// <returns>A string containing source code to interact with the view.</returns>
        public abstract string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment);

        /// <summary>
        /// Gets the file path that the source code should be generated to.
        /// </summary>
        /// <param name="baseDirectory">The base directory.</param>
        /// <param name="objectName">The name of the database object.</param>
        /// <returns>A file path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="baseDirectory"/> or <paramref name="objectName"/> is <c>null</c>.</exception>
        public virtual IFileInfo GetFilePath(IDirectoryInfo baseDirectory, Identifier objectName)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));

            var paths = new List<string> { baseDirectory.FullName, "Views" };
            if (objectName.Schema != null)
            {
                var schemaName = NameTranslator.SchemaToNamespace(objectName);
                if (schemaName != null)
                    paths.Add(schemaName);
            }

            var viewName = NameTranslator.ViewToClassName(objectName);
            paths.Add(viewName + ".cs");

            var viewPath = Path.Combine(paths.ToArray());
            var fileInfo = new FileInfo(viewPath);
            return new FileInfoWrapper(FileSystem, fileInfo);
        }
    }
}
