using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class DropViewOperation : MigrationOperation
    {
        public DropViewOperation(IDatabaseView view)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IDatabaseView View { get; }
    }
}
