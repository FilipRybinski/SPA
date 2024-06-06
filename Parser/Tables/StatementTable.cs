using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables.Models;
using Utils.Enums;

namespace Parser.Tables
{
    public sealed class StatementTable : IStmtTable
    {
        private static StatementTable? _singletonInstance;

        public static StatementTable? Instance
        {
            get { return _singletonInstance ?? (_singletonInstance = new StatementTable()); }
        }

        public List<Statement?> StatementsList { get; set; }

        private StatementTable()
        {
            StatementsList = new List<Statement?>();
        }

        public Node GetAstRoot(int lineNumber)
        {
            var stmt = GetStatement(lineNumber);
            return stmt == null ? null : stmt.AstRoot;
        }

        public List<Statement?> GetStatementsList()
        {
            return StatementsList;
        }

        public int GetSize()
        {
            return StatementsList.Count();
        }

        public Statement? GetStatement(int lineNumber)
        {
            return StatementsList.FirstOrDefault(i => i.LineNumber == lineNumber)!;
        }

        public int AddStatement(EntityType entityType, int lineNumber)
        {
            if (StatementsList.Any(i => i.LineNumber == lineNumber))
            {
                return -1;
            }
            else
            {
                var newStmt = new Statement(entityType, lineNumber);
                StatementsList.Add(newStmt);
                return 0;
            }
        }

        public int SetAstRoot(int lineNumber, Node node)
        {
            var procedure = GetStatement(lineNumber);
            if (procedure == null)
            {
                return -1;
            }
            else
            {
                procedure.AstRoot = node;
                return 0;
            }
        }
    }
}