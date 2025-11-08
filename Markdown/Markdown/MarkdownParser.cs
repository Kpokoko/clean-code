namespace Markdown;

public class MarkdownParser : IParser
{
    private static Dictionary<string, TokenType> _markdownToTag = new()
    {
        {"_", TokenType.Italic},
        {"__", TokenType.Bold},
        {"#", TokenType.Title}
    };
    public string Render(string markdown)
    {
        var tokensList = TokenizeText(markdown);
        return "";
    }

    public List<Token> TokenizeText(string markdown)
    {
        var stack = new Stack<RawToken>();
        var tokens = new List<Token>();
        var tagValidator = new MarkdownTagValidator(markdown);
        for (var i = 0; i < markdown.Length; ++i)
        {
            if (!_markdownToTag.ContainsKey(markdown[i].ToString()))
                continue;
            string currentTag = null;
            var tagLength = 1;
            if (markdown[i] == '_' && i + 1 < markdown.Length && markdown[i + 1] == '_')
            {
                currentTag = "__";
                tagLength = 2;
            }
            else
                currentTag = markdown[i].ToString();
            if (currentTag == "#")
                tagLength = 2;

            var isOpening = stack.Count == 0;
            var isTagCorrect = currentTag is not null && tagValidator.IsTagPartCorrect(i, isOpening, tagLength);
            if (!isTagCorrect)
            {
                i += tagLength - 1;
                continue;
            }
            if (stack.Count == 0 || stack.Peek().Type != _markdownToTag[currentTag])
            {
                stack.Push(new RawToken(_markdownToTag[currentTag], i + tagLength));
            }
            else
            {
                var opening = stack.Pop();
                if (tagValidator.HasTagContentInside(opening.StartIndex + tagLength, i - 1) &&
                    tagValidator.HasTagDigitsInside(opening.StartIndex + tagLength, i - 1))
                {
                    tokens.Add(CreateToken(opening, i, markdown));
                }
            }
            i += tagLength - 1;
        }

        var isLastCharUsed = false;
        while (stack.Count > 0)
        {
            if (stack.Peek().Type is TokenType.Title
                || stack.Peek().Type is TokenType.Italic && markdown[^1] == '_' && !isLastCharUsed)
            {
                tokens.Add(CreateToken(stack.Peek(), markdown.Length, markdown));
                isLastCharUsed = true;
            }

            stack.Pop();
        }
        return tokens;
    }

    public List<string> WrapTokensWithTags(List<Token> tokens)
    {
        throw new NotImplementedException();
    }

    public string RecoverStringFromTagList(List<string> tags)
    {
        throw new NotImplementedException();
    }

    private Token CreateToken(RawToken rawToken, int endIndex, string markdown)
    {
        var startIndex = rawToken.StartIndex;
        // while (_markdownToTag.ContainsKey(markdown[startIndex].ToString()) || markdown[startIndex] == ' ')
        //     ++startIndex;
        // while (_markdownToTag.ContainsKey(markdown[endIndex].ToString()) || markdown[endIndex] == ' ')
        //     --endIndex;
        return new Token(startIndex, endIndex - startIndex, rawToken.Type, null);
    }
}