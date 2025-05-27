using System.IO;
using System.Text;
using CommandForgeGenerator.Generator;
using CommandForgeGenerator.Generator.Semantic;
using UnityEditor;
using UnityEngine;

public class CommandTemplateGenerator : EditorWindow
{
    [SerializeField] private string commandsYamlPath = string.Empty;
    [SerializeField] private string outputDirectory = string.Empty;

    [MenuItem("Windows/CommandTemplateGenerator")]
    private static void ShowWindow()
    {
        var window = GetWindow<CommandTemplateGenerator>();
        window.titleContent = new GUIContent("CommandTemplateGenerator");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Commands.yaml", EditorStyles.boldLabel);
        commandsYamlPath = EditorGUILayout.TextField("Path", commandsYamlPath);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Output Directory", EditorStyles.boldLabel);
        outputDirectory = EditorGUILayout.TextField("Path", outputDirectory);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        if (!File.Exists(commandsYamlPath))
        {
            Debug.LogError($"commands.yaml not found: {commandsYamlPath}");
            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var yamlText = File.ReadAllText(commandsYamlPath);
        var semantics = CommandSemanticsLoader.GetCommandSemantics(yamlText);

        foreach (var command in semantics.Commands)
        {
            var className = command.ClassName;
            var code = $$"""
                         namespace CommandForgeGenerator.Command
                         {
                             public partial class {{className}} : ICommandForgeCommand
                             {
                             }
                         }
                         """;

            var filePath = Path.Combine(outputDirectory, className + ".cs");
            File.WriteAllText(filePath, code, Encoding.UTF8);
        }

        AssetDatabase.Refresh();
        Debug.Log("Command templates generated.");
    }
}
