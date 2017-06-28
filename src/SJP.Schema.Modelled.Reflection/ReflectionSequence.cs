using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schema.Core;
using System.Reflection;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionSequence : IDatabaseSequence
    {
        public ReflectionSequence(IRelationalDatabase database, Type sequenceType)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (sequenceType == null)
                throw new ArgumentNullException(nameof(sequenceType));

            SequenceType = sequenceType.GetTypeInfo();
            if (!_iSequenceType.IsAssignableFrom(SequenceType))
                throw new ArgumentException($"The sequence type { SequenceType.FullName } must implement the { _iSequenceType.FullName } interface.", nameof(sequenceType));
            var ctor = sequenceType.GetDefaultConstructor();
            if (ctor == null)
                throw new ArgumentException($"The sequence type { SequenceType.FullName } does not contain a default constructor.", nameof(sequenceType));

            var instance = ctor.Invoke(new object[0]) as ISequence;
            Cache = instance.Cache;
            Cycle = instance.Cycle;
            Increment = instance.Increment;
            MaxValue = instance.MaxValue;
            MinValue = instance.MinValue;
            Start = instance.Start;

            var dialect = database.Dialect;
            Name = dialect.GetQualifiedNameOverrideOrDefault(database, sequenceType);
        }

        public int Cache { get; }

        public bool Cycle { get; }

        // sequences can't depend on anything
        public IEnumerable<Identifier> Dependencies { get; } = Enumerable.Empty<Identifier>();

        // TODO:
        // this one is tricky...
        // would need to run through every check/default to make sure they're found
        public IEnumerable<Identifier> Dependents
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public decimal Increment { get; }

        public decimal? MaxValue { get; }

        public decimal? MinValue { get; }

        public Identifier Name { get; }

        public decimal Start { get; }

        public Task<IEnumerable<Identifier>> DependenciesAsync() => Task.FromResult(Dependencies);

        public Task<IEnumerable<Identifier>> DependentsAsync() => Task.FromResult(Dependents);

        protected TypeInfo SequenceType { get; }

        private readonly static TypeInfo _iSequenceType = typeof(ISequence).GetTypeInfo();
    }
}
