using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CommandForgeGenerator.Generator.Json;
using Microsoft.CodeAnalysis;

namespace CommandForgeGenerator.Generator.Semantic;

public class CommandSemanticsLoader
{
    public static CommandsSemantics GetCommandSemantics(ImmutableArray<AdditionalText> additionalTexts){
        
        if (additionalTexts.Length == 0)
        {
            throw new Exception("yamlファイルが指定されていません。csprojのAdditionalFilesに追加してください");
        }
        
        return GetCommandSemantics(additionalTexts[0].GetText()!.ToString());
    }
    
    public static CommandsSemantics GetCommandSemantics(string yamlText)
    {
        var rootNode = GetRootJsonObject();
        var semantics = ParseCommandsSchema(rootNode);
        var reservedCommands = ReservedCommandSemantics.GetReservedCommand();
        
        semantics.Commands.AddRange(reservedCommands);
        
        return semantics;
        
        
        #region Internal
        
        JsonObject GetRootJsonObject()
        {
            try
            {
                var jsonText = Yaml.ToJson(yamlText);
                return JsonParser.Parse(JsonTokenizer.GetTokens(jsonText)) as JsonObject;
            }
            catch (Exception e)
            {
                throw new Exception("yamlファイルの形式が正しくありません。" + e.Message + " " + e.StackTrace);
            }
        }
        
        CommandsSemantics ParseCommandsSchema(JsonObject root)
        {
            var commandsJson = root["commands"] as JsonArray ?? throw new Exception("commands 配列が見つかりません。");
            var generateCodePath = (root["generateCodePath"] as JsonString)?.Literal;

            var commands = new List<CommandSemantics>();
            
            foreach (var commandsJsonNode in commandsJson.Nodes)
            {
                var commandsJsonObject = commandsJsonNode as JsonObject ?? throw new Exception("command 要素がオブジェクトではありません。");
                
                var idNode = commandsJsonObject["id"] as JsonString ?? throw new Exception("command.id が見つかりません。");
                var commandName = idNode.Literal;
                
                // --- properties を走査して CommandProperty のリストを作成 ---
                var propsJsonObj = commandsJsonObject["properties"] as JsonObject
                                   ?? throw new Exception($"command \"{commandName}\" に properties がありません。");
                
                var properties = new List<CommandProperty>();
                
                foreach (var nodeKeyValue in propsJsonObj.Nodes)
                {
                    var propName = nodeKeyValue.Key;
                    var propNode = nodeKeyValue.Value;
                    
                    if (propNode is not JsonObject propObj) throw new Exception($"property \"{propName}\" がオブジェクトではありません。");
                    
                    // type 文字列を取得
                    var typeNode = propObj["type"] as JsonString ?? throw new Exception($"property \"{propName}\" に type がありません。");
                    
                    var typeStr = typeNode.Literal.ToLowerInvariant();
                    var mappedType = typeStr switch
                    {
                        "string" => CommandPropertyType.String,
                        "integer" => CommandPropertyType.Int,
                        "number" => CommandPropertyType.Float,
                        "boolean" => CommandPropertyType.Bool,
                        "enum" => CommandPropertyType.String,
                        "command" => CommandPropertyType.CommandId,
                        "vector2" => CommandPropertyType.Vector2,
                        "vector3" => CommandPropertyType.Vector3,
                        "vector4" => CommandPropertyType.Vector4,
                        "vector2int" => CommandPropertyType.Vector2Int,
                        "vector3int" => CommandPropertyType.Vector3Int,
                        _ => throw new Exception($"未知の property type \"{typeStr}\"")
                    };
                    
                    properties.Add(new CommandProperty(mappedType, propName));
                }
                
                commands.Add(new CommandSemantics(commandName, properties));
            }
            
            return new CommandsSemantics(commands, generateCodePath);
        }
        
        #endregion
    }
}

