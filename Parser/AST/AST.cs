using Parser.AST.Utils;
using Parser.Interfaces;
using Utils.Enums;

namespace Parser.AST;

public class Ast : IAst
{
    private static Ast? _instance;

    public static IAst? Instance
    {
        get{return _instance ??= new Ast();}
    }

    public Node? Root { get; set; }

    public Node GenerateNode(EntityType et) => new(et);

    public Node ReplicateNode(Node node) => new(node);

    public NodeAttribute GetAttr(Node node) => node.NodeAttribute;

    public Node? ReturnFirstChild(Node parent) => FindLinkedNodes(parent, LinkType.Child).FirstOrDefault();

    public List<Node> GetFollowedBy(Node node) => GetPrevLinkedNodes(node, LinkType.Follows);

    public List<Node> GetFollowedStarBy(Node node) => GetFollowedStarBy(node, new List<Node>());

    private List<Node> GetFollowedStarBy(Node node, List<Node> tempList)
    {
        foreach (var followedNode in GetFollowedBy(node))
        {
            tempList.Add(followedNode);
            GetFollowedStarBy(followedNode, tempList);
        }

        return tempList;
    }

    public List<Node> GetFollows(Node node) => FindLinkedNodes(node, LinkType.Follows);

    public List<Node> GetFollowsStar(Node node)
    {
        var nodes = new List<Node>();
        return GetFollowsStar(node, nodes);
    }

    private List<Node> GetFollowsStar(Node node, List<Node> tempList)
    {
        foreach (var followingNode in GetFollows(node))
        {
            tempList.Add(followingNode);
            GetFollowsStar(followingNode, tempList);
        }

        return tempList;
    }

    public List<Node> FindLinkedNodes(Node node, LinkType linkType) =>
        node == null ? new() :
        node.Links.Where(i => i.Type == linkType).Select(i => i.LinkNode).ToList();

    public List<Node> GetPrevLinkedNodes(Node node, LinkType linkType) =>
        node.PrevLinks.Where(i => i.Type == linkType).Select(i => i.LinkNode).ToList();

    public Node? FindChildByIndex(int idx, Node parent) =>
        FindLinkedNodes(parent, LinkType.Child).ElementAtOrDefault(idx);

    public Node? GetValueOfParentNode(Node node)
        => FindLinkedNodes(node, LinkType.Parent).FirstOrDefault();


    public List<Node> GetParentedBy(Node node)
        => GetPrevLinkedNodes(node, LinkType.Parent);


    public List<Node> GetParentedStarBy(Node node) => GetParentedStarBy(node, new List<Node>());

    private List<Node> GetParentedStarBy(Node node, List<Node> tempList)
    {
        foreach (var parent in GetParentedBy(node))
        {
            tempList.Add(parent);
            GetParentedStarBy(parent, tempList);
        }

        return tempList;
    }


    public List<Node> GetParentStar(Node node)
    {
        var nodes = new List<Node>();
        var tempNode = node;
        while (true)
        {
            var parentNode = GetValueOfParentNode(tempNode);

            if (parentNode is null)
                break;

            nodes.Add(parentNode);
            tempNode = parentNode;
        }

        return nodes;
    }

    public Node GetRoot()
        => Root;

    public EntityType GetType(Node node)
        => node.EntityType;


    public bool CheckFollowed(Node node1, Node node2)
        => GetFollows(node2).Contains(node1);


    public bool CheckFollowedStar(Node node1, Node node2)
        => GetFollowsStar(node2).Contains(node1);

    public bool IsLinked(LinkType linkType, Node node1, Node node2)
        => FindLinkedNodes(node1, linkType).Contains(node2);

    public bool CheckParent(Node parent, Node child) => GetValueOfParentNode(child)?.Equals(parent) ?? false;

    public bool CheckParentStar(Node parent, Node child) => GetParentStar(child).Contains(parent);

    public void SetAttr(Node node, NodeAttribute nodeAttribute)
    {
        node.NodeAttribute = nodeAttribute;
    }

    public void AttachChildToLinkType(Node child, Node parent)
    {
        SetLink(LinkType.Child, parent, child);
    }

    public void SetFirstChild(Node parent, Node child)
    {
        if (ReturnFirstChild(parent) == null)
            AttachChildToLinkType(child, parent);
        else
            SetChildOfIdx(1, parent, child);
    }

    public void AssignFollows(Node node1, Node node2)
    {
        SetLink(LinkType.Follows, node2, node1);
        SetPrevLink(LinkType.Follows, node1, node2);
    }

    public void SetLeftSibling(Node nodeL, Node nodeR)
    {
        SetLink(LinkType.LeftSibling, nodeL, nodeR);
        SetLink(LinkType.RightSibling, nodeL, nodeR);
    }

    public void SetLink(LinkType linkType, Node node1, Node node2)
    {
        node1.Links.Add(new Link(linkType, node2));
    }

    public void SetPrevLink(LinkType linkType, Node node1, Node node2)
    {
        node1.PrevLinks.Add(new Link(linkType, node2));
    }

    public void SetChildOfIdx(int idx, Node parent, Node child)
    {
        if (parent.Links.Count > idx)
        {
            if (parent.Links[idx - 1].Type == LinkType.Child)
                parent.Links[idx - 1].LinkNode = child;
        }
        else if (parent.Links.Count - 1 == idx)
            parent.Links.Add(new Link(LinkType.Child, child));
    }

    public void AttachValueToParentNode(Node child, Node parent)
    {
        SetLink(LinkType.Parent, child, parent);
        SetPrevLink(LinkType.Parent, parent, child);
    }

    public void SetRightSibling(Node nodeL, Node nodeR)
    {
        SetLink(LinkType.RightSibling, nodeL, nodeR);
        SetLink(LinkType.LeftSibling, nodeL, nodeR);
    }

    public void AssignToRootNode(Node node)
    {
        Root = node;
    }

    public List<int> GetReadOnlyVariables(Node node)
    {
        var constants = new List<int>();
        var children = FindLinkedNodes(node, LinkType.Child);
        if (children.Count <= 1) return constants.Distinct().ToList();
        foreach (var child in children)
        {
            if (child.EntityType == EntityType.Constant)
                constants.Add((int)EntityType.Constant);
            else
                constants.AddRange(GetReadOnlyVariables(child));
        }

        return constants.Distinct().ToList();
    }
}