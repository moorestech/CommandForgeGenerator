using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CommandForgeGenerator.Generator.Json;
using CommandForgeGenerator.Generator.Semantic;
using Microsoft.CodeAnalysis;

namespace CommandForgeEditor.Generator.Semantic;

public class CommandSemantics
{
    public CommandsSchema GetCommandSemantics(ImmutableArray<AdditionalText> additionalTexts){
        
        if (additionalTexts.Length == 0)
        {
            throw new Exception("yamlファイルが指定されていません。csprojのAdditionalFilesに追加してください");
        }
        
        var rootNode = GetRootJsonObject();
        return ParseCommandsSchema(rootNode);
        
        #region Internal
        
        JsonObject GetRootJsonObject()
        {
            try
            {
                var yamlText = additionalTexts[0].GetText()!.ToString();
                var jsonText = Yaml.ToJson(yamlText);
                return JsonParser.Parse(JsonTokenizer.GetTokens(jsonText)) as JsonObject;
            }
            catch (Exception e)
            {
                throw new Exception("yamlファイルの形式が正しくありません。" + e.Message);
            }
        }
        
        
        CommandsSchema ParseCommandsSchema(JsonObject root)
        {
            var commandsJson = root["commands"] as JsonArray ?? throw new Exception("commands 配列が見つかりません。");
            
            var commands = new List<CommandSchema>();
            
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
                        "enum" => CommandPropertyType.Enum,
                        "command" => CommandPropertyType.CommandId,
                        _ => throw new Exception($"未知の property type \"{typeStr}\"")
                    };
                    
                    properties.Add(new CommandProperty(mappedType, propName));
                }
                
                commands.Add(new CommandSchema(commandName, properties));
            }
            
            return new CommandsSchema(commands);
        }
        
        #endregion
    }
}

