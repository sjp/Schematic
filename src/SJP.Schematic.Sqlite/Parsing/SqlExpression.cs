
using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using Superpower.Model;

namespace SJP.Schematic.Sqlite.Parsing
{
    internal class SqlExpression
    {
        public SqlExpression(IEnumerable<Token<SqliteToken>> tokens)
        {
            if (tokens == null || tokens.Empty())
                throw new ArgumentNullException(nameof(tokens));

            Tokens = tokens.ToList();
        }

        public IEnumerable<Token<SqliteToken>> Tokens { get; }
    }
}
