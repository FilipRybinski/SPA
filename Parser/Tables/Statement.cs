using Parser.AST.Enums;
using Parser.AST.Utils;

namespace Parser.Tables
{
    public class Statement
    {
        public int CodeLine { get; set; }
        public EntityTypeEnum Type { get; set; }
        public TNODE AstRoot { get; set; }
        public Dictionary<int, bool> ModifiesList { get; set; }
        public Dictionary<int, bool> UsesList { get; set; }
        public Statement(EntityTypeEnum entityTypeEnum, int codeLine)
        {
            if (!(entityTypeEnum == EntityTypeEnum.Assign || entityTypeEnum == EntityTypeEnum.If || entityTypeEnum == EntityTypeEnum.While || entityTypeEnum == EntityTypeEnum.Call))
            {
                throw new InvalidOperationException();
            }
            CodeLine = codeLine;
            Type = entityTypeEnum;
            ModifiesList = new Dictionary<int, bool>();
            UsesList = new Dictionary<int, bool>();

        }
    }
}
