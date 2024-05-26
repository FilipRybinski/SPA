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
            EntityType firstArgType = DetermineEntityType(firstArgument);

            if (firstArgType == EntityType.Procedure)
                CheckProcedureModifiesOrUses(firstArgument, secondArgument, methodForProc);
            else
                CheckStatementModifiesOrUses(firstArgument, secondArgument, methodForStmt);
        }
        private static EntityType DetermineEntityType(string argument)
        {
            if (argument[0] == '\"' && argument[argument.Length - 1] == '\"')
                return EntityType.Procedure;
            else if (int.TryParse(argument, out _))
                return EntityType.Statement;
            else if (argument == "_")
                return EntityType.Statement;
            else
                return QueryProcessor.GetVariableEnumType(argument);
        }

        private static void CheckProcedureModifiesOrUses(string firstArgument, string secondArgument, Func<Variable, Procedure, bool> IsModifiedOrUsedByProc)
        {
            EntityType secondArgType;
            EntityType firstArgType;

            if (firstArgument[0] == '\"' & firstArgument[firstArgument.Length - 1] == '\"')
                firstArgType = EntityType.Procedure;
            else
                firstArgType = QueryProcessor.GetVariableEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityType.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityType.Variable;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> procStayinIndexes = new List<int>();
            List<int> varStayinIndexes = new List<int>();

            Procedure proc;
            Variable var;
            foreach (int firstIndex in firstArgIndexes)
                foreach (int secondIndex in secondArgIndexes)
                {
                    proc = ProcedureTable.Instance.GetProcedure(firstIndex);
                    var = ViariableTable.Instance.GetVar(secondIndex);
                    //Modifies.Modifies.Instance.IsModified
                    if (IsModifiedOrUsedByProc(var, proc))
                    {
                        procStayinIndexes.Add(firstIndex);
                        varStayinIndexes.Add(secondIndex);

                    }
                }
            QueryParser.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                   procStayinIndexes,
                                                   varStayinIndexes);
        }

        private static void CheckStatementModifiesOrUses(string firstArgument, string secondArgument, Func<Variable, Statement, bool> IsModifiedOrUsedByStmt)
        {
            EntityType firstArgType;
            EntityType secondArgType;

            if (int.TryParse(firstArgument, out _))
                firstArgType = EntityType.Statement;
            else if (firstArgument == "_")
                firstArgType = EntityType.Statement;
            else
                firstArgType = QueryProcessor.GetVariableEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityType.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityType.Variable;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> stmtStayinIndexes = new List<int>();
            List<int> varStayinIndexes = new List<int>();

            foreach (int firstIndex in firstArgIndexes)
                foreach (int secondIndex in secondArgIndexes)
                {
                    Statement statement = StatementTable.Instance.GetStatement(firstIndex);
                    Variable variable = ViariableTable.Instance.GetVar(secondIndex);
                    //Modifies.Modifies.Instance.IsModified
                    if (IsModifiedOrUsedByStmt(variable, statement))
                    {
                        stmtStayinIndexes.Add(firstIndex);
                        varStayinIndexes.Add(secondIndex);
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
                firstArgType = QueryProcessor.GetVariableEnumType(firstArgument);

            if (int.TryParse(secondArgument, out _))
                secondArgType = EntityType.Statement;
            else if (secondArgument == "_")
                secondArgType = EntityType.Statement;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> firstStayinIndexes = new List<int>();
            List<int> secondStayinIndexes = new List<int>();

            Node first;
            Node second;
            foreach (int firstIndex in firstArgIndexes)
                foreach (int secondIndex in secondArgIndexes)
                {
                    first = GetNodeByType(firstArgType, firstIndex);
                    second = GetNodeByType(secondArgType, secondIndex);
                    if (method(first, second))
                    {
                        firstStayinIndexes.Add(firstIndex);
                        secondStayinIndexes.Add(secondIndex);
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
                firstArgType = QueryProcessor.GetVariableEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[secondArgument.Length - 1] == '\"'))
                secondArgType = EntityType.Procedure;
            else if (secondArgument == "_")
                secondArgType = EntityType.Procedure;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

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
            foreach (int firstIndex in firstArgIndexes)
                foreach (int secondIndex in secondArgIndexes)
                {
                    p1 = ProcedureTable.Instance.GetProcedure(firstIndex);
                    p2 = ProcedureTable.Instance.GetProcedure(secondIndex);

                    first = p1 == null ? "" : p1.Identifier;
                    second = p2 == null ? "" : p2.Identifier;

                    if (method(first, second))
                    {
                        firstStayinIndexes.Add(firstIndex);
                        secondStayinIndexes.Add(secondIndex);
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
                firstArgType = QueryProcessor.GetVariableEnumType(firstArgument);

            if (int.TryParse(secondArgument, out _))
                secondArgType = EntityType.Prog_line;
            else if (secondArgument == "_")
                secondArgType = EntityType.Prog_line;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

            List<int> firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            List<int> secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            List<int> firstStayinIndexes = new List<int>();
            List<int> secondStayinIndexes = new List<int>();

            foreach (int firstIndex in firstArgIndexes)
                foreach (int secondIndex in secondArgIndexes)
                {
                    if (method(firstIndex, secondIndex))
                    {
                        firstStayinIndexes.Add(firstIndex);
                        secondStayinIndexes.Add(secondIndex);
                    }
                }

            QueryParser.RemoveIndexesFromLists(firstArgument, secondArgument,
                                                  firstStayinIndexes,
                                                  secondStayinIndexes);

        }

        private static Node GetNodeByType(EntityType entityType, int index)
        {
            if (entityType == EntityType.Procedure)
            {
                return ProcedureTable.Instance.GetAstRoot(index);
            }
            else
            {
                return StatementTable.Instance.GetAstRoot(index);
            }

        }


    }
}
