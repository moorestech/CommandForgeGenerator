using System.Collections.Generic;

namespace CommandForgeGenerator.Generator.Semantic;

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
    public readonly List<CommandProperty> Properties;

    public CommandSemantics(string name, List<CommandProperty> properties)
    {
        Name = name;
        Properties = properties;
    }
}

public class CommandProperty{
    public readonly string Name;
    public readonly CommandPropertyType Type;
    
    public string CodeProperty => Name.ToUpper();

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
    CommandId,
}