using System.Text.Json.Serialization;

namespace TaskBoard.Mcp;

// String converter so the tool's input schema lists the column names, not 0/1/2.
[JsonConverter(typeof(JsonStringEnumConverter<BoardColumn>))]
public enum BoardColumn
{
    ToDo,
    InProgress,
    Done,
}
