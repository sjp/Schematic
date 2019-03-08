using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.DataAccess.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendComment(this StringBuilder builder, string indent, string comment)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (indent == null)
                throw new ArgumentNullException(nameof(indent));
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            var commentLines = GetLines(comment);
            var summaryContent = commentLines.Count > 1
                ? commentLines.Select(l => "/// <para>" + l + "</para>").Join(Environment.NewLine + indent)
                : "/// " + commentLines.Single();

            return builder.Append(indent)
                .AppendLine("/// <summary>")
                .Append(indent)
                .AppendLine(summaryContent)
                .Append(indent)
                .AppendLine("/// </summary>");
        }

        private static IReadOnlyCollection<string> GetLines(string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            return comment.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
