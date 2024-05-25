using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Interfaces;

namespace Parser.AST;

public class AST:IAST
{
     private static AST _instance = null;

        public static AST Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AST();
                }
                return _instance;
            }
        }

        private AST()
        {

        }
        public TNODE Root { get; set; }

        public TNODE CreateTNode(EntityTypeEnum et)
        {
            return new TNODE(et);
        }

        public TNODE GetTNodeDeepCopy(TNODE node)
        {
            return new TNODE(node);
        }

        public ATTR GetAttr(TNODE node)
        {
            return node.Attr;
        }

        public TNODE GetFirstChild(TNODE parent)
        {
            List<TNODE> childNodes = GetLinkedNodes(parent, LinkTypeEnum.Child);
            return childNodes.FirstOrDefault();
        }

        public List<TNODE> GetFollowedBy(TNODE node)
        {
            return GetPrevLinkedNodes(node, LinkTypeEnum.Follows);
        }

        public List<TNODE> GetFollowedStarBy(TNODE node)
        {
            List<TNODE> nodes = new List<TNODE>();
            return GetFollowedStarBy(node, nodes);
        }

        private List<TNODE> GetFollowedStarBy(TNODE node, List<TNODE> tempList)
        {
            foreach (TNODE tnode in GetFollowedBy(node))
            {
                tempList.Add(tnode);
                GetFollowedStarBy(tnode, tempList);
            }
            return tempList;
        }

        public List<TNODE> GetFollows(TNODE node)
        {
            return GetLinkedNodes(node,LinkTypeEnum.Follows);
        }

        public List<TNODE> GetFollowsStar(TNODE node)
        {
            List<TNODE> nodes = new List<TNODE>();
            return GetFollowsStar(node, nodes);
            
        }

        private List<TNODE> GetFollowsStar(TNODE node, List<TNODE> tempList)
        {
            foreach(TNODE tnode in GetFollows(node))
            {
                tempList.Add(tnode);
                GetFollowsStar(tnode, tempList);
            }
            return tempList;
        }

        public List<TNODE> GetLinkedNodes(TNODE node, LinkTypeEnum linkTypeEnum)
        {
            List<TNODE> nodes = new List<TNODE>();
            nodes = node.Links.Where(i => i.LinkTypeEnum == linkTypeEnum).Select(i => i.LinkNode).ToList();
            return nodes;

        }

        public List<TNODE> GetPrevLinkedNodes(TNODE node, LinkTypeEnum linkTypeEnum)
        {
            List<TNODE> nodes = new List<TNODE>();
            nodes = node.PrevLinks.Where(i => i.LinkTypeEnum == linkTypeEnum).Select(i => i.LinkNode).ToList();
            return nodes;

        }

        public TNODE GetNthChild(int nth, TNODE parent)
        {
            return GetLinkedNodes(parent, LinkTypeEnum.Child).ElementAtOrDefault(nth);
        }

        public TNODE GetParent(TNODE node)
        {
            return GetLinkedNodes(node, LinkTypeEnum.Parent).FirstOrDefault();
        }

        public List<TNODE> GetParentedBy(TNODE node)
        {
            return GetPrevLinkedNodes(node, LinkTypeEnum.Parent);
        }

        public List<TNODE> GetParentedStarBy(TNODE node)
        {
            List<TNODE> nodes = new List<TNODE>();
            return GetParentedStarBy(node, nodes);
        }

        private List<TNODE> GetParentedStarBy(TNODE node, List<TNODE> tempList)
        {
            foreach (TNODE tnode in GetParentedBy(node))
            {
                tempList.Add(tnode);
                GetParentedStarBy(tnode, tempList);
            }
            return tempList;
        }


        public List<TNODE> GetParentStar(TNODE node)
        {
            List<TNODE> nodes = new List<TNODE>();
            TNODE tempNode = node;
            while(tempNode != null)
            {
                TNODE parentNode = GetParent(tempNode);
                if(parentNode != null)
                {
                    nodes.Add(parentNode);
                }
                tempNode = parentNode;
            }
            return nodes;
        }

        public TNODE GetRoot()
        {
            return Root;
        }

        public EntityTypeEnum GetType(TNODE node)
        {
            return node.EntityTypeEnum;
        }

        //Does node2 follow node1
        public bool IsFollowed(TNODE node1, TNODE node2)
        {
            return GetFollows(node2).Contains(node1);
        }

        public bool IsFollowedStar(TNODE node1, TNODE node2)
        {
            return GetFollowsStar(node2).Contains(node1);
        }

        public bool IsLinked(LinkTypeEnum linkTypeEnum, TNODE node1, TNODE node2)
        {
            return GetLinkedNodes(node1, linkTypeEnum).Contains(node2);
        }

        public bool IsParent(TNODE parent, TNODE child)
        {
            return GetParent(child) == parent;
        }

        public bool IsParentStar(TNODE parent, TNODE child)
        {
            return GetParentStar(child).Contains(parent);

        }

        public void SetAttr(TNODE node, ATTR attr)
        {
            node.Attr = attr;
        }

        public void SetChildOfLink(TNODE child, TNODE parent)
        {
            SetLink(LinkTypeEnum.Child, parent, child);

        }

        public void SetFirstChild(TNODE parent, TNODE child)
        {
            
            if(GetFirstChild(parent) == null)
            {
                SetChildOfLink(child, parent);
            }
            else
            {
                SetNthChild(1, parent, child);
            }
    
        }

        public void SetFollows(TNODE node1, TNODE node2)
        {
            SetLink(LinkTypeEnum.Follows, node2, node1);
            SetPrevLink(LinkTypeEnum.Follows, node1, node2);
        }

        public void SetLeftSibling(TNODE nodeL, TNODE nodeR)
        {
            SetLink(LinkTypeEnum.LeftSibling, nodeL, nodeR);
            SetLink(LinkTypeEnum.RightSibling, nodeL, nodeR);

        }

        public void SetLink(LinkTypeEnum linkTypeEnum, TNODE node1, TNODE node2)
        {
            node1.Links.Add(new LINK(linkTypeEnum, node2));
        }

        public void SetPrevLink(LinkTypeEnum linkTypeEnum, TNODE node1, TNODE node2)
        {
            node1.PrevLinks.Add(new LINK(linkTypeEnum, node2));
        }

        public void SetNthChild(int nth, TNODE parent, TNODE child)
        {
            if (parent.Links.Count() > nth)
            {
                if (parent.Links[nth - 1].LinkTypeEnum == LinkTypeEnum.Child)
                {
                    parent.Links[nth - 1].LinkNode = child;
                }
            }
            else if (parent.Links.Count() - 1 == nth)
            {
                parent.Links.Add(new LINK(LinkTypeEnum.Child, child));
            }
        }

        public void SetParent(TNODE child, TNODE parent)
        {
            SetLink(LinkTypeEnum.Parent, child, parent);
            SetPrevLink(LinkTypeEnum.Parent, parent, child);
        }

        public void SetRightSibling(TNODE nodeL, TNODE nodeR)
        {
            SetLink(LinkTypeEnum.RightSibling, nodeL, nodeR);
            SetLink(LinkTypeEnum.LeftSibling, nodeL, nodeR);
        }

        public void SetRoot(TNODE node)
        {
            Root = node;
        }

        public List<int> GetConstants(TNODE node)
        {
            List<int> constans = new List<int>();
            List<TNODE> childs = GetLinkedNodes(node, LinkTypeEnum.Child);
            if(childs != null)
                if(childs.Count > 1)
                    foreach(TNODE child in childs)
                    {
                        if(child.EntityTypeEnum == EntityTypeEnum.Constant)
                        {
                            //int value = Int32.Parse(child.Attr.Name);
                            int value = Int32.Parse("0");
                            constans.Add(value);
                        }
                        else
                            constans.AddRange(GetConstants(child));
                    }
            return constans.Distinct().ToList();
        }

}