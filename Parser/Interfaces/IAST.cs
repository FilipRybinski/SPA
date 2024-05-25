using Parser.AST.Enums;
using Parser.AST.Utils;

namespace Parser.Interfaces;

public interface IAST
{
        TNODE CreateTNode(EntityTypeEnum et);
        TNODE GetTNodeDeepCopy(TNODE node);
        void SetRoot(TNODE node);
        void SetAttr(TNODE node, ATTR attr);
        void SetFirstChild(TNODE parent, TNODE child);
        void SetRightSibling(TNODE nodeL, TNODE nodeR);
        void SetChildOfLink(TNODE child, TNODE parent);
        void SetLeftSibling(TNODE nodeL, TNODE nodeR);
        void SetLink(LinkTypeEnum linkTypeEnum, TNODE node1, TNODE node2);
        void SetPrevLink(LinkTypeEnum linkTypeEnum, TNODE node1, TNODE node2);
        void SetNthChild(int nth, TNODE parent, TNODE child);
        TNODE GetNthChild(int nth, TNODE parent);
        TNODE GetRoot();
        EntityTypeEnum GetType(TNODE node);
        ATTR GetAttr(TNODE node);
        TNODE GetFirstChild(TNODE parent);
        List<TNODE> GetLinkedNodes(TNODE node, LinkTypeEnum linkTypeEnum);
        List<TNODE> GetPrevLinkedNodes(TNODE node, LinkTypeEnum linkTypeEnum);
        bool IsLinked(LinkTypeEnum linkTypeEnum, TNODE node1, TNODE node2);
        void SetParent(TNODE parent, TNODE child);
        TNODE GetParent(TNODE node);
        List<TNODE> GetParentedBy(TNODE node);
        List<TNODE> GetParentStar(TNODE node);
        List<TNODE> GetParentedStarBy(TNODE node);
        void SetFollows(TNODE node1, TNODE node2);
        List<TNODE> GetFollows(TNODE node);
        List<TNODE> GetFollowsStar(TNODE node);
        List<TNODE> GetFollowedBy(TNODE node);
        List<TNODE> GetFollowedStarBy(TNODE node);
        bool IsFollowed(TNODE node1, TNODE node2);
        bool IsFollowedStar(TNODE node1, TNODE node2);
        bool IsParent(TNODE parent, TNODE child);
        bool IsParentStar(TNODE parent, TNODE child);
}