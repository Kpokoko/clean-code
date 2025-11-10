using Markdown;

public class TagBuilder
{
    public static Tag Bold => new("strong");
    public static Tag Italic => new("em");
    public static Tag Header => new("h1");
    public static Tag ListItem => new("li");
    
    public (string openingTag, string closingTag) BuildTag(string markdownTag)
    {
        switch (markdownTag)
        {
            case "Bold":
                return (Bold.OpeningTag, Bold.ClosingTag);
            case "Italic":
                return (Italic.OpeningTag, Italic.ClosingTag);
            case "Title":
                return (Header.OpeningTag, Header.ClosingTag);
            case "ListItem":
                return (ListItem.OpeningTag, ListItem.OpeningTag);
            default:
                throw new Exception($"Unknown tag '{markdownTag}'");
        }
    }
}