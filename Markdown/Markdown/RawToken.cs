namespace Markdown;

public class RawToken
{
    public TokenType Type { get; set; }
    public int StartIndex { get; set; }

    public RawToken(TokenType type, int startIndex)
    {
        this.Type = type;
        this.StartIndex = startIndex;
    }
}