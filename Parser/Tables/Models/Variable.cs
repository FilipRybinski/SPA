namespace Parser.Tables.Models
{
    public class Variable
    {
        public int Id { get; set; }
        public string Identifier { get; set; }

        public Variable(string identifier)
        {
            Identifier = identifier;
        }
    }
}