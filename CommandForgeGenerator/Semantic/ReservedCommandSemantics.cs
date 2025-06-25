using System.Collections.Generic;

namespace CommandForgeGenerator.Generator.Semantic;

public static class ReservedCommandSemantics
{
    public static List<CommandSemantics> GetReservedCommand()
    {
        var groupStart = new CommandSemantics("group_start", new List<CommandProperty>()
        {
            new(CommandPropertyType.String, "groupName", true),
            new(CommandPropertyType.Bool, "isCollapsed", true),
        });
        var groupEnd = new CommandSemantics("group_end", new List<CommandProperty>());
        return new List<CommandSemantics>(){groupStart, groupEnd};
    }
}