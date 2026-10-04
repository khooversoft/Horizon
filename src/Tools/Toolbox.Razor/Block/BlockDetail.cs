//using Toolbox.Tools;

//namespace Toolbox.Razor.Block;

//public record BlockDetail
//{
//    public BlockDetail(string? height = null, string? width = null, string? backgroundImage = null, string? backgroundSize = null)
//    {
//        Height = height;
//        Width = width;
//        BackgroundImage = backgroundImage;
//        BackgroundSize = backgroundSize ?? "100% 100%";
//    }

//    public string? Height { get; init; }
//    public string? Width { get; init; }
//    public string? BackgroundImage { get; init; }
//    public string? BackgroundSize { get; init; }
//    public string? BackgroundColor { get; init; }

//    public string ToCssClass()
//    {
//        string? backgroundImage = BackgroundImage is null ? null : $"background-image: url('{BackgroundImage}'); background-size: {BackgroundSize}; background-repeat: no-repeat;";
//        string? sHeight = Height is null ? null : $"height: {Height};";
//        string? sWidth = Width is null ? null : $"width: {Width};";

//        var style = new CssStyleBuilder()
//            .Add(sHeight)
//            .Add(sWidth)
//            .Add(backgroundImage)
//            .ToString();

//        return style;
//    }
//}
