using Parser.AST.Utils;
using Utils.Enums;

namespace Parser.Interfaces;

public interface IAst
{
        Node CreateTNode(EntityType et);
        Node GetTNodeDeepCopy(Node node);
        void SetRoot(Node node);
        void SetAttr(Node node, NodeAttribute nodeAttribute);
        void SetFirstChild(Node parent, Node child);
        void SetRightSibling(Node nodeL, Node nodeR);
        void SetChildOfLink(Node child, Node parent);
        void SetLeftSibling(Node nodeL, Node nodeR);
        void SetLink(LinkType linkType, Node node1, Node node2);
        void SetPrevLink(LinkType linkType, Node node1, Node node2);
        void SetNthChild(int nth, Node parent, Node child);
        Node GetNthChild(int nth, Node parent);
        Node GetRoot();
        EntityType GetType(Node node);
        NodeAttribute GetAttr(Node node);
        Node GetFirstChild(Node parent);
        List<Node> GetLinkedNodes(Node node, LinkType linkType);
        List<Node> GetPrevLinkedNodes(Node node, LinkType linkType);
        bool IsLinked(LinkType linkType, Node node1, Node node2);
        void SetParent(Node parent, Node child);
        Node GetParent(Node node);
        List<Node> GetParentedBy(Node node);
        List<Node> GetParentStar(Node node);
        List<Node> GetParentedStarBy(Node node);
        void SetFollows(Node node1, Node node2);
        List<Node> GetFollows(Node node);
        List<Node> GetFollowsStar(Node node);
        List<Node> GetFollowedBy(Node node);
        List<Node> GetFollowedStarBy(Node node);
        bool IsFollowed(Node node1, Node node2);
        bool IsFollowedStar(Node node1, Node node2);
        bool IsParent(Node parent, Node child);
        bool IsParentStar(Node parent, Node child);
}