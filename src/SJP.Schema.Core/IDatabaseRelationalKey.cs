﻿namespace SJP.Schema.Core
{
    public interface IDatabaseRelationalKey
    {
        IDatabaseKey ParentKey { get; }

        IDatabaseKey ChildKey { get; }

        RelationalKeyUpdateAction UpdateAction { get; }

        RelationalKeyUpdateAction DeleteAction { get; }
    }
}
