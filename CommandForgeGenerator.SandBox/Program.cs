using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommandForgeGenerator.SandBox;

internal static class Program
{
    private static void Main(string[] args)
    {
        var blockJson = GetJson("blocks");
    }

    private static JToken GetJson(string name)
    {
        var blockJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestMod", $"{name}.json");
        var blockJson = File.ReadAllText(blockJsonPath);
        return (JToken)JsonConvert.DeserializeObject(blockJson);
    }
}
