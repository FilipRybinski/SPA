using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;
using Utils.Enums;
using IPkb = PKB.Interfaces.IPkb;

namespace QueryProcessor.Utils
{
    internal static class QueryChecker
    {
        private static readonly IPkb Pkb= PKB.Pkb.Instance!;
        public static void CheckModifiesOrUses(string firstArgument, string secondArgument,
                                        Func<Variable, Procedure, bool> methodForProc,
                                        Func<Variable, Statement, bool> methodForStmt)
        {
            var firstArgType = DetermineEntityType(firstArgument);

            if (firstArgType == EntityType.Procedure)
                CheckProcedureModifiesOrUses(firstArgument, secondArgument, methodForProc);
            else
                CheckStatementModifiesOrUses(firstArgument, secondArgument, methodForStmt);
        }
        private static EntityType DetermineEntityType(string argument)
        {
            if (argument[0] == '\"' && argument[^1] == '\"')
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

            if (firstArgument[0] == '\"' & firstArgument[^1] == '\"')
                firstArgType = EntityType.Procedure;
            else
                firstArgType = QueryProcessor.GetVariableEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[^1] == '\"'))
                secondArgType = EntityType.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityType.Variable;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

            var firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            var secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            var procStayinIndexes = new List<int>();
            var varStayinIndexes = new List<int>();

            foreach (var firstIndex in firstArgIndexes)
                foreach (var secondIndex in secondArgIndexes)
                {
                    var proc = Pkb.ProcTable!.GetProcedure(firstIndex);
                    var var = Pkb.VarTable!.GetVar(secondIndex);
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

            if ((secondArgument[0] == '\"' & secondArgument[^1] == '\"'))
                secondArgType = EntityType.Variable;
            else if (secondArgument == "_")
                secondArgType = EntityType.Variable;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

            var firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            var secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            var stmtStayinIndexes = new List<int>();
            var varStayinIndexes = new List<int>();

            foreach (var firstIndex in firstArgIndexes)
                foreach (var secondIndex in secondArgIndexes)
                {
                    var statement = Pkb.StmtTable!.GetStatement(firstIndex);
                    var variable = Pkb.VarTable!.GetVar(secondIndex);
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

            var firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            var secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            var firstStayinIndexes = new List<int>();
            var secondStayinIndexes = new List<int>();

            foreach (int firstIndex in firstArgIndexes)
                foreach (int secondIndex in secondArgIndexes)
                {
                    var first = GetNodeByType(firstArgType, firstIndex);
                    var second = GetNodeByType(secondArgType, secondIndex);
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

            if (firstArgument[0] == '\"' & firstArgument[^1] == '\"')
                firstArgType = EntityType.Procedure;
            else if (firstArgument == "_")
                firstArgType = EntityType.Procedure;
            else
                firstArgType = QueryProcessor.GetVariableEnumType(firstArgument);

            if ((secondArgument[0] == '\"' & secondArgument[^1] == '\"'))
                secondArgType = EntityType.Procedure;
            else if (secondArgument == "_")
                secondArgType = EntityType.Procedure;
            else
                secondArgType = QueryProcessor.GetVariableEnumType(secondArgument);

            var firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            var secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            var firstStayinIndexes = new List<int>();
            var secondStayinIndexes = new List<int>();

            if (firstArgType != EntityType.Procedure)
                throw new ArgumentException("Not a procedure: {0}", firstArgument);
            else if (secondArgType != EntityType.Procedure)
                throw new ArgumentException("Not a procedure: {0}", secondArgument);

            foreach (var firstIndex in firstArgIndexes)
                foreach (var secondIndex in secondArgIndexes)
                {
                    var p1 = Pkb.ProcTable!.GetProcedure(firstIndex);
                    var p2 = Pkb.ProcTable.GetProcedure(secondIndex);

                    var first = p1 == null ? "" : p1.Identifier;
                    var second = p2 == null ? "" : p2.Identifier;

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

            var firstArgIndexes = QueryParser.GetArgIndexes(firstArgument, firstArgType);
            var secondArgIndexes = QueryParser.GetArgIndexes(secondArgument, secondArgType);

            var firstStayinIndexes = new List<int>();
            var secondStayinIndexes = new List<int>();

            foreach (var firstIndex in firstArgIndexes)
                foreach (var secondIndex in secondArgIndexes)
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
                return Pkb.ProcTable!.GetAstRoot(index);
            }
            else
            {
                return Pkb.StmtTable!.GetAstRoot(index);
            }

        }


    }
}
