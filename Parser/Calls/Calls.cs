using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;

namespace Parser.Calls;

public class Calls : ICalls
{
      private static Calls _singletonInstance = null;

        public static Calls Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new Calls();
                }
                return _singletonInstance;
            }
        }

        private Calls()
        {

        }
        public List<Procedure> GetCalledBy(string proc)
        {
            List<Procedure> procedures = new List<Procedure>();
            foreach (Procedure procedure in ProcedureTable.Instance.ProceduresList)
            {
                if (IsCalls(procedure.Identifier, proc))
                {
                    procedures.Add(procedure);
                }
            }
            return procedures;
        }

        public List<Procedure> GetCalledByStar(string proc)
        {
            List<Procedure> procedures = new List<Procedure>();
            foreach(Procedure procedure in ProcedureTable.Instance.ProceduresList)
            {
                if (IsCallsStar(procedure.Identifier, proc))
                {
                    procedures.Add(procedure);
                }
            }
            return procedures;
        }

        public List<Procedure> GetCalls(List<Procedure> procedures, Node stmtNode)
        {
            

            List<string> procNames = AST.AST.Instance
                .GetLinkedNodes(stmtNode, LinkType.Child)
                .Where(i => i.EntityType == EntityType.Call)
                .Select(i => i.NodeAttribute.Name)
                .ToList();
            foreach(string proce in procNames)
            {
                Procedure findProcedure = ProcedureTable.Instance.GetProcedure(proce);
                if(findProcedure != null)
                {
                    procedures.Add(findProcedure);
                }
            }


            List<Node> ifOrWhileNodes = AST.AST.Instance
                .GetLinkedNodes(stmtNode, LinkType.Child)
                .Where(i => i.EntityType == EntityType.While || i.EntityType == EntityType.If).ToList();

            foreach(var node in ifOrWhileNodes)
            {
                List<Node> stmtLstNodes = AST.AST.Instance
                .GetLinkedNodes(node, LinkType.Child)
                .Where(i => i.EntityType == EntityType.Stmtlist).ToList();


                foreach(var stmtL in stmtLstNodes)
                {
                    GetCalls(procedures,stmtL);
                }

            }


            return procedures;
                
        }


        public List<Procedure> GetCalls(string proc)
        {
            List<Procedure> procedures = new List<Procedure>();
            Node procNode = ProcedureTable.Instance.GetAstRoot(proc);
            Node stmtLstChild = AST.AST.Instance.GetFirstChild(procNode);
            GetCalls(procedures, stmtLstChild);

            
            return procedures;
        }

        public List<Procedure> GetCallsStar(string proc)
        {
            List<Procedure> procedures = new List<Procedure>();
            return GetCallsStar(proc, procedures);
        }

        private List<Procedure> GetCallsStar(string proc,List<Procedure> procedures)
        {
            foreach (Procedure procedure in GetCalls(proc))
            {
                procedures.Add(procedure);
                GetCallsStar(procedure.Identifier, procedures);
            }
            return procedures;
        }

        public bool IsCalls(string proc1, string proc2)
        {
            return GetCalls(proc1)
                .Where(i => i.Identifier == proc2)
                .Any();
        }

        public bool IsCallsStar(string proc1, string proc2)
        {
            return GetCallsStar(proc1)
                .Where(i => i.Identifier == proc2)
                .Any();
        }
        public bool IsCalledBy(string proc1, string proc2)
        {
            return GetCalledBy(proc1)
                .Where(i => i.Identifier == proc2)
                .Any();
        }

        public bool IsCalledStarBy(string proc1, string proc2)
        {
            return GetCalledByStar(proc1)
                .Where(i => i.Identifier == proc2)
                .Any();
        }
}