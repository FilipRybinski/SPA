using KellermanSoftware.CompareNetObjects;
using Utils.Enums;

namespace Parser.AST.Utils;

public class Node
{
    public NodeAttribute NodeAttribute { get; set; }
    public List<Link> Links { get; }
    public List<Link> PrevLinks { get; }
    public EntityType EntityType { get; }

    public Node(EntityType entityType)
    {
        NodeAttribute = new NodeAttribute();
        PrevLinks = new List<Link>();
        Links = new List<Link>();
        EntityType = entityType;
    }

    public Node(Node node)
    {
        Links = node.Links;
        PrevLinks = node.PrevLinks;
        EntityType = node.EntityType;
        NodeAttribute = node.NodeAttribute;
    }
}