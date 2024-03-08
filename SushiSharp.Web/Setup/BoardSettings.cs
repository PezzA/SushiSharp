namespace SushiSharp.Web.Setup;

public class BoardSettings
{
    public const int BoardWidth = 1200;
    public const int BoardHeight = 800;
    public const float CardScale = 0.4f;
    public const int CardWidth = 270;
    public const int CardHeight = 400;

    public const int CardGapNormal = 3;
    public static float CardScaleRatio => (1f - CardScale) / 2f;
    public static int ScaledCardWidthOffSet => (int)(CardWidth * CardScaleRatio);
    public static int ScaledCardHeightOffSet => (int)(CardHeight * CardScaleRatio);

    public static int ScaledCardWidth => (int)(CardWidth * CardScale);
    public static int ScaledCardHeight => (int)(CardWidth * CardScale);

}