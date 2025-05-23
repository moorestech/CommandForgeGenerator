using System;
using System.Collections.Generic;
using System.Text;
using CommandForgeGenerator.Generator.Semantic;

namespace CommandForgeGenerator.Generator.CodeGenerate;

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
        files.Add(new CodeFile("CommandId.g.cs", CommandIdEnumCode()));
        
        foreach (var command in commandsSemantics.Commands)
        {
            var code = Generate(command.ClassName, command);
            files.Add(new CodeFile(command.ClassName + ".g.cs", code));
        }
        
        files.Add(new CodeFile("CommandForgeLoader.g.cs", GenerateLoaderCode(commandsSemantics)));
        
        AddCompilation(files);
        
        return files;
        
        #region Internal
        
        void AddCompilation(List<CodeFile> codeFiles)
        {
            foreach (var file in codeFiles)
            {
                file.Code = $"#if ENABLE_COMMAND_FORGE_GENERATOR\n{file.Code}\n#endif";
            }
        }
        
        #endregion
    }
    
    public static string TextInterfaceCode()
    {
        return """
               namespace CommandForgeGenerator.Command
               {
                   public partial interface ICommandForgeCommand
                   {
                   }
               }
               """;
    }
    
    public static string CommandIdEnumCode()
    {
        return """
               namespace CommandForgeGenerator.Command
               {
                   public enum CommandId { }
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
                          public const string Type = "{{{commandSemantics.Name}}}";
                          public readonly CommandId CommandId;
                          {{{GeneratePropertiesCode(commandSemantics.Properties).Indent(level: 2)}}}
                    
                          public static {{{className}}} Create(int commandId, global::Newtonsoft.Json.Linq.JToken json)
                          {
                              {{{GenerateCreateMethodTempVariables(commandSemantics.Properties).Indent(level: 3)}}}
                              
                              return new {{{className}}}(commandId{{{GenerateUseConstructorCode(commandSemantics.Properties)}}});
                          }
                          
                          public {{{className}}}(int commandId{{{GenerateConstructorPropertiesCode(commandSemantics.Properties)}}})
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
        properties.AppendLine();
        foreach (var property in commandProperties)
        {
            var type = GetTypeCode(property.Type);
            properties.AppendLine($"public readonly {type} {property.CodeProperty};");
        }
        
        return properties.ToString();
    }
    
    private static string GenerateCreateMethodTempVariables(List<CommandProperty> commandProperties)
    {
        var properties = new StringBuilder();
        properties.AppendLine();
        foreach (var property in commandProperties)
        {
            var type = GetTypeCode(property.Type);
            if (property.Type is CommandPropertyType.CommandId)
            {
                properties.AppendLine($"var {property.CodeProperty} = (CommandId)((int)json[\"{property.Name}\"]);");
            }
            else
            {
                properties.AppendLine($"var {property.CodeProperty} = ({type})json[\"{property.Name}\"];");
            }
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
        construct.AppendLine();
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
            CommandPropertyType.Bool => "bool",
            CommandPropertyType.CommandId => "CommandId",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    public static string GenerateLoaderCode(CommandsSemantics commandsSemantics)
    {
        var switchCases = GenerateLoaderSwitchCases(commandsSemantics.Commands);
        
        return $$$"""
                  using System.Collections.Generic;
                  
                  namespace CommandForgeGenerator.Command
                  {
                      public class CommandForgeLoader
                      {
                          public static List<ICommandForgeCommand> LoadCommands(global::Newtonsoft.Json.Linq.JToken json)
                          {
                              var results = new List<ICommandForgeCommand>();
                              var commandsJson = json["commands"];
                              foreach (var commandJson in commandsJson)
                              {
                                  var id = (int)commandJson["id"];
                                  var type = (string)commandJson["type"];
                                  
                                  var command = CreateCommand(id, type, commandJson);
                                  results.Add(command);
                              }
                              return results;
                          }
                          
                          public static ICommandForgeCommand CreateCommand(int id, string type, global::Newtonsoft.Json.Linq.JToken commandJson)
                          {
                              return type switch
                              {
                                  {{{switchCases.Indent(level: 4)}}}
                                  
                                  _ => throw new System.Exception($"Unknown command type: {type}")
                              };
                          }
                      }
                  }
                  """;
    }
    
    private static string GenerateLoaderSwitchCases(List<CommandSemantics> commands)
    {
        var switchCases = new StringBuilder();
        switchCases.AppendLine();
        
        foreach (var command in commands)
        {
            var className = command.ClassName;
            switchCases.AppendLine($"{className}.Type => {className}.Create(id, commandJson),");
        }
        
        return switchCases.ToString();
    }
}