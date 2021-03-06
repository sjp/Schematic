﻿using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionView : IDatabaseView
    {
        public ReflectionView(IRelationalDatabase database, IDatabaseDialect dialect, Type viewType)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));

            ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
            Name = dialect.GetQualifiedNameOrDefault(database, ViewType);
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

        public Identifier Name { get; }

        public bool IsMaterialized => throw new NotImplementedException();
    }
}
