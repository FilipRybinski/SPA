using Parser.AST.Enums;

namespace Parser.AST.Utils;

public class Node
{
    public NodeAttribute NodeAttribute { get; set; }
    public List<LINK> Links { get; set; }
    public List<LINK> PrevLinks { get; set; }
    public EntityType EntityType { get; set; }

    public Node(EntityType entityType)
    {
        NodeAttribute = new NodeAttribute();
        PrevLinks = new List<LINK>();
        Links = new List<LINK>();
        EntityType = entityType;
    }

    public Node(Node node)
    {
        this.Links = node.Links;
        this.PrevLinks = node.PrevLinks;
        this.EntityType = node.EntityType;
        this.NodeAttribute = node.NodeAttribute;
    }
}