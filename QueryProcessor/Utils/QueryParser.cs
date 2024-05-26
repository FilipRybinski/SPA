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
        private static int currentSum;
        private static bool algorithmNotFinished;
        private static readonly string procedureNameKey = "procname";
        private static readonly string variableNameKey = "varname";
        private static readonly string valueKey = "value";
        private static readonly string statementKey = "stmt#";

        private static void Initialize()
        {
            algorithmNotFinished = true;
            currentSum = -1;
            variableIndexes = new Dictionary<string, List<int>>();
        }

        public static List<string> GetData(bool testing)
        {
            Initialize();
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
                } while (algorithmNotFinished);
            }

            //After algorithm....
            return SendDataToPrint(testing);
        }

        private static void InsertIndexesIntoVarTables()
        {
            Dictionary<string, List<string>> varAttributes = QueryProcessor.GetVariableAttributes();

            foreach (KeyValuePair<string, EntityType> variable in QueryProcessor.GetQueryVariables())
            {
                Dictionary<string, List<string>> attributes = new Dictionary<string, List<string>>();
                foreach (KeyValuePair<string, List<string>> entry in varAttributes)
                {
                    string[] attrSplitted = entry.Key.Split(new string[] { "." }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (variable.Key == attrSplitted[0])
                        attributes.Add(attrSplitted[1].ToLower(), entry.Value);
                }

                switch (variable.Value)
                {
                    case EntityType.Procedure:
                        variableIndexes.Add(variable.Key, GetProcedureIndexes(attributes));
                        break;

                    case EntityType.Variable:
                        variableIndexes.Add(variable.Key, GetVariableIndexes(attributes));
                        break;

                    case EntityType.Assign:
                        variableIndexes.Add(variable.Key, GetStatementIndexes(attributes, EntityType.Assign));
                        break;

                    case EntityType.If:
                        variableIndexes.Add(variable.Key, GetStatementIndexes(attributes, EntityType.If));
                        break;

                    case EntityType.While:
                        variableIndexes.Add(variable.Key, GetStatementIndexes(attributes, EntityType.While));
                        break;
                    case EntityType.Call:
                        variableIndexes.Add(variable.Key, GetStatementIndexes(attributes, EntityType.Call));
                        break;
                    case EntityType.Statement:
                        variableIndexes.Add(variable.Key, GetStatementIndexes(attributes, EntityType.Statement));
                        break;
                    case EntityType.Prog_line:
                        variableIndexes.Add(variable.Key, GetProglineIndexes(attributes));
                        break;
                    case EntityType.Constant:
                        variableIndexes.Add(variable.Key, GetConstantIndexes(attributes));
                        break;
                    default:
                        throw new System.ArgumentException("# Invalid entity type!");
                }
            }
        }

        private static List<int> GetProcedureIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> procNames = new List<string>();

            if (attributes.ContainsKey(procedureNameKey))
                procNames = attributes[procedureNameKey];
            if (procNames.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (Procedure p in ProcedureTable.Instance.ProceduresList)
            {
                if (procNames.Count == 1)
                {
                    if (p.Identifier == procNames[0].Trim(charsToTrim))
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
            List<string> varNames = new List<string>();

            if (attributes.ContainsKey(variableNameKey))
                varNames = attributes[variableNameKey];
            if (varNames.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (Variable v in ViariableTable.Instance.VariablesList)
            {
                if (varNames.Count == 1)
                {
                    if (v.Identifier == varNames[0].Trim(charsToTrim))
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
            List<string> procLines = new List<string>();

            if (attributes.ContainsKey(valueKey))
                procLines = attributes[valueKey];
            if (procLines.Count > 1)
                return indexes;

            foreach (Statement stmt in StatementTable.Instance.StatementsList)
            {
                if (procLines.Count == 1)
                {
                    if (stmt.LineNumber.ToString() == procLines[0])
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
            List<string> constants = new List<string>();

            if (attributes.ContainsKey(valueKey))
                constants = attributes[valueKey];
            if (constants.Count > 1)
                return indexes;

            if (constants.Count == 1)
                if (!int.TryParse(constants[0], out _))
                    return indexes;

            foreach (Statement statement in StatementTable.Instance.StatementsList)
            {
                Node node = statement.AstRoot;
                List<int> constValues = AST.Instance.GetConstants(node);
                if (constants.Count == 1)
                {
                    if (constValues.Contains(Int32.Parse(constants[0])))
                        indexes.Add(Int32.Parse(constants[0]));
                }
                else
                    indexes.AddRange(constValues);
            }

            return indexes.Distinct().ToList();
        }

        private static List<int> GetStatementIndexes(Dictionary<string, List<string>> attributes, EntityType type)
        {
            List<int> indexes = new List<int>();
            List<string> stmtNumbers = new List<string>();

            if (attributes.ContainsKey(statementKey))
                stmtNumbers = attributes[statementKey];

            if (stmtNumbers.Count > 1)
                return indexes;

            if (stmtNumbers.Count != 1)
                foreach (Statement statement in StatementTable.Instance.StatementsList)
                {
                    if (statement.StmtType == type)
                        indexes.Add(statement.LineNumber);
                    else if (type == EntityType.Statement)
                        indexes.Add(statement.LineNumber);
                }
            else
            {
                try
                {
                    Statement statement = StatementTable.Instance.GetStatement(Int32.Parse(stmtNumbers[0]));
                    if (statement != null)
                        indexes.Add(statement.LineNumber);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(string.Format("# Wrong stmt# = {0}", stmtNumbers[0]));
                }
            }


            return indexes;
        }

        private static List<string> SendDataToPrint(bool testing)
        {
            List<string> varsToSelect = QueryProcessor.GetVariableToSelect();
            Dictionary<string, List<int>> varIndexesToPrint = new Dictionary<string, List<int>>();
            foreach (string variable in varsToSelect)
            {
                string trimedVar = variable.Trim();
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
            int tmpSum = 0;
            foreach (KeyValuePair<string, List<int>> item in variableIndexes)
            {
                tmpSum += item.Value.Count;
            }

            if (tmpSum != currentSum)
                currentSum = tmpSum;
            else
                algorithmNotFinished = false;
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
            List<string> sortedSuchThatPart = stp.OrderBy(x => x.Contains("\"")).ToList();
            sortedSuchThatPart.Reverse();
            return sortedSuchThatPart;
        }
    }
}
