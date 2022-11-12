namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseRoutineComments
{
    public required Identifier RoutineName { get; init; }

    public string? Comment { get; init; }
}