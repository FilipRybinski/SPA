using Parser.Interfaces;
using Utils.Enums;

namespace QueryProcessor.Utils
{
    internal static class ResultPrinter
    {
        private static readonly IPkb Pkb= Parser.Pkb.Instance!;
        public static List<string> Print(Dictionary<string, List<int>> resultToPrint, bool testing)
        {
            var results = new List<string>();


            foreach (var oneVar in resultToPrint)
            {
                var entityType = QueryProcessor.GetVariableEnumType(oneVar.Key);


                switch (entityType)
                {
                    case EntityType.Variable:
                        results.AddRange(PrintVariables(oneVar.Value));
                        break;
                    case EntityType.Procedure:
                        results.AddRange(PrintProcedures(oneVar.Value));
                        break;
                    default:
                        results.AddRange(PrintStatements(oneVar.Value));
                        break;
                }

            }
            if (!testing)
                if (results.Count > 0)
                    Console.WriteLine(string.Join(", ", results));
                else
                    Console.WriteLine("none");

            return results;

        }

        private static int PrintCodeLine(int number, bool lastResult)
        {
            if (lastResult) Console.Write("{0}", number);
            else Console.Write("{0},", number);
            return number;
        }

        private static List<string> PrintVariables(List<int> indexes)
        {
            var results = new List<string>();
            foreach (var index in indexes)
            {
                results.Add(Pkb.VarTable!.GetVar(index).Identifier);
            }

            return results;
        }

        private static List<string> PrintProcedures(List<int> indexes)
        {
            var results = new List<string>();
            foreach (var index in indexes)
            {
                results.Add(Pkb.ProcTable!.GetProcedure(index).Identifier);

            }
            

            return results;
        }

        private static List<string> PrintStatements(List<int> indexes)
        {
            var results = new List<string>();
            foreach (var index in indexes)
            {
                results.Add(index.ToString());
            }
            
            return results;
        }
    }
}
