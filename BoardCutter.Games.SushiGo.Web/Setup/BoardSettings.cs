namespace BoardCutter.Games.SushiGo.Web.Setup;

public class BoardSettings
{
    public const float CardScale = 1f;
    public const int CardWidth = 100;
    public const int CardHeight = 150;

    public const int CardGapNormal = 5;

    public static int ScaledCardHeight => (int)(CardHeight * CardScale);

}