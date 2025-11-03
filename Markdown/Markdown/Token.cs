namespace Markdown;

public class Token
{
    public int StartIndex;
    public int TokenLength;
    public TokenType Type;
    public Token NestedToken;
}