using Parser.AST.Enums;

namespace Parser.AST.Utils;

public class LINK
{
    public LinkTypeEnum LinkTypeEnum { get; set; }
    public TNODE LinkNode { get; set; }

    public LINK(LinkTypeEnum linkTypeEnum, TNODE nodeToLink)
    {
        LinkNode = nodeToLink;
        LinkTypeEnum = linkTypeEnum;
    }
}