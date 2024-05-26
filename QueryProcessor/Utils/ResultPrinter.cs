using Parser.AST.Enums;
using Parser.Tables;

namespace QueryProcessor.Utils
{
    public static class ResultPrinter
    {
        public static List<string> Print(Dictionary<string, List<int>> resultToPrint, bool testing)
        {
            List<string> results = new List<string>();


            foreach (KeyValuePair<string, List<int>> oneVar in resultToPrint)
            {
                EntityType entityType = QueryProcessor.GetVariableEnumType(oneVar.Key);


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
            List<string> results = new List<string>();
            foreach (int index in indexes)
            {
                results.Add(ViariableTable.Instance.GetVar(index).Identifier);
            }

            /*if (indexes.Count != 0)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    results.Add(ViariableTable.Instance.GetVar(indexes[i]).Identifier);
                }
            }*/

            return results;
        }

        private static List<string> PrintProcedures(List<int> indexes)
        {
            List<string> results = new List<string>();
            foreach (int index in indexes)
            {
                results.Add(ProcedureTable.Instance.GetProcedure(index).Identifier);

            }

            /*
                if (indexes.Count != 0)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    results.Add(ProcedureTable.Instance.GetProcedure(indexes[i]).Identifier);
                }
            }*/

            return results;
        }

        private static List<string> PrintStatements(List<int> indexes)
        {
            List<string> results = new List<string>();
            foreach (int index in indexes)
            {
                results.Add(index.ToString());
            }

            /*
                if (indexes.Count != 0)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    results.Add(indexes[i].ToString());

                }
            }*/
            return results;
        }
    }
}
