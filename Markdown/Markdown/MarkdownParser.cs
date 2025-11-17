using System.Text;

namespace Markdown;

public class MarkdownParser : IMarkdownParser
{
    private readonly Dictionary<string, TokenType> _pairMarkdownToTag = new()
    {
        {"_", TokenType.Italic},
        {"__", TokenType.Bold},
        {"#", TokenType.Title},
    };
    
    public string Render(string markdown)
    {
        var tokensList = TokenizeText(markdown);
        var htmlWithPairTags = BuildHTMLString(tokensList, markdown);
        return ProcessUnpairLineTags(htmlWithPairTags);
    }

    public List<Token> TokenizeText(string markdown)
    {
        var stack = new Stack<RawToken>();
        var tokens = new List<Token>();
        var tagValidator = new MarkdownTagValidator(markdown);
        for (var i = 0; i < markdown.Length; ++i)
        {
            if (!_pairMarkdownToTag.ContainsKey(markdown[i].ToString()))
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

            var isOpening = stack.Count == 0 || stack.Peek().Type != _pairMarkdownToTag[currentTag];
            var isTagCorrect = tagValidator.IsTagPartCorrect(i, isOpening, tagLength);
            if (!isTagCorrect)
            {
                i += tagLength - 1;
                continue;
            }
            if (stack.Count == 0 || stack.Peek().Type != _pairMarkdownToTag[currentTag])
            {
                stack.Push(new RawToken(_pairMarkdownToTag[currentTag], tagLength, i));
            }
            else
            {
                var opening = stack.Pop();
                if (tagValidator.HasTagContentInside(opening.StartIndex + tagLength, i - 1) &&
                    !tagValidator.HasTagDigitsInside(opening.StartIndex + tagLength, i - 1) &&
                    !tagValidator.IsTagPartsSplittingWord(opening.StartIndex, i))
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

    public List<Token> AddTokenWrappers(List<Token> tokens)
    {
        var tagBuilder = new TagBuilder();
        foreach (var token in tokens)
        {
            var wrappers = tagBuilder.GetWrappers(token.Type.ToString());
            token.SetTokenWrappers(wrappers);
        }
        return tokens;
    }

    public string BuildHTMLString(List<Token> tokens, string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown) || tokens.Count == 0)
            return markdown;
        tokens = tokens
            .OrderBy(t => t.StartIndex)
            .ToList();
        tokens = AddTokenWrappers(tokens);
        var htmlString = new StringBuilder();
        var tagStartPositions = new Dictionary<int, Token>();
        var tagEndPositions = new Dictionary<int, Token>();
        var openedTags = new Stack<Token>();
        foreach (var token in tokens)
        {
            tagStartPositions.Add(token.StartIndex, token);
            tagEndPositions.Add(token.StartIndex + token.TokenLength + token.TokenMarkLength, token);
        }

        for (var i = 0; i < markdown.Length; ++i)
        {
            if (tagStartPositions.ContainsKey(i))
            {
                htmlString.Append(tagStartPositions[i].TokenWrappers.TokenStart);
                openedTags.Push(tagStartPositions[i]);
                i += tagStartPositions[i].TokenMarkLength - 1;
                continue;
            }
            if (tagEndPositions.ContainsKey(i))
            {
                htmlString.Append(tagEndPositions[i].TokenWrappers.TokenEnd);
                openedTags.Pop();
                i += tagEndPositions[i].TokenMarkLength - 1;
                continue;
            }
            htmlString.Append(markdown[i]);
        }

        while (openedTags.Count > 0)
            htmlString.Append(openedTags.Pop().TokenWrappers.TokenEnd);
        return htmlString.ToString();
    }

    private Token CreateToken(RawToken rawToken, int endIndex)
    {
        var startIndex = rawToken.StartIndex;
        return new Token(startIndex, endIndex - rawToken.StartIndex - rawToken.TokenMarkLength, rawToken.Type, rawToken.TokenMarkLength);
    }

    private string ProcessUnpairLineTags(string htmlWithPairTags)
    {
        var lines = htmlWithPairTags.Split('\n');
        var result = new StringBuilder();
        var isListStarted = false;
        foreach (var line in lines)
        {
            var trimmedLine = line.TrimStart();
            if (trimmedLine.StartsWith("# "))
            {
                if (isListStarted)
                {
                    result.Append("</ul>");
                    isListStarted = false;
                }
                result.Append("<h1>").Append(trimmedLine.Substring(2)).Append("</h1>");
            }
            else if (trimmedLine.StartsWith("* "))
            {
                if (!isListStarted)
                {
                    result.Append("<ul>");
                    isListStarted = true;
                }
                result.Append("<li>").Append(trimmedLine.Substring(2)).Append("</li>");
            }
            else
            {
                if (isListStarted)
                {
                    result.Append("</ul>");
                    isListStarted = false;
                }
                result.Append(line);
            }
        }
        if (isListStarted) result.Append("</ul>");
        if (result.Length > 0 && result[^1] == '\n') result.Remove(result.Length - 1, 1);
        return result.ToString();
    }
}