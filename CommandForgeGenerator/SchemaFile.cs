using CommandForgeGenerator.Generator.JsonSchema;

namespace CommandForgeGenerator.Generator;

public record SchemaFile(string Path, Schema Schema)
{
    public string Path = Path;
    public Schema Schema = Schema;
}
