using System.Collections.Generic;
using CommandForgeGeneratorUtil;

namespace CommandForgeGeneratorUtil
{
    public class CommandsSemantics
    {
        public readonly List<CommandSemantics> Commands;

        public CommandsSemantics(List<CommandSemantics> commands)
        {
            Commands = commands;
        }
    }

    public class CommandSemantics
    {
        public readonly string Name;
        public string ClassName => Name.CommandNameToClassName();
        public readonly List<CommandProperty> Properties;

        public CommandSemantics(string name, List<CommandProperty> properties)
        {
            Name = name;
            Properties = properties;
        }
    }

    public class CommandProperty
    {
        public readonly string Name;
        public readonly CommandPropertyType Type;

        public string CodeProperty => Name.ToUpper(0);

        public CommandProperty(CommandPropertyType type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public enum CommandPropertyType
    {
        String,
        Int,
        Float,
        Bool,
        CommandId,
        Vector2,
        Vector3,
        Vector4,
        Vector2Int,
        Vector3Int,
    }
}