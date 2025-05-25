using System;
using System.Collections.Immutable;
using CommandForgeGenerator.Generator.CodeGenerate;
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
