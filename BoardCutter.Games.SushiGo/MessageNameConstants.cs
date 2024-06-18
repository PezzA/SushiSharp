namespace BoardCutter.Games.SushiGo;

public static class ServerMessages
{
    public const string GameList = "GameList";
    public const string LobbyChat = "LobbyChat";
    public const string ErrorMessage = "ErrorMessage";
    
    public const string SetPlayerGame = "SetGame";
    public const string SetPlayerVisibleData = "SetPlayerVisibleData";
    public const string SetViewerVisibleData = "SetViewerVisibleData";
    public const string SetPlayerTurnStatus = "SetPlayerTurnStatus";
    public const string SetIdentity = "SetIdentity";
}

public static class ClientMessage
{
    public const string InitClient = "InitClient";
    public const string InitClientGame = "InitClientGame";
    public const string StartGame = "StartGame";
    public const string JoinGame = "JoinGame";
    public const string LeaveGame = "LeaveGame";
    public const string CreateGame = "CreateGame";
    public const string SendLobbyChat = "SendLobbyChat";
    public const string SubmitTurn = "SubmitTurn";
}