using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables.Models;
using Utils.Enums;

namespace Parser.Tables
{
    public sealed class StatementTable : IStmtTable
    {
        private static StatementTable? _instance;

        private StatementTable()
        {
            StatementsList = new List<Statement?>();
        }

        public static IStmtTable? Instance
        {
            get { return _instance ??= new StatementTable(); }
        }

        public List<Statement?> StatementsList { get; set; }

        public Node FindAstRootNode(int lineNumber)
        {
            var stmt = FindStatement(lineNumber);
            return stmt == null ? null : stmt.AstRoot;
        }

        public List<Statement?> GetStatementsList()
        {
            return StatementsList;
        }

        public Statement? FindStatement(int lineNumber)
        {
            return StatementsList.FirstOrDefault(i => i.LineNumber == lineNumber)!;
        }

        public int InsertNewStatement(EntityType entityType, int lineNumber)
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

        public int AttachNewValueOfAstRoot(int lineNumber, Node node)
        {
            var procedure = FindStatement(lineNumber);
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