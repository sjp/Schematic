using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations.Comparers
{
    public sealed class OptionalNameComparer : EqualityComparer<Option<Identifier>>
    {
        public OptionalNameComparer() : this(IdentifierComparer.Ordinal) { }

        public OptionalNameComparer(IEqualityComparer<Identifier> comparer)
        {
            Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        private IEqualityComparer<Identifier> Comparer { get; }

        public override bool Equals(Option<Identifier> x, Option<Identifier> y)
        {
            if (x.IsNone && y.IsNone)
                return true;

            if (x.IsNone && y.IsSome)
                return false;

            return x.Match(
                xName => y.Match(yName => Comparer.Equals(xName, yName), () => false),
                () => false
            );
        }

        public override int GetHashCode(Option<Identifier> obj) => obj.GetHashCode();
    }
}
