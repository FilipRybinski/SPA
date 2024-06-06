using Parser.AST.Utils;

namespace Parser.Tables.Models
{
    public class Procedure
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public Node AstNodeRoot { get; set; }
        public Dictionary<int, bool> ModifiesList { get; set; }
        public Dictionary<int, bool> UsesList { get; set; }

        public Procedure(string identifier)
        {
            Identifier = identifier;
            ModifiesList = new Dictionary<int, bool>();
            UsesList = new Dictionary<int, bool>();
        }
    }
}