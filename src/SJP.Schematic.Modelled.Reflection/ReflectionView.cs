using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionView : IRelationalDatabaseView
    {
        public ReflectionView(IRelationalDatabase database, Type viewType)
        {
            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            Name = database.Dialect.GetQualifiedNameOrDefault(database, ViewType);
        }

        protected Type ViewType { get; }

        public string Definition => throw new NotImplementedException();

        public IReadOnlyList<IDatabaseColumn> Columns
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyCollection<IDatabaseIndex> Indexes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsIndexed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Identifier Name { get; }
    }
}
