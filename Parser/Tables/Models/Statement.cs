using Parser.AST.Utils;
using Utils.Enums;

namespace Parser.Tables.Models
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
            if (stmtType is not (EntityType.Assign or EntityType.If or EntityType.While or EntityType.Call))
                throw new InvalidOperationException();

            LineNumber = lineNumber;
            StmtType = stmtType;
            ModifiesList = new Dictionary<int, bool>();
            UsesList = new Dictionary<int, bool>();
        }
    }
}