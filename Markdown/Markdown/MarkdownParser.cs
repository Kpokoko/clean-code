using System.Text;

namespace Markdown;

public class MarkdownParser : IParser
{
    private static Dictionary<string, TokenType> _markdownToTag = new()
    {
        {"_", TokenType.Italic},
        {"__", TokenType.Bold},
        {"#", TokenType.Title}
    };
    
    private string _markdown;
    
    public void SetMarkdownText(string markdown) => this._markdown = markdown; // Только для теста обёртки токенов в теги
    
    public string Render(string markdown)
    {
        _markdown = markdown;
        var tokensList = TokenizeText(markdown);
        return BuildHTMLString(tokensList);
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
            
            if (currentTag == "__" && stack.Any(s => s.Type == TokenType.Italic))
            {
                i += tagLength - 1;
                continue;
            }

            var isOpening = stack.Count == 0 || stack.Peek().Type != _markdownToTag[currentTag];
            var isTagCorrect = currentTag is not null && tagValidator.IsTagPartCorrect(i, isOpening, tagLength);
            if (!isTagCorrect)
            {
                i += tagLength - 1;
                continue;
            }
            if (stack.Count == 0 || stack.Peek().Type != _markdownToTag[currentTag])
            {
                stack.Push(new RawToken(_markdownToTag[currentTag], tagLength, i));
            }
            else
            {
                var opening = stack.Pop();
                if (tagValidator.HasTagContentInside(opening.StartIndex + tagLength, i - 1) &&
                    tagValidator.HasTagDigitsInside(opening.StartIndex + tagLength, i - 1) &&
                    !tagValidator.IsTagSplittingWord(opening.StartIndex, i))
                {
                    tokens.Add(CreateToken(opening, i));
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
                tokens.Add(CreateToken(stack.Peek(), markdown.Length));
                isLastCharUsed = true;
            }

            stack.Pop();
        }
        return tokens;
    }

    public List<string> WrapTokensWithTags(List<Token> tokens)
    {
        var wrappedTokens = new List<string>();
        var tagBuilder = new TagBuilder();
        foreach (var token in tokens)
        {
            var tags = tagBuilder.BuildTag(token.Type.ToString());
            var builder = new StringBuilder();
            builder.Append(tags.openingTag)
                .Append(_markdown.Substring(token.StartIndex + token.TokenMarkLength, token.TokenLength))
                .Append(tags.closingTag);
            wrappedTokens.Add(builder.ToString());
        }
        return wrappedTokens;
    }

    public string BuildHTMLString(List<Token> tokens)
    {
        tokens = tokens
            .OrderBy(t => t.StartIndex)
            .ToList();
        var wrappedTags = WrapTokensWithTags(tokens);
        if (string.IsNullOrWhiteSpace(_markdown) || wrappedTags.Count == 0 || wrappedTags.Count == 0)
            return _markdown;
        var htmlString = new StringBuilder();
        var currEndIndex = 0;
        
        for (var i = 0; i < tokens.Count; ++i)
        {
            var token = tokens[i];
            if (token.StartIndex > currEndIndex)
                htmlString.Append(_markdown.Substring(currEndIndex,
                    token.StartIndex - currEndIndex));
            if (token.StartIndex >= currEndIndex)
                htmlString.Append(wrappedTags[i]);
            else
            {
                var oldData = _markdown
                    .Substring(token.StartIndex, token.TokenLength + token.TokenMarkLength * 2);
                htmlString.Replace(oldData, wrappedTags[i]);
            }
            currEndIndex = Math.Max(token.StartIndex + token.TokenLength + token.TokenMarkLength * 2, currEndIndex);
        }
        var lastToken = tokens[^1];
        if (currEndIndex < _markdown.Length - lastToken.TokenMarkLength)
            htmlString.Append(_markdown.Substring(currEndIndex,
                _markdown.Length - lastToken.TokenMarkLength - currEndIndex + 1));
        return htmlString.ToString();
    }

    private Token CreateToken(RawToken rawToken, int endIndex)
    {
        var startIndex = rawToken.StartIndex;
        return new Token(startIndex, endIndex - rawToken.StartIndex - rawToken.TokenMarkLength, rawToken.Type, rawToken.TokenMarkLength);
    }
}