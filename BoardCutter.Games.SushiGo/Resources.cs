namespace BoardCutter.Games.SushiGo;

public static class Resources
{
    public const string ResValidationMaxPlayers = "The maximum number of players has been reached";
    public const string ResValidationClientAlreadyInGame = "You are already in the game";
    public const string ResValidationCannotLeaveRunningGame = "You cannot leave a game that is running.";
    public const string ResValidationNotMemberOfTheGame = "You are not a member of the game you tried to leave(!)";
    public const string ResValidationPlayWasEmpty = "Sumbitted move contained no cards (or was null).";
    public const string ResValidationCreatorMustStart = "Only the player who created the game can start the game.";
    public const string ResValidationMinPlayers = "A game must have at least 2 players to begin.";
    
    public const string ResErrorNoNullCreator = "Game Creator is Null";
    public const string ResErrorGameNotExpectingPlay = "Game is not expecting a play message";
    public const string ResErrorDrawPileIsNull = "The draw pile is null";
}