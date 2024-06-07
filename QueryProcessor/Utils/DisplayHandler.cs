using Parser.Interfaces;
using Utils.Enums;

namespace QueryProcessor.Utils
{
    internal static class DisplayHandler
    {
        private static readonly IPkb Pkb = Parser.Pkb.Instance!;

        public static List<string> Print(Dictionary<string, List<int>> resultToPrint, bool testing)
        {
            var results = new List<string>();


            foreach (var oneVar in resultToPrint)
            {
                var entityType = QueryProcessor.GetVariableEnumType(oneVar.Key);


                switch (entityType)
                {
                    case EntityType.Variable:
                        results.AddRange(DisplayVariables(oneVar.Value));
                        break;
                    case EntityType.Procedure:
                        results.AddRange(DisplayProcedures(oneVar.Value));
                        break;
                    default:
                        results.AddRange(DisplayStatements(oneVar.Value));
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

        private static List<string> DisplayVariables(List<int> indexes)
        {
            var results = new List<string>();
            foreach (var index in indexes)
            {
                results.Add(Pkb.VarTable!.FindVariable(index)?.Identifier);
            }

            return results;
        }

        private static List<string> DisplayProcedures(List<int> indexes)
        {
            var results = new List<string>();
            foreach (var index in indexes)
            {
                results.Add(Pkb.ProcTable!.FindProcedure(index)?.Identifier);
            }


            return results;
        }

        private static List<string> DisplayStatements(List<int> indexes)
        {
            return indexes.Select(index => index.ToString()).ToList();
        }
    }
}