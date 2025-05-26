using CommandForgeGenerator.Command;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Assertions;

namespace CommandForgeGenerator.SandBox;

internal static class Program
{
    private static void Main(string[] args)
    {
        var json = GetJson();
        var loader = CommandForgeLoader.LoadCommands(json);
        
        if (loader.Count == 12)
        {
            Console.WriteLine("OK");
        }
        else
        {
            Console.WriteLine("NG " + loader.Count);
        }
    }

    private static JToken GetJson()
    {
        var skitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../", "SampleProject", "skits", "sample_skit.json");
        Console.WriteLine(skitPath);
        var json = File.ReadAllText(skitPath);
        return (JToken)JsonConvert.DeserializeObject(json);
    }
}
