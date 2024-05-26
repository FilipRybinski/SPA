using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Interfaces;

namespace Parser.Tables
{
    public sealed class StatementTable : IStmtTable
    {
        private static StatementTable _singletonInstance = null;

        public static StatementTable Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new StatementTable();
                }
                return _singletonInstance;
            }
        }
        public List<Statement> StatementsList { get; set; }
        private StatementTable()
        {
            StatementsList = new List<Statement>();
        }
        public Node GetAstRoot(int lineNumber)
        {
            var stmt = GetStatement(lineNumber);
            return stmt == null ? null : stmt.AstRoot;
        }

        public int GetSize()
        {
            return StatementsList.Count();
        }

        public Statement GetStatement(int lineNumber)
        {
            return StatementsList.Where(i => i.LineNumber == lineNumber).FirstOrDefault();
        }

        public int AddStatement(EntityType entityType, int lineNumber)
        {
            if (StatementsList.Where(i => i.LineNumber == lineNumber).Any())
            {
                return -1;
            }
            else
            {
                Statement newStmt = new Statement(entityType, lineNumber);
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
