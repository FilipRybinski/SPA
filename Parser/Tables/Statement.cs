using Parser.AST.Enums;
using Parser.AST.Utils;

namespace Parser.Tables
{
    public class Statement
    {
        public int LineNumber { get; set; }
        public EntityType StmtType { get; set; }
        public Node AstRoot { get; set; }
        public Dictionary<int, bool> ModifiesList { get; set; }
        public Dictionary<int, bool> UsesList { get; set; }
        public Statement(EntityType stmtType, int lineNumber)
        {
            if (!(stmtType == EntityType.Assign || stmtType == EntityType.If || stmtType == EntityType.While || stmtType == EntityType.Call))
            {
                throw new InvalidOperationException();
            }
            LineNumber = lineNumber;
            StmtType = stmtType;
            ModifiesList = new Dictionary<int, bool>();
            UsesList = new Dictionary<int, bool>();

        }
    }
}
