namespace Markdown;

public interface IParser
{
    public string Render(string markdown);

    public List<Token> TokenizeText(string markdown);

    public List<string> WrapTokensWithTags(List<Token> tokens);

    public string RecoverStringFromTagList(List<string> tags);
}