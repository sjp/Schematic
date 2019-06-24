using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Migrations.Operations
{
    public class RenameViewOperation : MigrationOperation
    {
        public RenameViewOperation(IDatabaseView view, Identifier targetName)
        {
            View = view ?? throw new ArgumentNullException(nameof(view));
            TargetName = targetName ?? throw new ArgumentNullException(nameof(targetName));
        }

        public IDatabaseView View { get; }

        public Identifier TargetName { get; }
    }
}
