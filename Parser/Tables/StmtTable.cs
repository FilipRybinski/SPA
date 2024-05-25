using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Interfaces;

namespace Parser.Tables
{
    public sealed class StmtTable : IStmtTable
    {
        private static StmtTable _instance = null;

        public static StmtTable Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StmtTable();
                }
                return _instance;
            }
        }
        public List<Statement> Statements { get; set; }
        private StmtTable()
        {
            Statements = new List<Statement>();
        }
        public Node GetAstRoot(int codeLine)
        {
            var proc = GetStmt(codeLine);
            return proc == null ? null : proc.AstRoot;
        }

        public int GetSize()
        {
            return Statements.Count();
        }

        public Statement GetStmt(int codeLine)
        {
            return Statements.Where(i => i.CodeLine == codeLine).FirstOrDefault();
        }

        public int InsertStmt(EntityType entityType, int codeLine)
        {
            if (Statements.Where(i => i.CodeLine == codeLine).Any())
            {
                return -1;
            }
            else
            {
                Statement variable = new Statement(entityType, codeLine);
                Statements.Add(variable);
                return 0;
            }
        }

        public int SetAstRoot(int codeLine, Node node)
        {
            var procedure = GetStmt(codeLine);
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
