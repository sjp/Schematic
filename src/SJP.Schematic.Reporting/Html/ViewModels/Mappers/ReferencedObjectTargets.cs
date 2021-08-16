﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ReferencedObjectTargets
    {
        public ReferencedObjectTargets(
            IDependencyProvider dependencyProvider,
            IEnumerable<IRelationalDatabaseTable> tables,
            IEnumerable<IDatabaseView> views,
            IEnumerable<IDatabaseSequence> sequences,
            IEnumerable<IDatabaseSynonym> synonyms,
            IEnumerable<IDatabaseRoutine> routines
        )
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (views == null)
                throw new ArgumentNullException(nameof(views));
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));
            if (synonyms == null)
                throw new ArgumentNullException(nameof(synonyms));
            if (routines == null)
                throw new ArgumentNullException(nameof(routines));

            DependencyProvider = dependencyProvider ?? throw new ArgumentNullException(nameof(dependencyProvider));
            TableNames = tables.Select(static t => t.Name).ToList();
            ViewNames = views.Select(static v => v.Name).ToList();
            SequenceNames = sequences.Select(static s => s.Name).ToList();
            SynonymNames = synonyms.Select(static s => s.Name).ToList();
            RoutineNames = routines.Select(static r => r.Name).ToList();
        }

        private IDependencyProvider DependencyProvider { get; }

        private IEnumerable<Identifier> TableNames { get; }

        private IEnumerable<Identifier> ViewNames { get; }

        private IEnumerable<Identifier> SequenceNames { get; }

        private IEnumerable<Identifier> SynonymNames { get; }

        private IEnumerable<Identifier> RoutineNames { get; }

        public IReadOnlyCollection<HtmlString> GetReferencedObjectLinks(string rootPath, Identifier objectName, string expression)
        {
            if (rootPath == null)
                throw new ArgumentNullException(nameof(rootPath));
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (expression.IsNullOrWhiteSpace())
                return Array.Empty<HtmlString>();

            var referencedNames = DependencyProvider.GetDependencies(objectName, expression);
            if (referencedNames.Count == 0)
                return Array.Empty<HtmlString>();

            var referencedUris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<HtmlString>();

            var orderedNames = referencedNames
                .OrderBy(static name => name.Schema ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(static name => name.LocalName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var name in orderedNames)
            {
                var qualifiedName = QualifyReferenceName(objectName, name);
                var targetLinks = GetReferenceTargetLinks(rootPath, objectName, qualifiedName);
                var linkTexts = targetLinks
                    .Where(link => referencedUris.Add(link.TargetUri.ToString()))
                    .Select(static link => new HtmlString($"<a href=\"{ link.TargetUri }\">{ HttpUtility.HtmlEncode(link.ObjectName.ToVisibleName()) }</a>"))
                    .ToList();
                result.AddRange(linkTexts);
            }

            return result;
        }

        private IReadOnlyCollection<Link> GetReferenceTargetLinks(string rootPath, Identifier objectName, Identifier referenceName)
        {
            if (rootPath == null)
                throw new ArgumentNullException(nameof(rootPath));
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (referenceName == null)
                throw new ArgumentNullException(nameof(referenceName));

            var qualifiedReference = QualifyReferenceName(objectName, referenceName);
            var isSelfReference = string.Equals(objectName.Schema, qualifiedReference.Schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(objectName.LocalName, qualifiedReference.LocalName, StringComparison.OrdinalIgnoreCase);
            if (isSelfReference)
                return Array.Empty<Link>();

            var result = new List<Link>();

            var matchingTables = GetMatchingObjects(TableNames, qualifiedReference)
                .Select(name => new Link(name, new Uri(rootPath + UrlRouter.GetTableUrl(name), UriKind.Relative)));
            result.AddRange(matchingTables);

            var matchingViews = GetMatchingObjects(ViewNames, qualifiedReference)
                .Select(name => new Link(name, new Uri(rootPath + UrlRouter.GetViewUrl(name), UriKind.Relative)));
            result.AddRange(matchingViews);

            var matchingSequences = GetMatchingObjects(SequenceNames, qualifiedReference)
                .Select(name => new Link(name, new Uri(rootPath + UrlRouter.GetSequenceUrl(name), UriKind.Relative)));
            result.AddRange(matchingSequences);

            var matchingSynonyms = GetMatchingObjects(SynonymNames, qualifiedReference)
                .Select(name => new Link(name, new Uri(rootPath + UrlRouter.GetSynonymUrl(name), UriKind.Relative)));
            result.AddRange(matchingSynonyms);

            var matchingRoutines = GetMatchingObjects(RoutineNames, qualifiedReference)
                .Select(name => new Link(name, new Uri(rootPath + UrlRouter.GetRoutineUrl(name), UriKind.Relative)));
            result.AddRange(matchingRoutines);

            return result;
        }

        private static IReadOnlyCollection<Identifier> GetMatchingObjects(IEnumerable<Identifier> objectNames, Identifier referenceName)
        {
            return objectNames
                .Where(name => string.Equals(name.Schema, referenceName.Schema, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(name.LocalName, referenceName.LocalName, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();
        }

        private static Identifier QualifyReferenceName(Identifier objectName, Identifier referenceName)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (referenceName == null)
                throw new ArgumentNullException(nameof(referenceName));

            return Identifier.CreateQualifiedIdentifier(
                referenceName.Server ?? objectName.Server,
                referenceName.Database ?? objectName.Database,
                referenceName.Schema ?? objectName.Schema,
                referenceName.LocalName ?? objectName.LocalName
            );
        }

        private sealed class Link
        {
            public Link(Identifier objectName, Uri targetUri)
            {
                ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
                TargetUri = targetUri ?? throw new ArgumentNullException(nameof(targetUri));
            }

            public Identifier ObjectName { get; }

            public Uri TargetUri { get; }

            public override string ToString() => string.Empty;
        }
    }
}
