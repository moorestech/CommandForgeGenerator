using System;
using System.Collections.Generic;

namespace CommandForgeGeneratorUtil
{
 // ──────────────────────────
    // インターフェース
    // ──────────────────────────
    public interface IJsonNode
    {
        IJsonNode? Parent { get; }
        string?    PropertyName { get; }
    }

    // ──────────────────────────
    // JsonObject  (オブジェクトノード)
    // ──────────────────────────
    public record JsonObject : IJsonNode
    {
        public Dictionary<string, IJsonNode> Nodes { get; }
        public IJsonNode? Parent            { get; }
        public string?    PropertyName      { get; }

        public JsonObject(
            Dictionary<string, IJsonNode> nodes,
            IJsonNode? parent,
            string? propertyName)
        {
            Nodes        = nodes ?? new Dictionary<string, IJsonNode>();
            Parent       = parent;
            PropertyName = propertyName;
        }

        public IJsonNode? this[string key] =>
            Nodes.TryGetValue(key, out var value) ? value : null;
    }

    // ──────────────────────────
    // JsonArray  (配列ノード)
    // ──────────────────────────
    public record JsonArray : IJsonNode
    {
        public IJsonNode[] Nodes     { get; set; }
        public IJsonNode?  Parent    { get; }
        public string?     PropertyName { get; }

        public JsonArray(
            IJsonNode[] nodes,
            IJsonNode? parent,
            string? propertyName)
        {
            Nodes        = nodes ?? Array.Empty<IJsonNode>();
            Parent       = parent;
            PropertyName = propertyName;
        }

        public IJsonNode? this[int index] =>
            index >= 0 && index < Nodes.Length ? Nodes[index] : null;
    }

    // ──────────────────────────
    // JsonString  (文字列リテラル)
    // ──────────────────────────
    public record JsonString : IJsonNode
    {
        public string      Literal       { get; }
        public IJsonNode?  Parent        { get; }
        public string?     PropertyName  { get; }

        public JsonString(
            string literal,
            IJsonNode? parent,
            string? propertyName)
        {
            Literal       = literal;
            Parent        = parent;
            PropertyName  = propertyName;
        }
    }

    // ──────────────────────────
    // JsonBoolean  (真偽値リテラル)
    // ──────────────────────────
    public record JsonBoolean : IJsonNode
    {
        public bool        Literal      { get; }
        public IJsonNode?  Parent       { get; }
        public string?     PropertyName { get; }

        public JsonBoolean(
            bool literal,
            IJsonNode? parent,
            string? propertyName)
        {
            Literal       = literal;
            Parent        = parent;
            PropertyName  = propertyName;
        }
    }

    // ──────────────────────────
    // JsonNumber  (浮動小数点数リテラル)
    // ──────────────────────────
    public record JsonNumber : IJsonNode
    {
        public double      Literal      { get; }
        public IJsonNode?  Parent       { get; }
        public string?     PropertyName { get; }

        public JsonNumber(
            double literal,
            IJsonNode? parent,
            string? propertyName)
        {
            Literal       = literal;
            Parent        = parent;
            PropertyName  = propertyName;
        }
    }

    // ──────────────────────────
    // JsonInt  (整数リテラル)
    // ──────────────────────────
    public record JsonInt : IJsonNode
    {
        public long        Literal      { get; }
        public IJsonNode?  Parent       { get; }
        public string?     PropertyName { get; }

        public JsonInt(
            long literal,
            IJsonNode? parent,
            string? propertyName)
        {
            Literal       = literal;
            Parent        = parent;
            PropertyName  = propertyName;
        }
    }

    // ──────────────────────────
    // 走査用イテレータ (値型)
    // ──────────────────────────
    public struct Iterator
    {
        private readonly Token[] _tokens;
        public int CurrentIndex { get; set; }

        public Iterator(Token[] tokens)
        {
            _tokens      = tokens ?? Array.Empty<Token>();
            CurrentIndex = 0;
        }

        public Token CurrentToken =>
            _tokens.Length > CurrentIndex
                ? _tokens[CurrentIndex]
                : new Token(TokenType.Illegal, string.Empty);

        public Token NextToken =>
            _tokens.Length > CurrentIndex + 1
                ? _tokens[CurrentIndex + 1]
                : new Token(TokenType.Illegal, string.Empty);
    }

    public static class JsonParser
    {
        public static IJsonNode Parse(Token[] tokens)
        {
            var iterator = new Iterator(tokens);
            return Parse(ref iterator, null, null);
        }

        private static IJsonNode Parse(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            return iterator.CurrentToken.Type switch
            {
                TokenType.String => ParseString(ref iterator, parent, name),
                TokenType.LBrace => ParseObject(ref iterator, parent, name),
                TokenType.LSquare => ParseArray(ref iterator, parent, name),
                TokenType.True or TokenType.False => ParseBoolean(ref iterator, parent, name),
                TokenType.Number => ParseNumber(ref iterator, parent, name),
                TokenType.Int => ParseInt(ref iterator, parent, name),
                _ => throw new Exception($"Unexpected token: {iterator.CurrentToken.Type} \"{iterator.CurrentToken.Literal}\"")
            };
        }

        private static IJsonNode ParseMinus(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            iterator.CurrentIndex++; // skip '-'
            switch (iterator.CurrentToken.Type)
            {
                case TokenType.Int:
                    var intValue = ParseInt(ref iterator, parent, name);
                    return new JsonInt(-intValue.Literal, parent, name);
                case TokenType.Number:
                    var numberValue = ParseNumber(ref iterator, parent, name);
                    return new JsonNumber(-numberValue.Literal, parent, name);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static JsonInt ParseInt(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            var value = iterator.CurrentToken.Literal;
            iterator.CurrentIndex++; // skip int
            return new JsonInt(long.Parse(value), parent, name);
        }

        private static JsonNumber ParseNumber(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            var value = iterator.CurrentToken.Literal;
            iterator.CurrentIndex++; // skip number
            return new JsonNumber(double.Parse(value), parent, name);
        }

        private static JsonBoolean ParseBoolean(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            var value = iterator.CurrentToken.Literal;
            iterator.CurrentIndex++; // skip boolean
            return new JsonBoolean(value == "true", parent, name);
        }

        private static JsonString ParseString(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            var value = iterator.CurrentToken.Literal;
            iterator.CurrentIndex++; // skip string
            return new JsonString(value, parent, name);
        }

        private static JsonArray ParseArray(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            var nodes = new List<IJsonNode>();
            iterator.CurrentIndex++; // skip '['

            var jsonNode = new JsonArray(new List<IJsonNode>().ToArray(), parent, name);

            while (iterator.CurrentToken.Type != TokenType.RSquare)
            {
                nodes.Add(Parse(ref iterator, jsonNode, null));
                if (iterator.CurrentToken.Type == TokenType.RSquare) break;
                iterator.CurrentIndex++; // skip ','
            }

            iterator.CurrentIndex++; // skip ']'

            jsonNode.Nodes = nodes.ToArray();

            return jsonNode;
        }

        private static JsonObject ParseObject(ref Iterator iterator, IJsonNode? parent, string? name)
        {
            var nodes = new Dictionary<string, IJsonNode>();
            iterator.CurrentIndex++; // skip '{'

            var jsonNode = new JsonObject(nodes, parent, name);

            while (iterator.CurrentToken.Type != TokenType.RBrace)
            {
                var key = iterator.CurrentToken.Literal;
                iterator.CurrentIndex++; // skip string
                iterator.CurrentIndex++; // skip ':'
                var value = Parse(ref iterator, jsonNode, key);
                nodes.Add(key, value);
                if (iterator.CurrentToken.Type == TokenType.RBrace) break;
                iterator.CurrentIndex++; // skip ','
            }

            iterator.CurrentIndex++; // skip '}'

            return jsonNode;
        }
    }
}