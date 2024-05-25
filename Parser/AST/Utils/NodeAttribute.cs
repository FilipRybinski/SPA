namespace Parser.AST.Utils;

public class NodeAttribute
{
    private const string DEFAULT = "DEFAULT";
    public string Name { get; set; }

    public NodeAttribute()
    {
        Name = DEFAULT;
    }
}