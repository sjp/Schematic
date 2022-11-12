using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Dto;

public class RelationalDatabase
{
    public IIdentifierResolutionStrategy? IdentifierResolver { get; set; }

    public required IdentifierDefaults IdentifierDefaults { get; init; }

    public required IEnumerable<RelationalDatabaseTable> Tables { get; init; }

    public required IEnumerable<DatabaseView> Views { get; init; }

    public required IEnumerable<DatabaseSequence> Sequences { get; init; }

    public required IEnumerable<DatabaseSynonym> Synonyms { get; init; }

    public required IEnumerable<DatabaseRoutine> Routines { get; init; }
}