using Parser.AST.Utils;
using Utils.Enums;

namespace Parser.Interfaces;

public interface IAst
{
        Node Root { set; }
        Node GenerateNode(EntityType et);
        Node ReplicateNode(Node node);
        void AssignToRootNode(Node node);
        void AttachChildToLinkType(Node child, Node parent);
        Node? FindChildByIndex(int idx, Node parent);
        Node? ReturnFirstChild(Node parent);
        List<Node> FindLinkedNodes(Node node, LinkType linkType);
        void AttachValueToParentNode(Node parent, Node child);
        Node? GetValueOfParentNode(Node node);
        void AssignFollows(Node node1, Node node2);
        bool CheckFollowed(Node node1, Node node2);
        bool CheckFollowedStar(Node node1, Node node2);
        bool CheckParent(Node parent, Node child);
        bool CheckParentStar(Node parent, Node child);
        List<int> GetReadOnlyVariables(Node node);
}