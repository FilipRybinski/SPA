using System.Text.RegularExpressions;

namespace  Program
{
    class Program
    {
        private const string QueryProcessorReady = "Ready";
        private const string Failed = "none";
        public static void Main(string[] args)
        {
            
            try
            {           
                string SimpleCode = File.ReadAllText(args[0]); //pobieranie nazwy pliku z kodem simple
                SimpleCode = Regex.Replace(SimpleCode, @"\r", ""); // usunięcie nowej lini
                Parser.Parser Parser = new Parser.Parser();
                Parser.CleanData();
                Parser.StartParse(SimpleCode);
                Console.WriteLine(QueryProcessorReady); //informacja dla PipeTestera, że może wprowadzać zapytania PQL

                string variables;
                string query;
                string PQL;
                List<string> results;

                while(true){
                    variables = Console.ReadLine();
                    query = Console.ReadLine();
                    PQL = variables + query;
                    results = QueryProcessor.QueryProcessor.ProcessQuery(PQL, testing: true);
                    if(results.Count == 0)
                        Console.WriteLine(Failed);
                    else
                    {
                        Console.WriteLine(string.Join(", ", results));
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error");
            } 
        }
    }
}

