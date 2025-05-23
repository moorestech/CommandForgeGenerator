using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandForgeEditor.Generator.LoaderGenerate;
using CommandForgeEditor.Generator.Semantic;
using CommandForgeGenerator.Generator.Definitions;
using CommandForgeGenerator.Generator.NameResolve;
using Type = CommandForgeGenerator.Generator.Definitions.Type;

namespace CommandForgeEditor.Generator.CodeGenerate;

public record CodeFile(string FileName, string Code)
{
    public string Code = Code;
    public string FileName = FileName;
}

public static class CodeGenerator
{
    public static List<CodeFile> Generate(CommandsSemantics commandsSemantics)
    {
        var files = new List<CodeFile>();
        
        files.Add(new CodeFile("ICommandForgeCommand.g.cs", TextInterfaceCode()));
        
        foreach (var command in commandsSemantics.Commands)
        {
            var className = command.Name + "Command";
            var code = Generate(className, command);
            files.Add(new CodeFile(className + ".g.cs", code));
        }
        
        return files;
    }
    
    public static string TextInterfaceCode()
    {
        return $$$"""
                  namespace CommandForgeGenerator.Command
                  {
                      public partial interface ICommandForgeCommand
                      {
                      }
                  }
                  """;
    }
    
    public static string Generate(string className, CommandSemantics commandSemantics)
    {
        return $$$"""
                    namespace CommandForgeGenerator.Command
                    {
                        public partial class {{{className}}} : ICommandForgeCommand
                        {
                            public readonly CommandId CommandId;
                            {{{GeneratePropertiesCode(commandSemantics.Properties).Indent(level: 2)}}}
                            
                            public static {{{className}}} Create(int commandId, global::Newtonsoft.Json.Linq.JToken json)
                            {
                                {{{GenerateCreateMethodTempVariables(commandSemantics.Properties).Indent(level: 3)}}}
                                
                                return new {{{className}}}(commandId, {{{GenerateUseConstructorCode(commandSemantics.Properties)}}});
                            }
                            
                            public {{{className}}}(int commandId, {{{GenerateConstructorPropertiesCode(commandSemantics.Properties)}}})
                            {
                                CommandId = (CommandId)commandId;
                                {{{GenerateConstructSetPropertiesCode(commandSemantics.Properties).Indent(level: 2)}}}
                            }
                        }
                    }
                  """;
    }
    
    private static string GeneratePropertiesCode(List<CommandProperty> commandProperties)
    {
        var properties = new StringBuilder();
        properties.AppendLine("\n");
        foreach (var property in commandProperties)
        {
            var type = GetTypeCode(property.Type);
            properties.AppendLine($"public readonly {type} {property.CodeProperty} = ({type})json[\"{property.Name}\"]");
        }
        
        return properties.ToString();
    }
    
    private static string GenerateCreateMethodTempVariables(List<CommandProperty> commandProperties)
    {
        var properties = new StringBuilder();
        properties.AppendLine("\n");
        foreach (var property in commandProperties)
        {
            var type = GetTypeCode(property.Type);
            properties.AppendLine($"var {property.CodeProperty} = ({type})json[\"{property.Name}\"]");
        }
        
        return properties.ToString();
    }
    private static string GenerateUseConstructorCode(List<CommandProperty> commandProperties)
    {
        var useConstruct = new StringBuilder();
        foreach (var property in commandProperties)
        {
            useConstruct.Append($", {property.CodeProperty}");
        }
        
        return useConstruct.ToString();
    }
    
    public static string GenerateConstructorPropertiesCode(List<CommandProperty> commandProperties)
    {
        var properties = new StringBuilder();
        foreach (var property in commandProperties)
        {
            var type = GetTypeCode(property.Type);
            properties.Append($", {type} {property.CodeProperty}");
            
        }
        
        return properties.ToString();
    }
    
    private static string GenerateConstructSetPropertiesCode(List<CommandProperty> commandProperties)
    {
        var construct = new StringBuilder();
        construct.AppendLine("\n");
        foreach (var property in commandProperties)
        {
            construct.AppendLine($"this.{property.CodeProperty} = {property.CodeProperty};");
        }
        
        return construct.ToString();
    }
    
    
    private static string GetTypeCode(CommandPropertyType type)
    {
        return type switch
        {
            CommandPropertyType.String => "string",
            CommandPropertyType.Int => "int",
            CommandPropertyType.Float => "float",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
