using System;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle
{
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
                var bodyText = Body.Match(b => b, () => string.Empty);
                return Specification + bodyText;
            }
        }
    }
}
