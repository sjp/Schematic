using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Tests.Fakes
{
    public class FakeDialect : DatabaseDialect<FakeDialect>
    {
        private FakeDialect() { }

        public static IDatabaseDialect Instance { get; } = new FakeDialect();

        public override IDbConnection CreateConnection(string connectionString, bool openConnection = true) => null;

        public override string GetTypeName(DataType dataType) => "int";

        public override bool IsValidColumnName(Identifier name) => true;

        public override bool IsValidConstraintName(Identifier name) => true;

        public override bool IsValidObjectName(Identifier name) => true;
    }
}
