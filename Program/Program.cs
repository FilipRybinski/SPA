using System.Text;
using System.Text.RegularExpressions;

namespace Program
{
    class Program
    {
        private const string QueryProcessorReady = "Ready";
        private const string Failed = "none";

        public static void Main(string[] args)
        {
            try
            {
                var code = PrepareSimpleCode(args[0]);
                ParseCode(code);
                while (true) RunReadQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static string PrepareSimpleCode(string arg)
        {
            var code = File.ReadAllText(arg);
            code = Regex.Replace(code, @"\r", "");
            return code;
        }

        private static void ParseCode(string code)
        {
            var parser = new Parser.Parser();
            Parser.Parser.CleanData();
            parser.StartParse(code);
            Console.WriteLine(QueryProcessorReady);
        }

        private static void RunReadQuery()
        {
            var variables = Console.ReadLine().Trim();
            var query = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(query))
                query = Console.ReadLine().Trim();

            var results = QueryProcessor.QueryProcessor.ProcessQuery(variables + query, testing: true);
            Console.WriteLine(results.Count == 0 ? Failed : string.Join(", ", results));
        }
    }
}