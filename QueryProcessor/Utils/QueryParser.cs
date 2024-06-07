using Parser.AST;
using Parser.Interfaces;
using QueryProcessor.Helper;
using Utils.Enums;
using Utils.Helper;

namespace QueryProcessor.Utils
{
    public static class QueryParser
    {
        private static Dictionary<string, List<int>>? _variableIndexes;
        private static int _currentSum;
        private static bool _algorithmNotFinished;
        private const string ProcedureNameKey = "procname";
        private const string VariableNameKey = "varname";
        private const string ValueKey = "value";
        private const string StatementKey = "stmt#";
        private static readonly IPkb Pkb= Parser.Pkb.Instance!;
        private static readonly IAst Ast = Parser.AST.Ast.Instance!;

        private static void Initialize()
        {
            _algorithmNotFinished = true;
            _currentSum = -1;
            _variableIndexes = new Dictionary<string, List<int>>();
        }

        public static List<string> GetData(bool testing)
        {
            Initialize();
            InsertIndexesIntoVarTables();
            var queryDetails = QueryProcessor.GetQueryDetails();
            var suchThatPart = new List<string>();
            try
            {
                suchThatPart = new List<string>(queryDetails[SyntaxDirectory.SUCHTHAT]);
            }
            catch (Exception e)
            {
                Console.WriteLine("#" + e.Message);
            };
            
            if (suchThatPart.Count > 0)
            {
                suchThatPart = SortSuchThatPart(suchThatPart);
                do
                {
                    foreach (var method in suchThatPart)
                    {
                        if (method.Length > 0)
                            DecodeMethod(method);
                    }
                    CheckSum();
                } while (_algorithmNotFinished);
            }
            return SendDataToPrint(testing);
        }

        private static void InsertIndexesIntoVarTables()
        {
            var varAttributes = QueryProcessor.GetVariableAttributes();

            foreach (var (key, value) in QueryProcessor.GetQueryVariables())
            {
                var attributes = new Dictionary<string, List<string>>();
                foreach (var entry in varAttributes)
                {
                    var attrSplitted = entry.Key.Split(new string[] { "." }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (key == attrSplitted[0])
                        attributes.Add(attrSplitted[1].ToLower(), entry.Value);
                }

                switch (value)
                {
                    case EntityType.Procedure:
                        _variableIndexes!.Add(key, GetProcedureIndexes(attributes));
                        break;

                    case EntityType.Variable:
                        _variableIndexes!.Add(key, GetVariableIndexes(attributes));
                        break;

                    case EntityType.Assign:
                        _variableIndexes!.Add(key, GetStatementIndexes(attributes, EntityType.Assign));
                        break;

                    case EntityType.If:
                        _variableIndexes!.Add(key, GetStatementIndexes(attributes, EntityType.If));
                        break;

                    case EntityType.While:
                        _variableIndexes!.Add(key, GetStatementIndexes(attributes, EntityType.While));
                        break;
                    case EntityType.Call:
                        _variableIndexes!.Add(key, GetStatementIndexes(attributes, EntityType.Call));
                        break;
                    case EntityType.Statement:
                        _variableIndexes!.Add(key, GetStatementIndexes(attributes, EntityType.Statement));
                        break;
                    case EntityType.Prog_line:
                        _variableIndexes!.Add(key, GetProglineIndexes(attributes));
                        break;
                    case EntityType.Constant:
                        _variableIndexes!.Add(key, GetConstantIndexes(attributes));
                        break;
                    default:
                        throw new Exception(SyntaxDirectory.ERROR);
                }
            }
        }

        private static List<int> GetProcedureIndexes(Dictionary<string, List<string>> attributes)
        {
            var indexes = new List<int>();
            var procNames = new List<string>();

            if (attributes.ContainsKey(ProcedureNameKey))
                procNames = attributes[ProcedureNameKey];
            if (procNames.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (var p in Pkb.ProcTable!.GetProcedureList())
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
            var indexes = new List<int>();
            var varNames = new List<string>();

            if (attributes.TryGetValue(VariableNameKey, out var attribute))
                varNames = attribute;
            if (varNames.Count > 1)
                return indexes;

            char[] charsToTrim = { '"', };

            foreach (var v in Pkb.VarTable!.GetVariablesList())
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

        private static List<int> GetProglineIndexes(IReadOnlyDictionary<string, List<string>> attributes)
        {
            var indexes = new List<int>();
            var procLines = new List<string>();

            if (attributes.ContainsKey(ValueKey))
                procLines = attributes[ValueKey];
            if (procLines.Count > 1)
                return indexes;

            foreach (var stmt in Pkb.StmtTable!.GetStatementsList())
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

        private static List<int> GetConstantIndexes(IReadOnlyDictionary<string, List<string>> attributes)
        {
            var indexes = new List<int>();
            var constants = new List<string>();

            if (attributes.ContainsKey(ValueKey))
                constants = attributes[ValueKey];
            if (constants.Count > 1)
                return indexes;

            if (constants.Count == 1)
                if (!int.TryParse(constants[0], out _))
                    return indexes;

            foreach (var statement in Pkb.StmtTable!.GetStatementsList())
            {
                var node = statement.AstRoot;
                var constValues = Ast!.GetReadOnlyVariables(node);
                if (constants.Count == 1)
                {
                    if (constValues.Contains(int.Parse(constants[0])))
                        indexes.Add(int.Parse(constants[0]));
                }
                else
                    indexes.AddRange(constValues);
            }

            return indexes.Distinct().ToList();
        }

        private static List<int> GetStatementIndexes(Dictionary<string, List<string>> attributes, EntityType type)
        {
            var indexes = new List<int>();
            var stmtNumbers = new List<string>();

            if (attributes.ContainsKey(StatementKey))
                stmtNumbers = attributes[StatementKey];

            if (stmtNumbers.Count > 1)
                return indexes;

            if (stmtNumbers.Count != 1)
                foreach (var statement in Pkb.StmtTable!.GetStatementsList())
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
                    var statement = Pkb.StmtTable!.FindStatement(int.Parse(stmtNumbers[0]));
                    if (statement != null)
                        indexes.Add(statement.LineNumber);
                }
                catch (Exception e)
                {
                    throw new Exception(SyntaxDirectory.ERROR);
                }
            }


            return indexes;
        }

        private static List<string> SendDataToPrint(bool testing)
        {
            var varsToSelect = QueryProcessor.GetVariableToSelect();
            var varIndexesToPrint = new Dictionary<string, List<int>>();
            foreach (var variable in varsToSelect)
            {
                var trimedVar = variable.Trim();
                try
                {
                    varIndexesToPrint.Add(trimedVar, _variableIndexes![trimedVar]);
                }
                catch (Exception e)
                {
                    throw new Exception(SyntaxDirectory.ERROR);
                }
            }

            return ResultPrinter.Print(varIndexesToPrint, testing);
        }

        private static void CheckSum()
        {
            var tmpSum = 0;
            foreach (var (_, value) in _variableIndexes!)
                tmpSum += value.Count;

            if (tmpSum != _currentSum)
                _currentSum = tmpSum;
            else
                _algorithmNotFinished = false;
        }

        private static void DecodeMethod(string method)
        {
            var typeAndArguments = method.Split(new string[] { " ", "(", ")", "," }, System.StringSplitOptions.RemoveEmptyEntries);
            switch (typeAndArguments[0].ToLower())
            {
                case StringDirectory.Modifies:
                    QueryChecker.CheckModifiesOrUses(typeAndArguments[1], typeAndArguments[2], Pkb.Modifies!.AttachValueOfModifies, Pkb.Modifies.AttachValueOfModifies);
                    break;
                case StringDirectory.Uses:
                    QueryChecker.CheckModifiesOrUses(typeAndArguments[1], typeAndArguments[2], Pkb.Uses!.CheckUsesUsed, Pkb.Uses.CheckUsesUsed);
                    break;
                case StringDirectory.Parent:
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], Ast!.CheckParent);
                    break;
                case StringDirectory.ParentAsterisk:
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], Ast!.CheckParentStar);
                    break;
                case StringDirectory.Follows:
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], Ast!.CheckFollowed);
                    break;
                case StringDirectory.FollowsAsterisk:
                    QueryChecker.CheckParentOrFollows(typeAndArguments[1], typeAndArguments[2], Ast!.CheckFollowedStar);
                    break;
                case StringDirectory.Calls:
                    QueryChecker.CheckCalls(typeAndArguments[1], typeAndArguments[2], Pkb.Calls!.CheckCalls);
                    break;
                case StringDirectory.CallsAsterisk:
                    QueryChecker.CheckCalls(typeAndArguments[1], typeAndArguments[2], Pkb.Calls!.CheckCallsStar);
                    break;
                default:
                    throw new Exception(SyntaxDirectory.ERROR);
            }
        }

        public static List<int> GetArgIndexes(string var, EntityType type)
        {
            switch (var)
            {
                case "_":
                    return GetAllArgIndexes(type);

                case string variable when SyntaxDirectory.ArgumentChecker(variable):
                    var name = var.Substring(1, var.Length - 2);
                    if (type == EntityType.Procedure)
                        return new List<int>(new int[] { Pkb.ProcTable!.FindIndexOfProcedure(name) });

                    else if (type == EntityType.Variable)
                        return new List<int>(new int[] { Pkb.VarTable!.FindIndexOfGetIndex(name) });
                    return _variableIndexes![var];

                case string _ when int.TryParse(var, out _):
                    return new List<int>(new int[] { Int32.Parse(var) });
                
                default:
                    return _variableIndexes![var];
            }
        }

        public static List<int> GetAllArgIndexes(EntityType type)
        {
            var result = type switch
            {
                EntityType.Variable => Pkb.VarTable!.GetVariablesList().Select(v => v.Id).ToList(),
                EntityType.Procedure => Pkb.ProcTable!.GetProcedureList().Select(p => p.Id).ToList(),
                _ => Pkb.StmtTable!.GetStatementsList().Select(s => s.LineNumber).ToList()
            };

            return result;
        }

        public static void RemoveIndexesFromLists(string firstArgument, string secondArgument, List<int> firstList, List<int> secondList)
        {

            if (firstArgument != "_" && !SyntaxDirectory.ArgumentChecker(firstArgument) && !int.TryParse(firstArgument, out _))
            {
                _variableIndexes![firstArgument] = _variableIndexes[firstArgument]
                    .Where(i => firstList.Any(j => j == i))
                    .Distinct()
                    .ToList();
            }

            if (secondArgument != "_" && !SyntaxDirectory.ArgumentChecker(secondArgument) && !int.TryParse(secondArgument, out _))
            {
                _variableIndexes![secondArgument] = _variableIndexes[secondArgument]
                    .Where(i => secondList.Any(j => j == i))
                    .Distinct()
                    .ToList();
            }
        }

        private static List<string> SortSuchThatPart(List<string> stp)
        {
            var sortedSuchThatPart = stp.OrderBy(x => x.Contains("\"")).ToList();
            sortedSuchThatPart.Reverse();
            return sortedSuchThatPart;
        }
    }
}
