using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace QueryProcessor.Utils
{
    public static class QueryDataGetter
    {
        private static Dictionary<string, List<int>> variableIndexes = null;
        private static int Sum;
        private static bool AlgorithmNotEnd;

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
            Dictionary<string, List<string>> varAttributes = QueryProcessor.GetVarAttributes();

            foreach (KeyValuePair<string, EntityTypeEnum> oneVar in QueryProcessor.GetQueryVars())
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
                    case EntityTypeEnum.Procedure:
                        variableIndexes.Add(oneVar.Key, GetProcedureIndexes(attributes));
                        break;

                    case EntityTypeEnum.Variable:
                        variableIndexes.Add(oneVar.Key, GetVariableIndexes(attributes));
                        break;

                    case EntityTypeEnum.Assign:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityTypeEnum.Assign));
                        break;

                    case EntityTypeEnum.If:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityTypeEnum.If));
                        break;

                    case EntityTypeEnum.While:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityTypeEnum.While));
                        break;
                    case EntityTypeEnum.Call:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityTypeEnum.Call));
                        break;
                    case EntityTypeEnum.Statement:
                        variableIndexes.Add(oneVar.Key, GetStatementIndexes(attributes, EntityTypeEnum.Statement));
                        break;
                    case EntityTypeEnum.Prog_line:
                        variableIndexes.Add(oneVar.Key, GetProglineIndexes(attributes));
                        break;
                    case EntityTypeEnum.Constant:
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

            if (attributes.ContainsKey("procname"))
                procName = attributes["procname"];
            if (procName.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (Procedure p in ProcTable.ProcTable.Instance.Procedures)
            {
                if (procName.Count == 1)
                {
                    if (p.Name == procName[0].Trim(charsToTrim))
                        indexes.Add(p.Index);
                }
                else
                    indexes.Add(p.Index);
            }
            return indexes;
        }

        private static List<int> GetVariableIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> varName = new List<string>();

            if (attributes.ContainsKey("varname"))
                varName = attributes["varname"];
            if (varName.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (Variable v in VarTable.VarTable.Instance.Variables)
            {
                if (varName.Count == 1)
                {
                    if (v.Name == varName[0].Trim(charsToTrim))
                        indexes.Add(v.Index);
                }
                else
                    indexes.Add(v.Index);
            }

            return indexes;
        }

        private static List<int> GetProglineIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> procLine = new List<string>();

            if (attributes.ContainsKey("value"))
                procLine = attributes["value"];
            if (procLine.Count > 1)
                return indexes;

            foreach (Statement stmt in StmtTable.StmtTable.Instance.Statements)
            {
                if (procLine.Count == 1)
                {
                    if (stmt.CodeLine.ToString() == procLine[0])
                        indexes.Add(stmt.CodeLine);
                }
                else
                    indexes.Add(stmt.CodeLine);
            }

            return indexes;
        }

        private static List<int> GetConstantIndexes(Dictionary<string, List<string>> attributes)
        {
            List<int> indexes = new List<int>();
            List<string> constantV = new List<string>();

            if (attributes.ContainsKey("value"))
                constantV = attributes["value"];
            if (constantV.Count > 1)
                return indexes;

            if (constantV.Count == 1)
                if (!int.TryParse(constantV[0], out _))
                    return indexes;

            foreach (Statement stmt in StmtTable.StmtTable.Instance.Statements)
            {
                TNODE node = stmt.AstRoot;
                List<int> consts = AST.AST.Instance.GetConstants(node);
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

        private static List<int> GetStatementIndexes(Dictionary<string, List<string>> attributes, EntityTypeEnum enumType)
        {
            List<int> indexes = new List<int>();
            List<string> stmtNr = new List<string>();

            if (attributes.ContainsKey("stmt#"))
                stmtNr = attributes["stmt#"];

            if (stmtNr.Count > 1)
                return indexes;

            if (stmtNr.Count != 1)
                foreach (Statement s in StmtTable.StmtTable.Instance.Statements)
                {
                    if (s.Type == enumType)
                        indexes.Add(s.CodeLine);
                    else if (enumType == EntityTypeEnum.Statement)
                        indexes.Add(s.CodeLine);
                }
            else
            {
                try
                {
                    Statement s = StmtTable.StmtTable.Instance.GetStmt(Int32.Parse(stmtNr[0]));
                    if (s != null)
                        indexes.Add(s.CodeLine);
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
            List<string> varsToSelect = QueryProcessor.GetVarToSelect();
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
                    QueryMethodChecker.CheckModifiesOrUses(typeAndArguments[1], typeAndArguments[2], Modifies.Modifies.Instance.IsModified, Modifies.Modifies.Instance.IsModified);
                    break;
                case "uses":
                    QueryMethodChecker.CheckModifiesOrUses(typeAndArguments[1], typeAndArguments[2], Uses.Uses.Instance.IsUsed, Uses.Uses.Instance.IsUsed);
                    break;
                case "parent":
                    QueryMethodChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.AST.Instance.IsParent);
                    break;
                case "parent*":
                    QueryMethodChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.AST.Instance.IsParentStar);
                    break;
                case "follows":
                    QueryMethodChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.AST.Instance.IsFollowed);
                    break;
                case "follows*":
                    QueryMethodChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], AST.AST.Instance.IsFollowedStar);
                    break;
                case "calls":
                    QueryMethodChecker.CheckCalls(typeAndArguments[1], typeAndArguments[2], Calls.Calls.Instance.IsCalls);
                    break;
                case "calls*":
                    QueryMethodChecker.CheckCalls(typeAndArguments[1], typeAndArguments[2], Calls.Calls.Instance.IsCallsStar);
                    break;
                case "next":
                    QueryMethodChecker.CheckNext(typeAndArguments[1], typeAndArguments[2], Next.Next.Instance.IsNext);
                    break;
                case "next*":
                    QueryMethodChecker.CheckNext(typeAndArguments[1], typeAndArguments[2], Next.Next.Instance.IsNextStar);
                    break;
                default:
                    throw new ArgumentException(string.Format("# Niepoprawna metoda: \"{0}\"", typeAndArguments[0]));
            }
        }

        public static List<int> GetArgIndexes(string var, EntityTypeEnum type)
        {
            if (var == "_")
                return GetAllArgIndexes(type);

            if (var[0] == '\"' & var[var.Length - 1] == '\"')
            {
                string name = var.Substring(1, var.Length - 2);
                if (type == EntityTypeEnum.Procedure)
                    return new List<int>(new int[] { ProcTable.ProcTable.Instance.GetProcIndex(name) });

                else if (type == EntityTypeEnum.Variable)
                    return new List<int>(new int[] { VarTable.VarTable.Instance.GetVarIndex(name) });
            }

            if (int.TryParse(var, out _))
                return new List<int>(new int[] { Int32.Parse(var) });
            return variableIndexes[var];
        }

        public static List<int> GetAllArgIndexes(EntityTypeEnum type)
        {
            List<int> result = new List<int>();
            if (type == EntityTypeEnum.Variable)
                foreach (Variable v in VarTable.VarTable.Instance.Variables)
                    result.Add(v.Index);

            else if (type == EntityTypeEnum.Procedure)
                foreach (Procedure p in ProcTable.ProcTable.Instance.Procedures)
                    result.Add(p.Index);
            else
                foreach (Statement s in StmtTable.StmtTable.Instance.Statements)
                    result.Add(s.CodeLine);

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
