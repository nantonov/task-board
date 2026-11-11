namespace TaskBoard.Mcp;

public sealed class BoardApiException(string message, Exception? innerException = null)
    : Exception(message, innerException);
