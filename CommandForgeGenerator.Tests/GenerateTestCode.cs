using System.IO;

namespace CommandForgeGenerator.Tests;

public class GenerateTestCode
{
    // プロジェクトファイルに存在する GenerateSampleTextCommand.cs を取得する
    public static string TextCommandStr => File.ReadAllText("../../../GenerateSampleTextCommand.cs");
}