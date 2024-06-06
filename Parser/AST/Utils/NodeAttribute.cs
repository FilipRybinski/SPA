using KellermanSoftware.CompareNetObjects;

namespace Parser.AST.Utils;

public class NodeAttribute
{
    public string Name { get; set; } = "DEFAULT";

    public override bool Equals(object? obj)
    {
        if (obj is not NodeAttribute comp) return false;
        var compareLogic = new CompareLogic();
        return compareLogic.Compare(comp, this).AreEqual;
    }
}