using Parser.AST.Enums;

namespace Parser.AST.Utils;

public class LINK
{
    public LinkType LinkType { get; set; }
    public Node LinkNode { get; set; }

    public LINK(LinkType linkType, Node nodeToLink)
    {
        LinkNode = nodeToLink;
        LinkType = linkType;
    }
}