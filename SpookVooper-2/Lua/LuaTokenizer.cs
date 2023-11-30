using System.Text.RegularExpressions;

namespace SV2.Scripting.Tokens;

public enum LuaTokenType
{
    String,
    Comment,
    Variable,
    NewLine,
    Whitespace,
    Equals,
    Dot,
    OpenSquareBracket,
    CloseSquareBracket,
    OpenCurlyBracket,
    CloseCurlyBracket,
    Comma,
    Item,
    WildCard,
    OpenRoundBracket,
    CloseRoundBracket,
    Quote,
    Number
}

public readonly struct Token<TType>
{
    public Token(TType type, string value)
    {
        this.Type = type;
        this.Value = value;
    }

    public TType Type { get; }

    public string Value { get; }
}

public class Tokenizer<TType>
{
    private readonly IList<TokenType> tokenTypes = new List<TokenType>();
    private readonly TType defaultTokenType;

    public Tokenizer(TType defaultTokenType) => this.defaultTokenType = defaultTokenType;

    public Tokenizer<TType> Token(TType type, params string[] matchingRegexs)
    {
        foreach (var matchingRegex in matchingRegexs)
            this.tokenTypes.Add(new TokenType(type, matchingRegex));

        return this;
    }

    public List<Token<TType>> Tokenize(string input)
    {
        IEnumerable<Token<TType>> tokens = new[] { new Token<TType>(this.defaultTokenType, input) };
        foreach (var type in this.tokenTypes)
            tokens = ExtractTokenType(tokens, type);

        return tokens.ToList();
    }

    private IEnumerable<Token<TType>> ExtractTokenType(
        IEnumerable<Token<TType>> tokens,
        TokenType toExtract)
    {
        var tokenType = toExtract.Type;
        var tokenMatcher = new Regex(toExtract.MatchingRegex, RegexOptions.Multiline);
        foreach (var token in tokens)
        {
            if (!token.Type.Equals(this.defaultTokenType))
            {
                yield return token;
                continue;
            }

            var matches = tokenMatcher.Matches(token.Value);
            if (matches.Count == 0)
            {
                yield return token;
                continue;
            }

            var currentIndex = 0;
            foreach (Match match in matches)
            {
                if (currentIndex < match.Index)
                {
                    yield return new Token<TType>(
                        this.defaultTokenType,
                        token.Value.Substring(currentIndex, match.Index - currentIndex));
                }

                yield return new Token<TType>(tokenType, match.Value);
                currentIndex = match.Index + match.Length;
            }

            if (currentIndex < token.Value.Length)
            {
                yield return new Token<TType>(
                    this.defaultTokenType,
                    token.Value.Substring(currentIndex, token.Value.Length - currentIndex));
            }
        }
    }

    private readonly struct TokenType
    {
        public TokenType(TType type, string matchingRegex)
        {
            this.Type = type;
            this.MatchingRegex = matchingRegex;
        }

        public TType Type { get; }

        public string MatchingRegex { get; }
    }
}