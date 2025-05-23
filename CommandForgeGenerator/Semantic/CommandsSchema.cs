using System.Collections.Generic;

namespace CommandForgeEditor.Generator.Semantic;

public class CommandsSchema
{
    public readonly List<CommandSchema> Commands;

    public CommandsSchema(List<CommandSchema> commands)
    {
        Commands = commands;
    }
}

public class CommandSchema
{
    public readonly string Name;
    public readonly List<CommandProperty> Properties;

    public CommandSchema(string name, List<CommandProperty> properties)
    {
        Name = name;
        Properties = properties;
    }
}

public class CommandProperty{
    public readonly string Name;
    public readonly CommandPropertyType Type;

    public CommandProperty(CommandPropertyType type, string name)
    {
        Type = type;
        Name = name;
    }

}

public enum CommandPropertyType{
    String,
    Int,
    Float,
    Bool,
    Enum,
    CommandId,
}