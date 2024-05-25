using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryProcessor.Utils
{
    public static class ResultPrinter
    {
        public static List<string> Print(Dictionary<string, List<int>> resultToPrint, bool testing)
        {
            List<string> results = new List<string>();


            foreach (KeyValuePair<string, List<int>> oneVar in resultToPrint)
            {
                EntityTypeEnum type = QueryProcessor.GetVarEnumType(oneVar.Key);


                switch (type)
                {
                    case EntityTypeEnum.Variable:
                        results.AddRange(PrintVariables(oneVar.Value));
                        break;
                    case EntityTypeEnum.Procedure:
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
            if (indexes.Count != 0)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    results.Add(Tables.VarTable.Instance.GetVar(indexes[i]).Name);
                }
            }

            return results;
        }

        private static List<string> PrintProcedures(List<int> indexes)
        {
            List<string> results = new List<string>();
            if (indexes.Count != 0)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    results.Add(ProcTable.ProcTable.Instance.GetProc(indexes[i]).Name);
                }
            }

            return results;
        }

        private static List<string> PrintStatements(List<int> indexes)
        {
            List<string> results = new List<string>();
            if (indexes.Count != 0)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    results.Add(indexes[i].ToString());

                }
            }
            return results;
        }
    }
}
