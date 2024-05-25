using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Tables;

namespace QueryProcessor.Utils
{
    public static class QueryChecker
    {

        public static void CheckModifiesOrUses(string firstArgument, string secondArgument,
                                        Func<Variable, Procedure, bool> methodForProc,
                                        Func<Variable, Statement, bool> methodForStmt)
        {
            EntityType firstArgType;
            if (firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"')
                firstArgType = EntityType.Procedure;
            else if (int.TryParse(firstArgument, out _))
                firstArgType = EntityType.Statement;
            else if (firstArgument == "_")
                firstArgType = EntityType.Statement;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if (firstArgType == EntityType.Procedure)
                CheckProcedureModifiesOrUses(firstArgument, secondArgument, methodForProc);
            else
                CheckStatementModifiesOrUses(firstArgument, secondArgument, methodForStmt);
        }

        private static void CheckProcedureModifiesOrUses(string firstArgument, string secondArgument, Func<Variable, Procedure, bool> IsModifiedOrUsedByProc)
        {
            EntityType secondArgType;
            EntityType firstArgType;

            if (firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"')
                firstArgType = EntityType.Procedure;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityType.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityType.Variable;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> procStayinIndexes = new List<int>();
            List<int> varStayinIndexes = new List<int>();

            Procedure proc;
            Variable var;
            foreach (int firstInd in firstArgIndexes)
                foreach (int secondInd in secondArgIndexes)
                {
                    proc = ProcTable.Instance.GetProc(firstInd);
                    var = VarTable.Instance.GetVar(secondInd);
                    //Modifies.Modifies.Instance.IsModified
                    if (IsModifiedOrUsedByProc(var, proc))
                    {
                        procStayinIndexes.Add(firstInd);
                        varStayinIndexes.Add(secondInd);

                    }
                }
            QueryParser.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                   procStayinIndexes,
                                                   varStayinIndexes);
        }

        private static void CheckStatementModifiesOrUses(string firstArgument, string secondArgument, Func<Variable, Statement, bool> IsModifiedOrUsedByStmt)
        {
            EntityType secondArgType;
            EntityType firstArgType;

            if (int.TryParse(firstArgument, out _))
                firstArgType = EntityType.Statement;
            else if (firstArgument == "_")
                firstArgType = EntityType.Statement;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityType.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityType.Variable;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> stmtStayinIndexes = new List<int>();
            List<int> varStayinIndexes = new List<int>();

            Statement stmt;
            Variable var;
            foreach (int firstInd in firstArgIndexes)
                foreach (int secondInd in secondArgIndexes)
                {
                    stmt = StmtTable.Instance.GetStmt(firstInd);
                    var = VarTable.Instance.GetVar(secondInd);
                    //Modifies.Modifies.Instance.IsModified
                    if (IsModifiedOrUsedByStmt(var, stmt))
                    {
                        stmtStayinIndexes.Add(firstInd);
                        varStayinIndexes.Add(secondInd);
                    }
                }
            QueryParser.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                   stmtStayinIndexes,
                                                   varStayinIndexes);
        }

        public static void CheckParentOrFollows(string firstArgument, string secondArgument, Func<Node, Node, bool> method)
        {
            EntityType firstArgType;
            EntityType secondArgType;
            if (int.TryParse(firstArgument, out _))
                firstArgType = EntityType.Statement;
            else if (firstArgument == "_")
                firstArgType = EntityType.Statement;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if (int.TryParse(secondArgument, out _))
                secondArgType = EntityType.Statement;
            else if (secondArgument == "_")
                secondArgType = EntityType.Statement;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> firstStayinIndexes = new List<int>();
            List<int> secondStayinIndexes = new List<int>();

            Node first;
            Node second;
            foreach (int firstInd in firstArgIndexes)
                foreach (int secondInd in secondArgIndexes)
                {
                    first = GetNodeByType(firstArgType, firstInd);
                    second = GetNodeByType(secondArgType, secondInd);
                    if (method(first, second))
                    {
                        firstStayinIndexes.Add(firstInd);
                        secondStayinIndexes.Add(secondInd);
                    }
                }

            QueryParser.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                  firstStayinIndexes,
                                                  secondStayinIndexes);

        }

        public static void CheckCalls(string firstArgument, string secondArgument, Func<string, string, bool> method)
        {
            EntityType secondArgType;
            EntityType firstArgType;

            if (firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"')
                firstArgType = EntityType.Procedure;
            else if (firstArgument == "_")
                firstArgType = EntityType.Procedure;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityType.Procedure;
            else if (secondArgument == "_")
                secondArgType = EntityType.Procedure;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> firstStayinIndexes = new List<int>();
            List<int> secondStayinIndexes = new List<int>();

            if (firstArgType != EntityType.Procedure)
                throw new ArgumentException("Not a procedure: {0}", firstArgument);
            else if (secondArgType != EntityType.Procedure)
                throw new ArgumentException("Not a procedure: {0}", secondArgument);

            string first, second;
            Procedure p1, p2;
            foreach (int firstInd in firstArgIndexes)
                foreach (int secondInd in secondArgIndexes)
                {
                    p1 = ProcTable.Instance.GetProc(firstInd);
                    p2 = ProcTable.Instance.GetProc(secondInd);

                    first = p1 == null ? "" : p1.Name;
                    second = p2 == null ? "" : p2.Name;

                    if (method(first, second))
                    {
                        firstStayinIndexes.Add(firstInd);
                        secondStayinIndexes.Add(secondInd);
                    }
                }

            QueryParser.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                  firstStayinIndexes,
                                                  secondStayinIndexes);
        }

        public static void CheckNext(string firstArgument, string secondArgument, Func<int, int, bool> method)
        {
            EntityType firstArgType;
            EntityType secondArgType;
            if (int.TryParse(firstArgument, out _))
                firstArgType = EntityType.Prog_line;
            else if (firstArgument == "_")
                firstArgType = EntityType.Prog_line;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if (int.TryParse(secondArgument, out _))
                secondArgType = EntityType.Prog_line;
            else if (secondArgument == "_")
                secondArgType = EntityType.Prog_line;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> firstStayinIndexes = new List<int>();
            List<int> secondStayinIndexes = new List<int>();

            foreach (int firstInd in firstArgIndexes)
                foreach (int secondInd in secondArgIndexes)
                {
                    if (method(firstInd, secondInd))
                    {
                        firstStayinIndexes.Add(firstInd);
                        secondStayinIndexes.Add(secondInd);
                    }
                }

            QueryParser.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                  firstStayinIndexes,
                                                  secondStayinIndexes);

        }

        private static Node GetNodeByType(EntityType et, int ind)
        {
            Node node;
            if (et == EntityType.Procedure)
            {
                node = ProcTable.Instance.GetAstRoot(ind);
            }
            else
            {
                node = StmtTable.Instance.GetAstRoot(ind);
            }

            return node;
        }


    }
}
