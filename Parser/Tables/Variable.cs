namespace Parser.Tables
{
    public class Variable
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public Variable(string name)
        {
            Name = name;
        }

    }
}
