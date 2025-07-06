#if ENABLE_COMMAND_FORGE_GENERATOR
namespace CommandForgeGenerator.Command
{
    public partial class TextCommand : ICommandForgeCommand
    {
        public const string Type = "text";
        public readonly CommandId CommandId;
        
        public readonly string Character;
        public readonly string? Body;
        public readonly string? VoiceId;
        
  
        public static TextCommand Create(int commandId, global::Newtonsoft.Json.Linq.JToken json)
        {
            
            var Character = (string)json["character"];
            var Body = json["body"] == null ? null : (string)json["body"];
            var VoiceId = json["voiceId"] == null ? null : (string)json["voiceId"];
            
            
            return new TextCommand(commandId, Character, Body, VoiceId);
        }
        
        public TextCommand(int commandId, string Character, string? Body, string? VoiceId)
        {
            CommandId = (CommandId)commandId;
            
        this.Character = Character;
        this.Body = Body;
        this.VoiceId = VoiceId;
        
        }
    }
}
#endif