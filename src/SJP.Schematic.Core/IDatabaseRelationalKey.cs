namespace SJP.Schematic.Core
{
    public interface IDatabaseRelationalKey
    {
        Identifier ParentTable { get; }

        IDatabaseKey ParentKey { get; }

        Identifier ChildTable { get; }

        IDatabaseKey ChildKey { get; }

        ReferentialAction UpdateAction { get; }

        ReferentialAction DeleteAction { get; }
    }
}
