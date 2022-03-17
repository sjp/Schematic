namespace SJP.Schematic.Serialization.Dto.Comments;

public class DatabaseRoutineComments
{
    public Identifier RoutineName { get; set; } = default!;

    public string? Comment { get; set; }
}
