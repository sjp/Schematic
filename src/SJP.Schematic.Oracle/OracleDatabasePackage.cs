using System;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class OracleDatabasePackage : IOracleDatabasePackage
    {
        public OracleDatabasePackage(Identifier name, string specification, Option<string> body)
        {
            if (specification.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(specification));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Specification = specification;
            Body = body;
        }

        public Identifier Name { get; }

        public string Specification { get; }

        public Option<string> Body { get; }

        public string Definition
        {
            get
            {
                var bodyText = Body.Match(b => Environment.NewLine + b, () => string.Empty);
                return Specification + bodyText;
            }
        }

        public override string ToString() => "Package: " + Name.ToString();

        private string DebuggerDisplay
        {
            get
            {
                var builder = StringBuilderCache.Acquire();

                builder.Append("Package: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append(".");

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
