using Parser.AST.Utils;

namespace Parser.Tables
{
    public class Procedure
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public Node AstRoot { get; set; }
        public Dictionary<int, bool> ModifiesList { get; set; }
        public Dictionary<int, bool> UsesList { get; set; }
        public Procedure(string name)
        {
            Name = name;
            ModifiesList = new Dictionary<int, bool>();
            UsesList = new Dictionary<int, bool>();
        }


    }
}
