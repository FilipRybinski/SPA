using Parser.AST.Enums;

namespace Parser.AST.Utils;

public class TNODE
{
    public ATTR Attr { get; set; }
    public List<LINK> Links { get; set; }
    public List<LINK> PrevLinks { get; set; }
    public EntityTypeEnum EntityTypeEnum { get; set; }

    public TNODE(EntityTypeEnum entityTypeEnum)
    {
        Attr = new ATTR();
        PrevLinks = new List<LINK>();
        Links = new List<LINK>();
        EntityTypeEnum = entityTypeEnum;
    }

    public TNODE(TNODE node)
    {
        this.Links = node.Links;
        this.PrevLinks = node.PrevLinks;
        this.EntityTypeEnum = node.EntityTypeEnum;
        this.Attr = node.Attr;
    }
}