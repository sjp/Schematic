using System;

namespace SJP.Schematic.SchemaSpy.Dot
{
    internal abstract class DotNode
    {
        protected DotNode(DotIdentifier identifier)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            _dotBuilder = new Lazy<string>(BuildDot);
        }

        public DotIdentifier Identifier { get; }

        public override string ToString() => _dotBuilder.Value;

        protected abstract string BuildDot();

        private readonly Lazy<string> _dotBuilder;
    }
}
