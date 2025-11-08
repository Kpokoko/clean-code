namespace Markdown;

public class Token
{
    public int StartIndex { get; set; }
    public int TokenLength { get; set; }
    public TokenType Type { get; set; }
    public Token NestedToken { get; set; }
    public IEnumerable<char> TokenContent { get; set; }

    public Token(int startIndex, int tokenLength, TokenType type, Token nestedToken)
    {
        this.StartIndex = startIndex;
        this.TokenLength = tokenLength;
        this.Type = type;
        this.NestedToken = nestedToken;
    }
}