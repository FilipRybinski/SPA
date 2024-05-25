using Parser.AST.Enums;
using Parser.AST.Utils;

namespace Parser.Tables
{
    public class Statement
    {
        public int CodeLine { get; set; }
        public EntityType Type { get; set; }
        public Node AstRoot { get; set; }
        public Dictionary<int, bool> ModifiesList { get; set; }
        public Dictionary<int, bool> UsesList { get; set; }
        public Statement(EntityType entityType, int codeLine)
        {
            if (!(entityType == EntityType.Assign || entityType == EntityType.If || entityType == EntityType.While || entityType == EntityType.Call))
            {
                throw new InvalidOperationException();
            }
            CodeLine = codeLine;
            Type = entityType;
            ModifiesList = new Dictionary<int, bool>();
            UsesList = new Dictionary<int, bool>();

        }
    }
}
