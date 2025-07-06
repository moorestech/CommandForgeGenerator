using System;
using System.IO;
using System.Linq;
using CommandForgeGenerator.Generator.CodeGenerate;
using CommandForgeGenerator.Generator.Json;
using CommandForgeGenerator.Generator.Semantic;
using UnityEngine;
using Xunit;

namespace CommandForgeGenerator.Tests;

public class Test
{
    [Fact]
    public void JsonTokenizerTest()
    {
        var json = """
                   {
                       "hoge": "fuga", 
                       "piyo": [
                           "puyo", 
                           "poyo"
                       ]
                   }
                   """;

        Token[] answer =
        [
            new(TokenType.LBrace, "{"),
            new(TokenType.String, "hoge"),
            new(TokenType.Colon, ":"),
            new(TokenType.String, "fuga"),
            new(TokenType.Comma, ","),
            new(TokenType.String, "piyo"),
            new(TokenType.Colon, ":"),
            new(TokenType.LSquare, "["),
            new(TokenType.String, "puyo"),
            new(TokenType.Comma, ","),
            new(TokenType.String, "poyo"),
            new(TokenType.RSquare, "]"),
            new(TokenType.RBrace, "}")
        ];
        var tokens = JsonTokenizer.GetTokens(json);
        Assert.Equivalent(tokens, answer, true);
    }

    [Fact]
    public void JsonParserTest()
    {
        var json = """
                   {
                       "hoge": "fuga", 
                       "piyo": [
                           "puyo", 
                           "poyo"
                       ]
                   }
                   """;
    }
    
    [Fact]
    public void GenerateTest()
    {
        var yaml = GenerateTestCode.YamlFileStr;
        var commandsSchema = CommandSemanticsLoader.GetCommandSemantics(yaml);
        var codeFiles = CodeGenerator.Generate(commandsSchema);
        
        var file = codeFiles.FirstOrDefault(c => c.FileName == "TextCommand.g.cs").Code;
        
        //File.WriteAllText("/Users/katsumi.sato/Desktop/a/TextCommand.g.cs", file);
        
        
        Assert.Equal(17, codeFiles.Count);
        Assert.Equal(GenerateTestCode.TextCommandStr, codeFiles.FirstOrDefault(c => c.FileName == "TextCommand.g.cs").Code);
    }
}
