namespace Markdown;

public interface IParser
{
    public string Render(string markdown);

    public List<Token> TokenizeText(string markdown);

    public string BuildHTMLString(List<Token> tokens);
}