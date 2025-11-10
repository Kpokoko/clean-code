using Markdown;

public class TagBuilder
{
    public static Tag Bold => new("strong");
    public static Tag Italic => new("em");
    public static Tag Header => new("h1");
    
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
            default:
                throw new Exception($"Unknown tag '{markdownTag}'");
        }
    }
}