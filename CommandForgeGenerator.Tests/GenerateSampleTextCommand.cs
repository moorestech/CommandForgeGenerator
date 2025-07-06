#if ENABLE_COMMAND_FORGE_GENERATOR
namespace CommandForgeGenerator.Command
{
    public partial class TextCommand : ICommandForgeCommand
    {
        public const string Type = "text";
        public readonly CommandId CommandId;
        
        public readonly string CharacterId;
        public readonly bool IsOverrideCharacterName;
        public readonly string? OverrideCharacterName;
        public readonly string Body;
        
  
        public static TextCommand Create(int commandId, global::Newtonsoft.Json.Linq.JToken json)
        {
            
            var CharacterId = (string)json["characterId"];
            var IsOverrideCharacterName = (bool)json["isOverrideCharacterName"];
            var OverrideCharacterName = json["overrideCharacterName"] == null ? null : (string)json["overrideCharacterName"];
            var Body = (string)json["body"];
            
            
            return new TextCommand(commandId, CharacterId, IsOverrideCharacterName, OverrideCharacterName, Body);
        }
        
        public TextCommand(int commandId, string CharacterId, bool IsOverrideCharacterName, string? OverrideCharacterName, string Body)
        {
            CommandId = (CommandId)commandId;
            
        this.CharacterId = CharacterId;
        this.IsOverrideCharacterName = IsOverrideCharacterName;
        this.OverrideCharacterName = OverrideCharacterName;
        this.Body = Body;
        
        }
    }
}
#endif