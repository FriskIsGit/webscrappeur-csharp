namespace WebScrapper.scrapper;

public class Tags{
    public const string Div = "div";
    public const string Html = "html";
    public const string Head = "head";
    public const string Body = "body";
    public const string TBody = "tbody";
    public const string Style = "style";
    public const string Paragraph = "p";
    public const string UnorderedList = "ul";
    public const string OrderedList = "ol";
    public const string ListItem = "li";
    public const string Footer = "footer";
    public const string Span = "span";
    public const string TSpan = "tspan";
    public const string IFrame = "iframe";
    public const string Table = "table";
    public const string TableRow = "tr";
    public const string TableHeader = "th";
    public const string TableDataCell = "td";
    public const string Script = "script";
    public const string Link = "link";
    public const string Label = "label";
    public const string Header = "link";
    public const string IdiomaticText = "i";
    public const string Main = "main";
    public const string Map = "map";
    public const string Mark = "mark";
    public const string Section = "section";
    public const string Svg = "svg";
    public const string Anchor = "a";
    public const string Audio = "audio";
    public const string Use = "use";
    public const string Text = "text";
    public const string View = "view";
    public const string Time = "time";
    public const string Figure = "figure";

    public const string Image = "img";
    public const string Input = "input";
    public const string ThematicBreak = "hr";
    public const string Meta = "meta";
    public const string LineBreak = "br";

    private static readonly List<string> SELF_CLOSING = new(){ Image, Input, LineBreak, ThematicBreak, Meta};

    public static bool IsSelfClosing(string tag){
        return SELF_CLOSING.Contains(tag);
    }
}


