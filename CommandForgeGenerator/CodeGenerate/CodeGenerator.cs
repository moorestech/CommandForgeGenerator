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
            var type = GetTypeCode(property.Type, property.IsRequired);
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
            var type = GetTypeCode(property.Type, property.IsRequired);
            
            if (property.IsRequired)
            {
                // required の場合は従来通り
                if (property.Type is CommandPropertyType.CommandId)
                {
                    properties.AppendLine($"var {property.CodeProperty} = (CommandId)((int)json[\"{property.Name}\"]);");
                }
                else if (property.Type is CommandPropertyType.Vector2)
                {
                    properties.AppendLine($"var {property.CodeProperty}Array = json[\"{property.Name}\"];");
                    properties.AppendLine($"var {property.CodeProperty} = new global::UnityEngine.Vector2((float){property.CodeProperty}Array[0], (float){property.CodeProperty}Array[1]);");
                }
                else if (property.Type is CommandPropertyType.Vector3)
                {
                    properties.AppendLine($"var {property.CodeProperty}Array = json[\"{property.Name}\"];");
                    properties.AppendLine($"var {property.CodeProperty} = new global::UnityEngine.Vector3((float){property.CodeProperty}Array[0], (float){property.CodeProperty}Array[1], (float){property.CodeProperty}Array[2]);");
                }
                else if (property.Type is CommandPropertyType.Vector4)
                {
                    properties.AppendLine($"var {property.CodeProperty}Array = json[\"{property.Name}\"];");
                    properties.AppendLine($"var {property.CodeProperty} = new global::UnityEngine.Vector4((float){property.CodeProperty}Array[0], (float){property.CodeProperty}Array[1], (float){property.CodeProperty}Array[2], (float){property.CodeProperty}Array[3]);");
                }
                else if (property.Type is CommandPropertyType.Vector2Int)
                {
                    properties.AppendLine($"var {property.CodeProperty}Array = json[\"{property.Name}\"];");
                    properties.AppendLine($"var {property.CodeProperty} = new global::UnityEngine.Vector2Int((int){property.CodeProperty}Array[0], (int){property.CodeProperty}Array[1]);");
                }
                else if (property.Type is CommandPropertyType.Vector3Int)
                {
                    properties.AppendLine($"var {property.CodeProperty}Array = json[\"{property.Name}\"];");
                    properties.AppendLine($"var {property.CodeProperty} = new global::UnityEngine.Vector3Int((int){property.CodeProperty}Array[0], (int){property.CodeProperty}Array[1], (int){property.CodeProperty}Array[2]);");
                }
                else
                {
                    properties.AppendLine($"var {property.CodeProperty} = ({type})json[\"{property.Name}\"];");
                }
            }
            else
            {
                // nullable の場合はnullチェックを追加
                properties.AppendLine($"var {property.CodeProperty}Token = json[\"{property.Name}\"];");
                
                if (property.Type is CommandPropertyType.CommandId)
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : (CommandId?)((int){property.CodeProperty}Token);");
                }
                else if (property.Type is CommandPropertyType.Vector2)
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : new global::UnityEngine.Vector2((float){property.CodeProperty}Token[0], (float){property.CodeProperty}Token[1]);");
                }
                else if (property.Type is CommandPropertyType.Vector3)
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : new global::UnityEngine.Vector3((float){property.CodeProperty}Token[0], (float){property.CodeProperty}Token[1], (float){property.CodeProperty}Token[2]);");
                }
                else if (property.Type is CommandPropertyType.Vector4)
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : new global::UnityEngine.Vector4((float){property.CodeProperty}Token[0], (float){property.CodeProperty}Token[1], (float){property.CodeProperty}Token[2], (float){property.CodeProperty}Token[3]);");
                }
                else if (property.Type is CommandPropertyType.Vector2Int)
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : new global::UnityEngine.Vector2Int((int){property.CodeProperty}Token[0], (int){property.CodeProperty}Token[1]);");
                }
                else if (property.Type is CommandPropertyType.Vector3Int)
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : new global::UnityEngine.Vector3Int((int){property.CodeProperty}Token[0], (int){property.CodeProperty}Token[1], (int){property.CodeProperty}Token[2]);");
                }
                else if (property.Type is CommandPropertyType.String)
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : (string?){property.CodeProperty}Token;");
                }
                else
                {
                    properties.AppendLine($"{type} {property.CodeProperty} = {property.CodeProperty}Token?.Type == global::Newtonsoft.Json.Linq.JTokenType.Null ? null : ({type}){property.CodeProperty}Token;");
                }
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
            var type = GetTypeCode(property.Type, property.IsRequired);
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
    
    private static string GetTypeCode(CommandPropertyType type, bool isRequired)
    {
        var baseType = type switch
        {
            CommandPropertyType.String => "string",
            CommandPropertyType.Int => "int",
            CommandPropertyType.Float => "float",
            CommandPropertyType.Bool => "bool",
            CommandPropertyType.CommandId => "CommandId",
            CommandPropertyType.Vector2 => "global::UnityEngine.Vector2",
            CommandPropertyType.Vector3 => "global::UnityEngine.Vector3",
            CommandPropertyType.Vector4 => "global::UnityEngine.Vector4",
            CommandPropertyType.Vector2Int => "global::UnityEngine.Vector2Int",
            CommandPropertyType.Vector3Int => "global::UnityEngine.Vector3Int",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        
        // 値型でrequiredでない場合は nullable にする
        if (!isRequired && type != CommandPropertyType.String)
        {
            return baseType + "?";
        }
        
        // 参照型でも明示的に nullable にする（C# 8.0以降）
        if (!isRequired && type == CommandPropertyType.String)
        {
            return baseType + "?";
        }
        
        return baseType;
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