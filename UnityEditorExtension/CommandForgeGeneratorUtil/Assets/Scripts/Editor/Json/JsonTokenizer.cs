using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandForgeGeneratorUtil
{
    public readonly struct Token : IEquatable<Token>
    {
        public TokenType Type { get; }
        public string Literal { get; }

        public Token(TokenType type, string literal)
        {
            Type = type;
            Literal = literal;
        }

        // Deconstruct サポート（record と同じ使い勝手）
        public void Deconstruct(out TokenType type, out string literal)
        {
            type = Type;
            literal = Literal;
        }

        // 等価比較（record が自動生成する内容に相当）
        public bool Equals(Token other) =>
            Type == other.Type && Literal == other.Literal;

        public override bool Equals(object? obj) =>
            obj is Token other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Type, Literal);

        // デバッグ用表示
        public override string ToString() =>
            $"Token {{ Type = {Type}, Literal = \"{Literal}\" }}";
    }


    public enum TokenType
    {
        String,
        Colon,
        RBrace,
        LBrace,
        RSquare,
        LSquare,
        Comma,

        True,
        False,

        Minus,
        Int,
        Number,

        Illegal
    }

    public static class JsonTokenizer
    {
        public static Token[] GetTokens(string json)
        {
            var tokens = new List<Token>();

            var iterator = new Iterator(json);

            while (json.Length > iterator.CurrentIndex)
            {
                // skip whitespace
                while (new[] { ' ', '\t', '\n', '\r' }.Contains(iterator.CurrentChar))
                    iterator.CurrentIndex++;

                // end
                if (iterator.CurrentChar == '\0') break;

                // tokenize
                switch (iterator.CurrentChar)
                {
                    case ':':
                        tokens.Add(new Token(TokenType.Colon, ":"));
                        break;
                    case '{':
                        tokens.Add(new Token(TokenType.LBrace, "{"));
                        break;
                    case '}':
                        tokens.Add(new Token(TokenType.RBrace, "}"));
                        break;
                    case '[':
                        tokens.Add(new Token(TokenType.LSquare, "["));
                        break;
                    case ']':
                        tokens.Add(new Token(TokenType.RSquare, "]"));
                        break;
                    case ',':
                        tokens.Add(new Token(TokenType.Comma, ","));
                        break;
                    case '"':
                        var literal = "";

                        while (iterator.NextChar != '"')
                        {
                            iterator.CurrentIndex++;
                            if (iterator.CurrentChar == '\\')
                                throw new Exception("not implemented \\");
                            literal += iterator.CurrentChar;
                        }

                        iterator.CurrentIndex++;

                        tokens.Add(new Token(TokenType.String, literal));
                        break;
                    case '/':
                        if (iterator.NextChar != '/') throw new Exception("not implemented");

                        // skip comment
                        iterator.CurrentIndex++;
                        while (iterator.NextChar != '\n') iterator.CurrentIndex++;

                        break;
                    case '-':
                        tokens.Add(new Token(TokenType.Minus, "-"));
                        break;
                    case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                        var number = iterator.CurrentChar.ToString();
                        var isNumber = false;

                        while (iterator.NextChar is '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' || (!isNumber && iterator.NextChar is '.'))
                        {
                            if (iterator.NextChar is '.')
                                isNumber = true;

                            iterator.CurrentIndex++;
                            number += iterator.CurrentChar;
                        }

                        tokens.Add(new Token(isNumber ? TokenType.Number : TokenType.Int, number));
                        break;
                    default:
                        var identifier = "" + iterator.CurrentChar;
                        while (char.IsLetter(iterator.NextChar))
                        {
                            iterator.CurrentIndex++;
                            identifier += iterator.CurrentChar;
                        }

                        switch (identifier)
                        {
                            case "true":
                                tokens.Add(new Token(TokenType.True, "true"));
                                break;
                            case "false":
                                tokens.Add(new Token(TokenType.False, "false"));
                                break;
                            default:
                                throw new Exception($"not implemented: \"{identifier}\"");
                        }

                        break;
                }

                iterator.CurrentIndex++;
            }

            return tokens.ToArray();
        }


        // C# 9.0 互換版
        private struct Iterator
        {
            private readonly string _sourceText;   // もとの sourceText
            public int CurrentIndex { get; set; }  // 走査位置（既定値 0）

            public Iterator(string sourceText)
            {
                _sourceText   = sourceText ?? string.Empty;
                CurrentIndex  = 0;                 // 明示しておくとわかりやすい
            }

            public char CurrentChar =>
                _sourceText.Length > CurrentIndex ? _sourceText[CurrentIndex] : '\0';

            public char NextChar =>
                _sourceText.Length > CurrentIndex + 1 ? _sourceText[CurrentIndex + 1] : '\0';
        }

    }
}