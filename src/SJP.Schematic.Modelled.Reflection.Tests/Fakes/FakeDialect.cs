using System.Data;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes
{
    public class FakeDialect : DatabaseDialect<FakeDialect>
    {
        public override IDbConnection CreateConnection(string connectionString, bool openConnection = true) => null;

        public override string GetTypeName(DataType dataType) => "int";

        public override bool IsValidColumnName(Identifier name) => true;

        public override bool IsValidConstraintName(Identifier name) => true;

        public override bool IsValidObjectName(Identifier name) => true;
    }
}
