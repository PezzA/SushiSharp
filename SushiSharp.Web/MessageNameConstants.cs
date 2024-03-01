namespace SushiSharp.Web;

public static class ServerMessages
{
    public const string GameList = "GameList";
    public const string LobbyChat = "LobbyChat";
    public const string ErrorMessage = "ErrorMessage";
    
    public const string SetGame = "SetGame";
    public const string SetPlayerData = "SetPlayerData";
    public const string SetPlayStatus = "SetPlayStatus";
}

public static class ClientMessage
{
    public const string InitClient = "InitClient";
    public const string StartGame = "StartGame";
    public const string JoinGame = "JoinGame";
    public const string LeaveGame = "LeaveGame";
    public const string CreateGame = "CreateGame";
    public const string SendLobbyChat = "SendLobbyChat";
}