using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables.Models;

namespace Parser.Tables
{
    public sealed class ProcedureTable : IProcTable
    {
        private static ProcedureTable? _singletonInstance;

        public static ProcedureTable? Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new ProcedureTable();
                }
                return _singletonInstance;
            }
        }
        public List<Procedure> ProceduresList { get; set; }

        private ProcedureTable()
        {
            ProceduresList = new List<Procedure>();
        }
        

        public Node GetAstRoot(string procName)
        {
            var proc = GetProcedure(procName);
            return proc == null ? null : proc.AstNodeRoot;
        }

        public Node GetAstRoot(int id)
        {
            var proc = GetProcedure(id);
            return proc == null ? null : proc.AstNodeRoot;
        }

        public List<Procedure> GetProcedureList()
        {
            return ProceduresList;
        }

        public Procedure GetProcedure(int id)
        {
            return ProceduresList.FirstOrDefault(i => i.Id == id)!;
        }

        public Procedure GetProcedure(string procName)
        {
            return ProceduresList.FirstOrDefault(i => i.Identifier == procName)!;
        }

        public int GetProcIndex(string procName)
        {
            var procedure = GetProcedure(procName);
            return procedure == null ? -1 : procedure.Id;
        }

        public int GetSize()
        {
            return ProceduresList.Count();
        }

        public int AddProcedure(string procName)
        {
            if (ProceduresList.Any(i => i.Identifier == procName))
            {
                return -1;
            }
            else
            {
                Procedure newProc = new Procedure(procName);
                newProc.Id = GetSize();
                ProceduresList.Add(newProc);
                return GetProcIndex(procName);
            }
        }

        public int SetAstRootNode(string procName, Node node)
        {
            var procedure = GetProcedure(procName);
            if (procedure == null)
            {
                return -1;

            }
            else
            {
                procedure.AstNodeRoot = node;
                return procedure.Id;
            }
        }
    }
}
