﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetUserSynonymNameQuery
    {
        public string SynonymName { get; init; } = default!;
    }
}
