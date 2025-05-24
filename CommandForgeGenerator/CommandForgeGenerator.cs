using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CommandForgeGenerator.Generator.CodeGenerate;
using CommandForgeGenerator.Generator.LoaderGenerate;
using CommandForgeGenerator.Generator.Semantic;
using CommandForgeGenerator.Generator;
using CommandForgeGenerator.Generator.Definitions;
using CommandForgeGenerator.Generator.Json;
using CommandForgeGenerator.Generator.JsonSchema;
using CommandForgeGenerator.Generator.NameResolve;
using CommandForgeGenerator.Generator.Semantic;
using Microsoft.CodeAnalysis;

namespace CommandForgeGenerator.Generator;

[Generator(LanguageNames.CSharp)]
public class CommandForgeGeneratorSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalTextsProvider = context.AdditionalTextsProvider.Collect();
        var provider = context.CompilationProvider.Combine(additionalTextsProvider);
        context.RegisterSourceOutput(provider, Emit);
    }

    private void Emit(SourceProductionContext context, (Compilation compilation, ImmutableArray<AdditionalText> additionalTexts) input)
    {
        try
        {
            var commandsSchema = CommandSemanticsLoader.GetCommandSemantics(input.additionalTexts);
            var codeFiles = CodeGenerator.Generate(commandsSchema);
            
            if (codeFiles.Count == 0) return;
            
            foreach (var codeFile in codeFiles) context.AddSource(codeFile.FileName, codeFile.Code);
        }
        catch (Exception e)
        {
            context.AddSource("Error.g.cs", GetErrorClass(e));
        }
    }

    private (ImmutableArray<SchemaFile> files, SchemaTable schemaTable) ParseAdditionalText(ImmutableArray<AdditionalText> additionalTexts)
    {
        var schemas = new List<SchemaFile>();
        var schemaTable = new SchemaTable();

        foreach (var additionalText in additionalTexts.Where(a => Path.GetExtension(a.Path) == ".yml"))
        {
            var yamlText = additionalText.GetText()!.ToString();
            var jsonText = Yaml.ToJson(yamlText);
            var json = JsonParser.Parse(JsonTokenizer.GetTokens(jsonText));
            var schema = JsonSchemaParser.ParseSchema((json as JsonObject)!, schemaTable);
            schemas.Add(new SchemaFile(additionalText.Path, schema));
        }

        return (schemas.ToImmutableArray(), schemaTable);
    }
    
    private static string GetErrorClass(Exception e)
    {
        var message = e.Message + "\n" + e.StackTrace;
        message = message.Replace("\"", "\\\"");
        return $$$"""
                  public class GenerateError
                  {
                      public string Message = @"
                  {{{message}}}
                  ";
                  }
                  """;
    }
}
