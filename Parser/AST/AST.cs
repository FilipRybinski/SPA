using Parser.AST.Utils;
using Parser.Interfaces;
using Utils.Enums;

namespace Parser.AST;

public class Ast:IAst
{
     private static Ast? _instance;

        public static Ast? Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Ast();
                }
                return _instance;
            }
        }

        private Ast()
        {

        }
        public Node Root { get; set; }

        public Node CreateTNode(EntityType et)
        {
            return new Node(et);
        }

        public Node GetTNodeDeepCopy(Node node)
        {
            return new Node(node);
        }

        public NodeAttribute GetAttr(Node node)
        {
            return node.NodeAttribute;
        }

        public Node GetFirstChild(Node parent)
        {
            List<Node> childNodes = GetLinkedNodes(parent, LinkType.Child);
            return childNodes.FirstOrDefault()!;
        }

        public List<Node> GetFollowedBy(Node node)
        {
            return GetPrevLinkedNodes(node, LinkType.Follows);
        }

        public List<Node> GetFollowedStarBy(Node node)
        {
            List<Node> nodes = new List<Node>();
            return GetFollowedStarBy(node, nodes);
        }

        private List<Node> GetFollowedStarBy(Node node, List<Node> tempList)
        {
            foreach (var tnode in GetFollowedBy(node))
            {
                tempList.Add(tnode);
                GetFollowedStarBy(tnode, tempList);
            }
            return tempList;
        }

        public List<Node> GetFollows(Node node)
        {
            return GetLinkedNodes(node,LinkType.Follows);
        }

        public List<Node> GetFollowsStar(Node node)
        {
            List<Node> nodes = new List<Node>();
            return GetFollowsStar(node, nodes);
            
        }

        private List<Node> GetFollowsStar(Node node, List<Node> tempList)
        {
            foreach(Node tnode in GetFollows(node))
            {
                tempList.Add(tnode);
                GetFollowsStar(tnode, tempList);
            }
            return tempList;
        }

        public List<Node> GetLinkedNodes(Node node, LinkType linkType)
        {
            var nodes = node.Links.Where(i => i.Type == linkType).Select(i => i.LinkNode).ToList();
            return nodes;

        }

        public List<Node> GetPrevLinkedNodes(Node node, LinkType linkType)
        {
            var nodes = node.PrevLinks.Where(i => i.Type == linkType).Select(i => i.LinkNode).ToList();
            return nodes;

        }

        public Node GetNthChild(int nth, Node parent)
        {
            return GetLinkedNodes(parent, LinkType.Child).ElementAtOrDefault(nth)!;
        }

        public Node GetParent(Node node)
        {
            return GetLinkedNodes(node, LinkType.Parent).FirstOrDefault()!;
        }

        public List<Node> GetParentedBy(Node node)
        {
            return GetPrevLinkedNodes(node, LinkType.Parent);
        }

        public List<Node> GetParentedStarBy(Node node)
        {
            List<Node> nodes = new List<Node>();
            return GetParentedStarBy(node, nodes);
        }

        private List<Node> GetParentedStarBy(Node node, List<Node> tempList)
        {
            foreach (Node tnode in GetParentedBy(node))
            {
                tempList.Add(tnode);
                GetParentedStarBy(tnode, tempList);
            }
            return tempList;
        }


        public List<Node> GetParentStar(Node node)
        {
            List<Node> nodes = new List<Node>();
            Node tempNode = node;
            while(tempNode != null)
            {
                Node parentNode = GetParent(tempNode);
                if(parentNode != null)
                {
                    nodes.Add(parentNode);
                }
                tempNode = parentNode;
            }
            return nodes;
        }

        public Node GetRoot()
        {
            return Root;
        }

        public EntityType GetType(Node node)
        {
            return node.EntityType;
        }
        
        public bool IsFollowed(Node node1, Node node2)
        {
            return GetFollows(node2).Contains(node1);
        }

        public bool IsFollowedStar(Node node1, Node node2)
        {
            return GetFollowsStar(node2).Contains(node1);
        }

        public bool IsLinked(LinkType linkType, Node node1, Node node2)
        {
            return GetLinkedNodes(node1, linkType).Contains(node2);
        }

        public bool IsParent(Node parent, Node child)
        {
            return GetParent(child) == parent;
        }

        public bool IsParentStar(Node parent, Node child)
        {
            return GetParentStar(child).Contains(parent);

        }

        public void SetAttr(Node node, NodeAttribute nodeAttribute)
        {
            node.NodeAttribute = nodeAttribute;
        }

        public void SetChildOfLink(Node child, Node parent)
        {
            SetLink(LinkType.Child, parent, child);

        }

        public void SetFirstChild(Node parent, Node child)
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

        public void SetFollows(Node node1, Node node2)
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

        public void SetNthChild(int nth, Node parent, Node child)
        {
            if (parent.Links.Count() > nth)
            {
                if (parent.Links[nth - 1].Type == LinkType.Child)
                {
                    parent.Links[nth - 1].LinkNode = child;
                }
            }
            else if (parent.Links.Count() - 1 == nth)
            {
                parent.Links.Add(new Link(LinkType.Child, child));
            }
        }

        public void SetParent(Node child, Node parent)
        {
            SetLink(LinkType.Parent, child, parent);
            SetPrevLink(LinkType.Parent, parent, child);
        }

        public void SetRightSibling(Node nodeL, Node nodeR)
        {
            SetLink(LinkType.RightSibling, nodeL, nodeR);
            SetLink(LinkType.LeftSibling, nodeL, nodeR);
        }

        public void SetRoot(Node node)
        {
            Root = node;
        }

        public List<int> GetConstants(Node node)
        {
            var constans = new List<int>();
            var childs = GetLinkedNodes(node, LinkType.Child);
            if(childs != null)
                if(childs.Count > 1)
                    foreach(Node child in childs)
                    {
                        if(child.EntityType == EntityType.Constant)
                        {
                            var value = int.Parse("0");
                            constans.Add(value);
                        }
                        else
                            constans.AddRange(GetConstants(child));
                    }
            return constans.Distinct().ToList();
        }

}