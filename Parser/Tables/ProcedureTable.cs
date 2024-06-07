using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables.Models;

namespace Parser.Tables
{
    public sealed class ProcedureTable : IProcTable
    {
        private static ProcedureTable?  _instance;

        public static IProcTable? Instance
        {
            get { return _instance ??= new ProcedureTable(); }
        }

        public List<Procedure> ProceduresList { get; set; }

        private ProcedureTable()
        {
            ProceduresList = new List<Procedure>();
        }


        public Node? GetAstRoot(string procName)
        {
            var proc = GetProcedure(procName);
            return proc?.AstNodeRoot;
        }

        public Node? GetAstRoot(int id)
        {
            var proc = GetProcedure(id);
            return proc?.AstNodeRoot;
        }

        public List<Procedure> GetProcedureList() => ProceduresList;

        public Procedure? GetProcedure(int id) => ProceduresList.FirstOrDefault(i => i.Id == id);

        public Procedure? GetProcedure(string procName) => ProceduresList.FirstOrDefault(i => i.Identifier.ToLower() == procName.ToLower());

        public int GetProcIndex(string procName)
        {
            var procedure = GetProcedure(procName);
            return procedure is null ? -1 : procedure.Id;
        }

        public int GetSize() => ProceduresList.Count;

        public int AddProcedure(string procName)
        {
            if (ProceduresList.Any(i => i.Identifier == procName))
                return -1;

            var newProc = new Procedure(procName);
            newProc.Id = GetSize();
            ProceduresList.Add(newProc);
            return GetProcIndex(procName);
        }

        public int SetAstRootNode(string procName, Node node)
        {
            var procedure = GetProcedure(procName);
            if (procedure is null)
                return -1;

            procedure.AstNodeRoot = node;
            return procedure.Id;
        }
    }
}