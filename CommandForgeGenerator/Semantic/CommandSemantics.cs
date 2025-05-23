using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CommandForgeGenerator.Generator.Json;
using Microsoft.CodeAnalysis;

namespace CommandForgeGenerator.Generator.Semantic;

public class CommandSemantics
{
    public List<CommandSchema> GetCommandSemantics(ImmutableArray<AdditionalText> additionalTexts){
        
        if (additionalTexts.Length == 0)
        {
            throw new Exception("yamlファイルが指定されていません。csprojのAdditionalFilesに追加してください");
        }
        
        var rootNode = GetRootJsonObject();
        
        
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
            var commandsJson = root["commands"] as JsonArray;
            var commands = new List<CommandSchema>();
            
            foreach (var commandsJsonNode in commandsJson.Nodes)
            {
                var commandsJsonObject = commandsJsonNode as JsonObject;
                
                // TODO
            }
            
            
            return new CommandsSchema(commands);
        }
        
        #endregion
    }
}

