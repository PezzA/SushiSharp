namespace BoardCutter.Games.SushiGo.Web.Setup;

public class CardState
{
    public int Top { get; set; }
    public int Left { get; set; }

    public bool ReadOnly { get; set; }
    public bool IsSelected { get; set; }
    public bool IsLocked { get; set; }

    public string AnimClass { get; set; } = string.Empty;
}