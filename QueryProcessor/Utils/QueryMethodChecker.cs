using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Tables;

namespace QueryProcessor.Utils
{
    public static class QueryMethodChecker
    {

        public static void CheckModifiesOrUses(string firstArgument, string secondArgument,
                                        Func<Variable, Procedure, bool> methodForProc,
                                        Func<Variable, Statement, bool> methodForStmt)
        {
            EntityTypeEnum firstArgType;
            if (firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"')
                firstArgType = EntityTypeEnum.Procedure;
            else if (int.TryParse(firstArgument, out _))
                firstArgType = EntityTypeEnum.Statement;
            else if (firstArgument == "_")
                firstArgType = EntityTypeEnum.Statement;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if (firstArgType == EntityTypeEnum.Procedure)
                CheckProcedureModifiesOrUses(firstArgument, secondArgument, methodForProc);
            else
                CheckStatementModifiesOrUses(firstArgument, secondArgument, methodForStmt);
        }

        private static void CheckProcedureModifiesOrUses(string firstArgument, string secondArgument, Func<Variable, Procedure, bool> IsModifiedOrUsedByProc)
        {
            EntityTypeEnum secondArgType;
            EntityTypeEnum firstArgType;

            if (firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"')
                firstArgType = EntityTypeEnum.Procedure;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityTypeEnum.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityTypeEnum.Variable;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryDataGetter.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryDataGetter.GetArgIndexes(secondArgument, secondArgType);

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
            QueryDataGetter.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                   procStayinIndexes,
                                                   varStayinIndexes);
        }

        private static void CheckStatementModifiesOrUses(string firstArgument, string secondArgument, Func<Variable, Statement, bool> IsModifiedOrUsedByStmt)
        {
            EntityTypeEnum secondArgType;
            EntityTypeEnum firstArgType;

            if (int.TryParse(firstArgument, out _))
                firstArgType = EntityTypeEnum.Statement;
            else if (firstArgument == "_")
                firstArgType = EntityTypeEnum.Statement;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityTypeEnum.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityTypeEnum.Variable;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryDataGetter.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryDataGetter.GetArgIndexes(secondArgument, secondArgType);

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
            QueryDataGetter.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                   stmtStayinIndexes,
                                                   varStayinIndexes);
        }

        public static void CheckParentOrFollows(string firstArgument, string secondArgument, Func<TNODE, TNODE, bool> method)
        {
            EntityTypeEnum firstArgType;
            EntityTypeEnum secondArgType;
            if (int.TryParse(firstArgument, out _))
                firstArgType = EntityTypeEnum.Statement;
            else if (firstArgument == "_")
                firstArgType = EntityTypeEnum.Statement;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if (int.TryParse(secondArgument, out _))
                secondArgType = EntityTypeEnum.Statement;
            else if (secondArgument == "_")
                secondArgType = EntityTypeEnum.Statement;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryDataGetter.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryDataGetter.GetArgIndexes(secondArgument, secondArgType);

            List<int> firstStayinIndexes = new List<int>();
            List<int> secondStayinIndexes = new List<int>();

            TNODE first;
            TNODE second;
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

            QueryDataGetter.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                  firstStayinIndexes,
                                                  secondStayinIndexes);

        }

        public static void CheckCalls(string firstArgument, string secondArgument, Func<string, string, bool> method)
        {
            EntityTypeEnum secondArgType;
            EntityTypeEnum firstArgType;

            if (firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"')
                firstArgType = EntityTypeEnum.Procedure;
            else if (firstArgument == "_")
                firstArgType = EntityTypeEnum.Procedure;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityTypeEnum.Procedure;
            else if (secondArgument == "_")
                secondArgType = EntityTypeEnum.Procedure;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryDataGetter.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryDataGetter.GetArgIndexes(secondArgument, secondArgType);

            List<int> firstStayinIndexes = new List<int>();
            List<int> secondStayinIndexes = new List<int>();

            if (firstArgType != EntityTypeEnum.Procedure)
                throw new ArgumentException("Not a procedure: {0}", firstArgument);
            else if (secondArgType != EntityTypeEnum.Procedure)
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

            QueryDataGetter.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                  firstStayinIndexes,
                                                  secondStayinIndexes);
        }

        public static void CheckNext(string firstArgument, string secondArgument, Func<int, int, bool> method)
        {
            EntityTypeEnum firstArgType;
            EntityTypeEnum secondArgType;
            if (int.TryParse(firstArgument, out _))
                firstArgType = EntityTypeEnum.Prog_line;
            else if (firstArgument == "_")
                firstArgType = EntityTypeEnum.Prog_line;
            else
                firstArgType = QueryProcessor.GetVarEnumType(firstArgument);

            if (int.TryParse(secondArgument, out _))
                secondArgType = EntityTypeEnum.Prog_line;
            else if (secondArgument == "_")
                secondArgType = EntityTypeEnum.Prog_line;
            else
                secondArgType = QueryProcessor.GetVarEnumType(secondArgument);

            List<int> firstArgIndexes = QueryDataGetter.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryDataGetter.GetArgIndexes(secondArgument, secondArgType);

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

            QueryDataGetter.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                  firstStayinIndexes,
                                                  secondStayinIndexes);

        }

        private static TNODE GetNodeByType(EntityTypeEnum et, int ind)
        {
            TNODE node;
            if (et == EntityTypeEnum.Procedure)
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
