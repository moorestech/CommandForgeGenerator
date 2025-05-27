using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CommandForgeGeneratorUtil
{
    public class CommandTemplateGenerator : EditorWindow
    {
        private const string CommandsYamlPathKey = "CommandTemplateGenerator_CommandsYamlPath";
        private const string OutputDirectoryKey = "CommandTemplateGenerator_OutputDirectory";

        [SerializeField] private string commandsYamlPath = string.Empty;
        [SerializeField] private string outputDirectory = string.Empty;

        [MenuItem("Window/CommandTemplateGenerator")]
        private static void ShowWindow()
        {
            var window = GetWindow<CommandTemplateGenerator>();
            window.titleContent = new GUIContent("CommandTemplateGenerator");
            window.Show();
        }

        private void OnEnable()
        {
            // Load saved preferences
            commandsYamlPath = EditorPrefs.GetString(CommandsYamlPathKey, string.Empty);
            outputDirectory = EditorPrefs.GetString(OutputDirectoryKey, string.Empty);
        }

        private void OnDisable()
        {
            // Save preferences when the window is closed
            EditorPrefs.SetString(CommandsYamlPathKey, commandsYamlPath);
            EditorPrefs.SetString(OutputDirectoryKey, outputDirectory);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Commands.yaml", EditorStyles.boldLabel);
            var newYamlPath = EditorGUILayout.TextField("Path", commandsYamlPath);
            if (newYamlPath != commandsYamlPath)
            {
                commandsYamlPath = newYamlPath;
                EditorPrefs.SetString(CommandsYamlPathKey, commandsYamlPath);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Output Directory", EditorStyles.boldLabel);
            var newOutputPath = EditorGUILayout.TextField("Path", outputDirectory);
            if (newOutputPath != outputDirectory)
            {
                outputDirectory = newOutputPath;
                EditorPrefs.SetString(OutputDirectoryKey, outputDirectory);
            }

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
                var sb = new StringBuilder()
                    .AppendLine("namespace CommandForgeGenerator.Command")
                    .AppendLine("{")
                    .AppendLine($"    public partial class {className}")
                    .AppendLine("    {")
                    .AppendLine("    }")
                    .AppendLine("}");
                var code = sb.ToString();

                var filePath = Path.Combine(outputDirectory, className + ".cs");
                File.WriteAllText(filePath, code, Encoding.UTF8);
            }

            AssetDatabase.Refresh();
            Debug.Log("Command templates generated.");
        }
    }
}
