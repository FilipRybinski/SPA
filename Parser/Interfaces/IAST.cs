using Parser.AST.Utils;
using Utils.Enums;

namespace Parser.Interfaces;

public interface IAst
{
        Node Root { set; }
        Node CreateTNode(EntityType et);
        Node GetTNodeDeepCopy(Node node);
        void SetRoot(Node node);
        void SetChildOfLink(Node child, Node parent);
        Node? GetChildOfIdx(int idx, Node parent);
        Node? GetFirstChild(Node parent);
        List<Node> GetLinkedNodes(Node node, LinkType linkType);
        void SetParent(Node parent, Node child);
        Node? GetParent(Node node);
        void SetFollows(Node node1, Node node2);
        bool IsFollowed(Node node1, Node node2);
        bool IsFollowedStar(Node node1, Node node2);
        bool IsParent(Node parent, Node child);
        bool IsParentStar(Node parent, Node child);
        List<int> GetConstants(Node node);
}