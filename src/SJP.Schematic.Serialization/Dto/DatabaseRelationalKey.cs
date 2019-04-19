using System.Data;

namespace SJP.Schematic.Serialization.Dto
{
    public class DatabaseRelationalKey
    {
        public Identifier ChildTable { get; set; }

        public DatabaseKey ChildKey { get; set; }

        public Identifier ParentTable { get; set; }

        public DatabaseKey ParentKey { get; set; }

        public Rule DeleteRule { get; set; }

        public Rule UpdateRule { get; set; }
    }
}