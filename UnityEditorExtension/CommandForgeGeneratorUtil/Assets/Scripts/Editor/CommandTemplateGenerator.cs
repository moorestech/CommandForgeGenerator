using UnityEditor;
using UnityEngine;


public class CommandTemplateGenerator : EditorWindow
{
    [MenuItem("Windows/CommandTemplateGenerator")]
    private static void ShowWindow()
    {
        var window = GetWindow<CommandTemplateGenerator>();
        window.titleContent = new GUIContent("CommandTemplateGenerator");
        window.Show();
    }

    private void CreateGUI()
    {
            
    }
}