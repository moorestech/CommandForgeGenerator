using System.Collections.Generic;
using CommandForgeGenerator.Generator.JsonSchema;

namespace CommandForgeGenerator.Generator;

public class DefineInterface(string interfaceName, Dictionary<string, IDefineInterfacePropertySchema> properties, string[] implementationInterfaces)
{
    public string[] ImplementationInterfaces = implementationInterfaces;
    public string InterfaceName = interfaceName;
    public Dictionary<string, IDefineInterfacePropertySchema> Properties = properties;
}
