using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Text;

namespace SJP.Schematic.SchemaSpy.Dot
{
    public interface IDotFormatter
    {

    }

    public class DotFormatter : IDotFormatter
    {
        protected string ToDot(IDotNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return string.Empty;
        }

        protected string ToDot(IDotConnector connector)
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            var edge = new StringBuilder();

            var childTableIdentifier = ToSafeDotIdentifier(connector.Key.ChildKey.Table.Name);
            var parentTableIdentifier = ToSafeDotIdentifier(connector.Key.ParentKey.Table.Name);

            edge.Append(childTableIdentifier);
            edge.Append(":w -> ");
            edge.Append(parentTableIdentifier);
            edge.Append(":e ");

            // parent end of connector
            edge.Append("[arrowhead=none");
            edge.Append(" dir=back]");

            return edge.ToString();
        }

        protected static string ToSafeDotIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var builder = new StringBuilder("\"");

            if (identifier.Server != null)
            {
                builder.Append(ToSafeDotIdentifier(identifier.Server));
                builder.Append(".");
            }

            if (identifier.Database != null)
            {
                builder.Append(ToSafeDotIdentifier(identifier.Database));
                builder.Append(".");
            }

            if (identifier.Schema != null)
            {
                builder.Append(ToSafeDotIdentifier(identifier.Schema));
                builder.Append(".");
            }

            builder.Append(ToSafeDotIdentifier(identifier.LocalName));
            builder.Append("\"");

            return builder.ToString();
        }

        protected static string ToSafeDotIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return identifier.Replace("\"", "\\\"");
        }
    }

    // inject this into a dotnode and a dotconnector so they can write ToDot() returning a string?

}
