using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class CreateViewOperation : MigrationOperation
    {
        public CreateViewOperation(IDatabaseView view)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public IDatabaseView View { get; }
    }
}
