using Parser.AST.Utils;
using Parser.Interfaces;
using Utils.Enums;

namespace Parser.AST;

public class Ast : IAst
{
    private static Ast? _instance;

    public static IAst? Instance
    {
        get { return _instance ??= new Ast(); }
    }

    public Node? Root { get; set; }

    public Node GenerateNode(EntityType et) => new(et);

    public Node ReplicateNode(Node node) => new(node);

    public Node? ReturnFirstChild(Node parent) => FindLinkedNodes(parent, LinkType.Child).FirstOrDefault();

    public List<Node> FindLinkedNodes(Node node, LinkType linkType) =>
        node == null ? new() : node.Links.Where(i => i.Type == linkType).Select(i => i.LinkNode).ToList();

    public Node? FindChildByIndex(int idx, Node parent) =>
        FindLinkedNodes(parent, LinkType.Child).ElementAtOrDefault(idx);

    public Node? GetValueOfParentNode(Node node)
        => FindLinkedNodes(node, LinkType.Parent).FirstOrDefault();

    public bool CheckFollowed(Node node1, Node node2)
        => FindFollows(node2).Contains(node1);


    public bool CheckFollowedStar(Node node1, Node node2)
        => FindFolowsStar(node2).Contains(node1);

    public bool CheckParent(Node parent, Node child) => GetValueOfParentNode(child)?.Equals(parent) ?? false;

    public bool CheckParentStar(Node parent, Node child) => FindParentStar(child).Contains(parent);

    public void AttachChildToLinkType(Node child, Node parent)
    {
        AttachValueLinkType(LinkType.Child, parent, child);
    }

    public void AssignFollows(Node node1, Node node2)
    {
        AttachValueLinkType(LinkType.Follows, node2, node1);
        AttachValueOfPreviousLink(LinkType.Follows, node1, node2);
    }

    public void AttachValueToParentNode(Node child, Node parent)
    {
        AttachValueLinkType(LinkType.Parent, child, parent);
        AttachValueOfPreviousLink(LinkType.Parent, parent, child);
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

    private List<Node> FindFollowedBy(Node node) => FindPevNodeLinked(node, LinkType.Follows);

    private List<Node> FindFollowsStar(Node node, List<Node> tempList)
    {
        foreach (var followedNode in FindFollowedBy(node))
        {
            tempList.Add(followedNode);
            FindFollowsStar(followedNode, tempList);
        }

        return tempList;
    }

    private List<Node> FindFollows(Node node) => FindLinkedNodes(node, LinkType.Follows);

    private List<Node> FindFolowsStar(Node node)
    {
        var nodes = new List<Node>();
        return FindFolowsStar(node, nodes);
    }

    private List<Node> FindFolowsStar(Node node, List<Node> tempList)
    {
        foreach (var followingNode in FindFollows(node))
        {
            tempList.Add(followingNode);
            FindFolowsStar(followingNode, tempList);
        }

        return tempList;
    }

    private List<Node> FindPevNodeLinked(Node node, LinkType linkType) =>
        node.PrevLinks.Where(i => i.Type == linkType).Select(i => i.LinkNode).ToList();

    private List<Node> FindParentStar(Node node)
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

    public bool IsLinked(LinkType linkType, Node node1, Node node2)
        => FindLinkedNodes(node1, linkType).Contains(node2);

    private void AttachValueLinkType(LinkType linkType, Node node1, Node node2)
    {
        node1.Links.Add(new Link(linkType, node2));
    }

    private void AttachValueOfPreviousLink(LinkType linkType, Node node1, Node node2)
    {
        node1.PrevLinks.Add(new Link(linkType, node2));
    }

    private void AttachNewValueOfChildIndex(int idx, Node parent, Node child)
    {
        if (parent.Links.Count > idx)
        {
            if (parent.Links[idx - 1].Type == LinkType.Child)
                parent.Links[idx - 1].LinkNode = child;
        }
        else if (parent.Links.Count - 1 == idx)
            parent.Links.Add(new Link(LinkType.Child, child));
    }
}