namespace Markdown;

public class MarkdownParser : IParser
{
    public string Render(string markdown)
    {
        throw new NotImplementedException();
    }

    public List<Token> TokenizeText(string markdown)
    {
        throw new NotImplementedException();
    }

    public List<string> WrapTokensWithTags(List<Token> tokens)
    {
        throw new NotImplementedException();
    }

    public string RecoverStringFromTagList(List<string> tags)
    {
        throw new NotImplementedException();
    }
}