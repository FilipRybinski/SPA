using System.Text.RegularExpressions;

namespace SPATests.Utils;

public static class Utils
{
    public static string PrepareResults(IReadOnlyCollection<string> results)
    {
        if (results.Count == 0)
        {
            return "none";
        }

        return string.Join(", ", results);
    }

    public static string PrepareSimpleCode(string filePath)
    {
        var code = File.ReadAllText(filePath);
        code = Regex.Replace(code, @"\r", "");
        return code;
    }

    public static void ParseCode(string code)
    {
        var parser = new Parser.Parser();
        Parser.Parser.CleanData();
        parser.StartDecoding(code);
    }
}