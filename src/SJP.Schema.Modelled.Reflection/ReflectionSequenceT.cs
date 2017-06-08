using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionSequence<T> : IDatabaseSequence where T : ISequence, new()
    {
        public ReflectionSequence(IRelationalDatabase database)
        {
            var instance = new T();
            Cache = instance.Cache;
            Cycle = instance.Cycle;
            Increment = instance.Increment;
            MaxValue = instance.MaxValue;
            MinValue = instance.MinValue;
            Start = instance.Start;

            var dialect = database.Dialect;
            Name = dialect.GetQualifiedNameOverrideOrDefault(database, SequenceType);
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

        protected Type SequenceType { get; } = typeof(T);
    }
}
