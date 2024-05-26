using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;
using Utils.Enums;

namespace Parser.Calls;

public class Calls : ICalls
{
      private static Calls? _singletonInstance;

        public static Calls? Instance
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
    
        public IEnumerable<Procedure> GetCalledBy(string proc)
        {
            return ProcedureTable.Instance!.ProceduresList.Where(procedure => IsCalls(procedure.Identifier, proc)).ToList();
        }

        public IEnumerable<Procedure> GetCalledByStar(string proc)
        {
            List<Procedure> procedures = new List<Procedure>();
            foreach(Procedure procedure in ProcedureTable.Instance!.ProceduresList)
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
            

            var procNames = AST.Ast.Instance!
                .GetLinkedNodes(stmtNode, LinkType.Child)
                .Where(i => i.EntityType == EntityType.Call)
                .Select(i => i.NodeAttribute.Name)
                .ToList();
            foreach(var proce in procNames)
            {
                var findProcedure = ProcedureTable.Instance!.GetProcedure(proce);
                if(findProcedure != null)
                {
                    procedures.Add(findProcedure);
                }
            }


            List<Node> ifOrWhileNodes = AST.Ast.Instance
                .GetLinkedNodes(stmtNode, LinkType.Child)
                .Where(i => i.EntityType is EntityType.While or EntityType.If).ToList();

            foreach(var node in ifOrWhileNodes)
            {
                List<Node> stmtLstNodes = AST.Ast.Instance
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
            var procedures = new List<Procedure>();
            var procNode = ProcedureTable.Instance!.GetAstRoot(proc);
            var stmtLstChild = AST.Ast.Instance!.GetFirstChild(procNode);
            GetCalls(procedures, stmtLstChild);

            
            return procedures;
        }

        public IEnumerable<Procedure> GetCallsStar(string proc)
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
                .Any(i => i.Identifier == proc2);
        }

        public bool IsCallsStar(string proc1, string proc2)
        {
            return GetCallsStar(proc1)
                .Any(i => i.Identifier == proc2);
        }
        public bool IsCalledBy(string proc1, string proc2)
        {
            return GetCalledBy(proc1)
                .Any(i => i.Identifier == proc2);
        }

        public bool IsCalledStarBy(string proc1, string proc2)
        {
            return GetCalledByStar(proc1)
                .Any(i => i.Identifier == proc2);
        }
}