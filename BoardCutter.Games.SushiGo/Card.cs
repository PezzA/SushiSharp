namespace BoardCutter.Games.SushiGo;

public record Card(int Id, CardSymbol[] Symbols, CardType Type, string Name, string Description);
