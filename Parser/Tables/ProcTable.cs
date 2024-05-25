using Parser.AST.Utils;
using Parser.Interfaces;

namespace Parser.Tables
{
    public sealed class ProcTable : IProcTable
    {
        private static ProcTable _instance = null;

        public static ProcTable Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProcTable();
                }
                return _instance;
            }
        }
        public List<Procedure> Procedures { get; set; }

        private ProcTable()
        {
            Procedures = new List<Procedure>();
        }

        public TNODE GetAstRoot(string procName)
        {
            var proc = GetProc(procName);
            return proc == null ? null : proc.AstRoot;
        }

        public TNODE GetAstRoot(int index)
        {
            var proc = GetProc(index);
            return proc == null ? null : proc.AstRoot;
        }

        public Procedure GetProc(int index)
        {
            return Procedures.Where(i => i.Index == index).FirstOrDefault();
        }

        public Procedure GetProc(string procName)
        {
            return Procedures.Where(i => i.Name == procName).FirstOrDefault();
        }

        public int GetProcIndex(string procName)
        {
            var procedure = GetProc(procName);
            return procedure == null ? -1 : procedure.Index;
        }

        public int GetSize()
        {
            return Procedures.Count();
        }

        public int InsertProc(string procName)
        {
            if (Procedures.Where(i => i.Name == procName).Any())
            {
                return -1;
            }
            else
            {
                Procedure variable = new Procedure(procName);
                variable.Index = GetSize();
                Procedures.Add(variable);
                return GetProcIndex(procName);
            }
        }

        public int SetAstRoot(string procName, TNODE node)
        {
            var procedure = GetProc(procName);
            if (procedure == null)
            {
                return -1;

            }
            else
            {
                procedure.AstRoot = node;
                return procedure.Index;
            }
        }
    }
}
