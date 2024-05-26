using Parser.AST;
using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Calls;
using Parser.Modifies;
using Parser.Tables;
using Parser.Uses;

namespace QueryProcessor.Utils
{
    public static class QueryParser
    {
        private static Dictionary<string, List<int>> variableIndexes = null;
        private static int Sum;
        private static bool AlgorithmNotEnd;
        private static readonly string procname = "procname";
        private static readonly string varname = "varname";
        private static readonly string value = "value";
        private static readonly string stmth = "stmt#";

        private static void Init()
        {
            AlgorithmNotEnd = true;
            Sum = -1;
            variableIndexes = new Dictionary<string, List<int>>();
        }

        public static List<string> GetData(bool testing)
        {
            Init();
            InsertIndexesIntoVarTables();
            Dictionary<string, List<string>> queryDetails = QueryProcessor.GetQueryDetails();
            List<string> suchThatPart = new List<string>();
            try
            {
                suchThatPart = new List<string>(queryDetails["SUCH THAT"]);
            }
            catch (Exception e)
            {
                Console.WriteLine("#" + e.Message);
            };

            //Algorithm here...
            if (suchThatPart.Count > 0)
            {
                suchThatPart = SortSuchThatPart(suchThatPart);
                do
                {
                    foreach (string method in suchThatPart)
                    {
                        if (method.Length > 0)
                            DecodeMethod(method);
                    }
                    CheckSum();
                } while (AlgorithmNotEnd);
            }

            //After algorithm....
            return SendDataToPrint(testing);
        }

        private static void InsertIndexesIntoVarTables()
        {
            Dictionary<string, List<string>> varAttributes = QueryProcessor.GetVariableAttributes();

            foreach (KeyValuePair<string, EntityType> oneVar in QueryProcessor.GetQueryVariables())
            {
                Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<string>> entry in varAttributes)
                {
                    string[] attrSplitted = entry.Key.Split(new string[] { "." }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (oneVar.Key == attrSplitted[0])
                        attributes.Add(attrSplitted[1].ToLower(), entry.Value);
                }

                switch (oneVar.Value)
                {
                    case EntityType.Procedure:
                        variableIndexes.Add(oneVar.Key, GetProcedureIndexes(attributes));
                        break;

                    case EntityType.Variable:
                        variableIndexes.Add(oneVar.Key, GetVariableIndexes(attributes));
                        break;

                    case EntityType.Assign:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityType.Assign));
                        break;

                    case EntityType.If:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityType.If));
                        break;

                    case EntityType.While:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityType.While));
                        break;
                    case EntityType.Call:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityType.Call));
                        break;
                    case EntityType.Statement:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityType.Statement));
                        break;
                    case EntityType.Prog_line:
                        variableIndexes.Add(oneVar.Key, GetProglineIndexes(attributes));
                        break;
                    case EntityType.Constant:
                        variableIndexes.Add(oneVar.Key, GetConstantIndexes(attributes));
                        break;
                    default:
                        throw new System.ArgumentException("# Wrong typo!");
                }
            }
        }

        private static List<int> GetProcedureIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> procName = new List<string>();

            if (attributes.ContainsKey(procname))
                procName = attributes[procname];
            if (procName.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (Procedure p in ProcedureTable.Instance.ProceduresList)
            {
                if (procName.Count == 1)
                {
                    if (p.Identifier == procName[0].Trim(charsToTrim))
                        indexes.Add(p.Id);
                }
                else
                    indexes.Add(p.Id);
            }
            return indexes;
        }

        private static List<int> GetVariableIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> varName = new List<string>();

            if (attributes.ContainsKey(varname))
                varName = attributes[varname];
            if (varName.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (Variable v in ViariableTable.Instance.VariablesList)
            {
                if (varName.Count == 1)
                {
                    if (v.Identifier == varName[0].Trim(charsToTrim))
                        indexes.Add(v.Id);
                }
                else
                    indexes.Add(v.Id);
            }

            return indexes;
        }

        private static List<int> GetProglineIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> procLine = new List<string>();

            if (attributes.ContainsKey(value))
                procLine = attributes[value];
            if (procLine.Count > 1)
                return indexes;

            foreach (Statement stmt in StatementTable.Instance.StatementsList)
            {
                if (procLine.Count == 1)
                {
                    if (stmt.LineNumber.ToString() == procLine[0])
                        indexes.Add(stmt.LineNumber);
                }
                else
                    indexes.Add(stmt.LineNumber);
            }

            return indexes;
        }

        private static List<int> GetConstantIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> constantV = new List<string>();

            if (attributes.ContainsKey(value))
                constantV = attributes[value];
            if (constantV.Count > 1)
                return indexes;

            if (constantV.Count == 1)
                if (!int.TryParse(constantV[0], out _))
                    return indexes;

            foreach (Statement stmt in StatementTable.Instance.StatementsList)
            {
                Node node = stmt.AstRoot;
                List<int> consts = AST.Instance.GetConstants(node);
                if (constantV.Count == 1)
                {
                    if (consts.Contains(Int32.Parse(constantV[0])))
                        indexes.Add(Int32.Parse(constantV[0]));
                }
                else
                    indexes.AddRange(consts);
            }

            return indexes.Distinct().ToList();
        }

        private static List<int> GetStatementIndexes(Dictionary<string, List<string>> attributes, EntityType type)
        {
            List<int> indexes = new List<int>();
            List<string> stmtNr = new List<string>();

            if (attributes.ContainsKey(stmth))
                stmtNr = attributes[stmth];

            if (stmtNr.Count > 1)
                return indexes;

            if (stmtNr.Count != 1)
                foreach (Statement s in StatementTable.Instance.StatementsList)
                {
                    if (s.StmtType == type)
                        indexes.Add(s.LineNumber);
                    else if (type == EntityType.Statement)
                        indexes.Add(s.LineNumber);
                }
            else
            {
                try
                {
                    Statement s = StatementTable.Instance.GetStatement(Int32.Parse(stmtNr[0]));
                    if (s != null)
                        indexes.Add(s.LineNumber);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(string.Format("# Wrong stmt# = {0}", stmtNr[0]));
                }
            }


            return indexes;
        }

        private static List<string> SendDataToPrint(bool testing)
        {
            List<string> varsToSelect = QueryProcessor.GetVariableToSelect();
            Dictionary<string, List<int>> varIndexesToPrint = new Dictionary<string, List<int>>();
            foreach (string var in varsToSelect)
            {
                string trimedVar = var.Trim();
                try
                {
                    varIndexesToPrint.Add(trimedVar, variableIndexes[trimedVar]);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(string.Format("# Wrong argument: \"{0}\"", trimedVar));
                }
            }

            return ResultPrinter.Print(varIndexesToPrint, testing);
        }

        private static void CheckSum()
        {
            int TmpSum = 0;
            foreach (KeyValuePair<string, List<int>> item in variableIndexes)
            {
                TmpSum += item.Value.Count;
            }

            if (TmpSum != Sum)
                Sum = TmpSum;
            else
                AlgorithmNotEnd = false;
        }

        private static void DecodeMethod(string method)
        {
            string[] typeAndArguments = method.Split(new string[] { " ", "(", ")", "," }, System.StringSplitOptions.RemoveEmptyEntries);
            switch (typeAndArguments[0].ToLower())
            {
                case "modifies":
                    QueryChecker.CheckModifiesOrUses(typeAndArguments[1], typeAndArguments[2], Modifies.Instance.IsModified, Modifies.Instance.IsModified);
                    break;
                case "uses":
                    QueryChecker.CheckModifiesOrUses(typeAndArguments[1], typeAndArguments[2], Uses.Instance.IsUsed, Uses.Instance.IsUsed);
                    break;
                case "parent":
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.Instance.IsParent);
                    break;
                case "parent*":
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.Instance.IsParentStar);
                    break;
                case "follows":
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.Instance.IsFollowed);
                    break;
                case "follows*":
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.Instance.IsFollowedStar);
                    break;
                case "calls":
                    QueryChecker.CheckCalls(typeAndArguments[1], typeAndArguments[2], Calls.Instance.IsCalls);
                    break;
                case "calls*":
                    QueryChecker.CheckCalls(typeAndArguments[1], typeAndArguments[2], Calls.Instance.IsCallsStar);
                    break;
                default:
                    throw new ArgumentException(string.Format("# Niepoprawna metoda: \"{0}\"", typeAndArguments[0]));
            }
        }

        public static List<int> GetArgIndexes(string var, EntityType type)
        {
            if (var == "_")
                return GetAllArgIndexes(type);

            if (var[0] == '\"' & var[var.Length - 1] == '\"')
            {
                string name = var.Substring(1, var.Length - 2);
                if (type == EntityType.Procedure)
                    return new List<int>(new int[] { ProcedureTable.Instance.GetProcIndex(name) });

                else if (type == EntityType.Variable)
                    return new List<int>(new int[] { ViariableTable.Instance.GetVarIndex(name) });
            }

            if (int.TryParse(var, out _))
                return new List<int>(new int[] { Int32.Parse(var) });
            return variableIndexes[var];
        }

        public static List<int> GetAllArgIndexes(EntityType type)
        {
            List<int> result = new List<int>();
            if (type == EntityType.Variable)
                foreach (Variable v in ViariableTable.Instance.VariablesList)
                    result.Add(v.Id);

            else if (type == EntityType.Procedure)
                foreach (Procedure p in ProcedureTable.Instance.ProceduresList)
                    result.Add(p.Id);
            else
                foreach (Statement s in StatementTable.Instance.StatementsList)
                    result.Add(s.LineNumber);

            return result;
        }

        public static void RemoveIndexesFromLists(string firstArgument, string secondArgument, List<int> firstList, List<int> secondList)
        {

            if (firstArgument != "_")
                if (!(firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"'))
                    if (!(int.TryParse(firstArgument, out _)))
                        variableIndexes[firstArgument] = variableIndexes[firstArgument].Where(i => firstList.Any(j => j == i)).Distinct().ToList();
            if (secondArgument != "_")
                if (!(secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                    if (!(int.TryParse(secondArgument, out _)))
                        variableIndexes[secondArgument] = variableIndexes[secondArgument].Where(i => secondList.Any(j => j == i)).Distinct().ToList();
        }

        private static List<string> SortSuchThatPart(List<string> stp)
        {
            List<string> newstp = stp.OrderBy(x => x.Contains("\"")).ToList();
            newstp.Reverse();
            return newstp;
        }
    }
}
