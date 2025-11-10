namespace Markdown;

public class Token
{
    public int StartIndex { get; set; }
    public int TokenLength { get; set; }
    public TokenType Type { get; set; }
    public int TokenMarkLength { get; set; }
    // public Token NestedToken { get; set; }

    public Token(int startIndex, int tokenLength, TokenType type, int tokenMarkLength)
    {
        this.StartIndex = startIndex;
        this.TokenLength = tokenLength;
        this.Type = type;
        this.TokenMarkLength = tokenMarkLength;
    }
}