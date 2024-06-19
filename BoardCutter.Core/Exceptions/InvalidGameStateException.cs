namespace BoardCutter.Core.Exceptions;

/// <summary>
/// InvalidGameStateException represents problems with internal state
/// </summary>
public class InvalidGameStateException : Exception
{
    public InvalidGameStateException()
    {
    }

    public InvalidGameStateException(string message)
        : base(message)
    {
    }

    public InvalidGameStateException(string message, Exception inner)
        : base(message, inner)
    {
    }
}