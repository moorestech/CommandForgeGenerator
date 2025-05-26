using System.IO;
using CommandForgeGenerator.Generator.Semantic;

namespace CommandForgeGenerator.Generator.CodeGenerate;

public static class TemplateClassGenerator
{
    public static void Generate(string baseDirectory, CommandsSemantics semantics)
    {
        if (string.IsNullOrEmpty(semantics.GenerateCodePath)) return;

        var outputDir = Path.Combine(baseDirectory, semantics.GenerateCodePath);
        Directory.CreateDirectory(outputDir);

        foreach (var command in semantics.Commands)
        {
            var filePath = Path.Combine(outputDir, command.ClassName + ".cs");
            if (File.Exists(filePath)) continue;
            var code = $$"""
namespace CommandForgeGenerator.Command
{{
    public partial class {command.ClassName}
    {{
    }}
}}
""";
            File.WriteAllText(filePath, code);
        }
    }
}

