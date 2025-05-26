namespace CommandForgeGenerator.Generator;

public static class StringExtension
{
    public static string Indent(this string code, bool firstLine = false, int level = 1)
    {
        var indent = new string(' ', 4 * level);
        return firstLine ? indent : "" + code.Replace("\n", $"\n{indent}");
    }
    
    public static string CommandNameToClassName(this string commandName)
    {
        return commandName.ToUpper(0) + "Command";
    }
    
    /// <summary>
    /// 指定した n 番目の文字を大文字に変換します。
    /// </summary>
    public static string ToUpper(this string self, int no = 0)
    {
        if (no > self.Length)
        {
            return self;
        }
        
        var _array = self.ToCharArray();
        var up = char.ToUpper(_array[no]);
        _array[no] = up;
        return new string(_array);
    }
}