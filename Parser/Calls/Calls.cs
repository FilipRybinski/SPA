using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;

namespace Parser.Calls;

public class Calls : ICalls
{
      private static Calls _instance = null;

        public static Calls Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Calls();
                }
                return _instance;
            }
        }

        private Calls()
        {

        }
        public List<Procedure> GetCalledBy(string proc)
        {
            List<Procedure> procedures = new List<Procedure>();
            foreach (Procedure procedure in ProcTable.Instance.Procedures)
            {
                if (IsCalls(procedure.Name, proc))
                {
                    procedures.Add(procedure);
                }
            }
            return procedures;
        }

        public List<Procedure> GetCalledByStar(string proc)
        {
            List<Procedure> procedures = new List<Procedure>();
            foreach(Procedure procedure in ProcTable.Instance.Procedures)
            {
                if (IsCallsStar(procedure.Name, proc))
                {
                    procedures.Add(procedure);
                }
            }
            return procedures;
        }

        public List<Procedure> GetCalls(List<Procedure> procedures, TNODE stmtNode)
        {
            

            List<string> procNames = AST.AST.Instance
                .GetLinkedNodes(stmtNode, LinkTypeEnum.Child)
                .Where(i => i.EntityTypeEnum == EntityTypeEnum.Call)
                .Select(i => i.Attr.Name)
                .ToList();
            foreach(string proce in procNames)
            {
                Procedure findProcedure = ProcTable.Instance.GetProc(proce);
                if(findProcedure != null)
                {
                    procedures.Add(findProcedure);
                }
            }


            List<TNODE> ifOrWhileNodes = AST.AST.Instance
                .GetLinkedNodes(stmtNode, LinkTypeEnum.Child)
                .Where(i => i.EntityTypeEnum == EntityTypeEnum.While || i.EntityTypeEnum == EntityTypeEnum.If).ToList();

            foreach(var node in ifOrWhileNodes)
            {
                List<TNODE> stmtLstNodes = AST.AST.Instance
                .GetLinkedNodes(node, LinkTypeEnum.Child)
                .Where(i => i.EntityTypeEnum == EntityTypeEnum.Stmtlist).ToList();


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
            TNODE procNode = ProcTable.Instance.GetAstRoot(proc);
            TNODE stmtLstChild = AST.AST.Instance.GetFirstChild(procNode);
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
                GetCallsStar(procedure.Name, procedures);
            }
            return procedures;
        }

        public bool IsCalls(string proc1, string proc2)
        {
            return GetCalls(proc1)
                .Where(i => i.Name == proc2)
                .Any();
        }

        public bool IsCallsStar(string proc1, string proc2)
        {
            return GetCallsStar(proc1)
                .Where(i => i.Name == proc2)
                .Any();
        }
        public bool IsCalledBy(string proc1, string proc2)
        {
            return GetCalledBy(proc1)
                .Where(i => i.Name == proc2)
                .Any();
        }

        public bool IsCalledStarBy(string proc1, string proc2)
        {
            return GetCalledByStar(proc1)
                .Where(i => i.Name == proc2)
                .Any();
        }
}