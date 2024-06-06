using Utils.Enums;

namespace Parser.AST.Utils;

public class Link
{
    public LinkType Type { get; set; }
    public Node LinkNode { get; set; }

    public Link(LinkType type, Node nodeToLink)
    {
        LinkNode = nodeToLink;
        Type = type;
    }
}