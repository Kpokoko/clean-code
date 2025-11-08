using FluentAssertions;
using NUnit.Framework;

namespace Markdown;

[TestFixture]
public class MarkdownTests
{
    private static Object[] _tokenizeTestCases = 
    {
        new Object[]
        {
            "_текст текст текст_",
            new List<Token>
            {
                new Token(0, 17, TokenType.Italic, null)
            },
        },
        new Object[]
        {
            "__текст текст текст__",
            new List<Token>
            {
                new Token(0, 17, TokenType.Bold, null)
            },
        },
        new Object[]
        {
            "#текст текст текст",
            new List<Token>
            {
                new Token(0, 17, TokenType.Title, null)
            },
        },
    };
    
    [TestCaseSource(nameof(_tokenizeTestCases))]
    public void TokenizeBasicTags(string rawText, List<Token> expectedTokens)
    {
        var parser = new MarkdownParser();
        parser.TokenizeText(rawText).Should().BeEquivalentTo(expectedTokens);
    }
    
    private static Object[] _wrapTokensTestCases = 
    {
        new Object[]
        {
            new List<Token>
            {
                new Token(0, 17, TokenType.Italic, null)
            },
            "<em>текст текст текст</em>",
        },
        new Object[]
        {
            new List<Token>
            {
                new Token(0, 17, TokenType.Bold, null)
            },
            "<strong>текст текст текст</strong>",
        },
        new Object[]
        {
            new List<Token>
            {
                new Token(0, 17, TokenType.Title, null)
            },
            "<h1>текст текст текст</h1>",
        },
    };
    
    [TestCaseSource(nameof(_tokenizeTestCases))]
    public void WrapBasicTags(List<Token> tokens, string expectedText)
    {
        var parser = new MarkdownParser();
        parser.WrapTokensWithTags(tokens).Should().BeEquivalentTo(tokens);
    }
    
    [TestCase("_текст текст текст_", "<em>текст текст текст</em>")]
    [TestCase("__текст текст текст__", "<strong>текст текст текст</strong>")]
    [TestCase("#текст текст текст", "<h1>текст текст текст</h1>")]
    [TestCase("текст текст текст", "текст текст текст")]
    public void ParseBasicTags(string rawText, string expectedText)
    {
        var parser = new MarkdownParser();
        parser.Render(rawText).Should().BeEquivalentTo(expectedText);
    }

    [TestCase(@"текст \текст тек\ст", @"текст \текст тек\ст")]
    [TestCase(@"\_текст текст \_текст", @"\_текст текст \_текст")]
    [TestCase(@"\\_текст текст\\_ текст", @"\\_текст текст\\_ текст")]
    public void TestShieldingParsing(string rawText, string expectedText)
    {
        var parser = new MarkdownParser();
        parser.Render(rawText).Should().BeEquivalentTo(expectedText);
    }

    [TestCase("__текст _текст_ текст__", "<strong>текст <em>текст<em/> текст</strong>")]
    [TestCase("текст текст_12_3", "текст текст_12_3")]
    [TestCase("_тек_ст те_кс_т тек_ст_", "<em>тек</em>ст те<em>кс</em>т тек<em>ст</em>\"")]
    [TestCase("текст т_екст тек_ст", "текст т_екст тек_ст")]
    [TestCase("текст __текст_ текст", "текст __текст_ текст")]
    [TestCase("текст_ текст_ текст", "текст_ текст_ текст")]
    [TestCase("текст _текст _текст", "текст _текст _текст")]
    [TestCase("__текст_ текст__ _текст", "__текст_ текст__ _текст")]
    [TestCase("____текст текст текст", "____текст текст текст")]
    [TestCase("#__текст _текст_ текст__", "<h1><strong>текст <em>текст</em> текст</strong>")]
    public void TestTagsInterations(string rawText, string expectedText)
    {
        var parser = new MarkdownParser();
        parser.Render(rawText).Should().BeEquivalentTo(expectedText);
    }
}