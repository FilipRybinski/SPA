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


        public Node? FindAstRootNode(string procName)
        {
            var proc = FindProcedure(procName);
            return proc?.AstNodeRoot;
        }

        public Node? FindAstRootNode(int id)
        {
            var proc = FindProcedure(id);
            return proc?.AstNodeRoot;
        }

        public List<Procedure> GetProcedureList() => ProceduresList;

        public Procedure? FindProcedure(int id) => ProceduresList.FirstOrDefault(i => i.Id == id);

        public Procedure? FindProcedure(string procName) => ProceduresList.FirstOrDefault(i => i.Identifier.ToLower() == procName.ToLower());

        public int FindIndexOfProcedure(string procName)
        {
            var procedure = FindProcedure(procName);
            return procedure is null ? -1 : procedure.Id;
        }

        public int CalculateSize() => ProceduresList.Count;

        public int InsertNewProcedure(string procName)
        {
            if (ProceduresList.Any(i => i.Identifier == procName))
                return -1;

            var newProc = new Procedure(procName);
            newProc.Id = CalculateSize();
            ProceduresList.Add(newProc);
            return FindIndexOfProcedure(procName);
        }

        public int AttachNewValueOfRootNode(string procName, Node node)
        {
            var procedure = FindProcedure(procName);
            if (procedure is null)
                return -1;

            procedure.AstNodeRoot = node;
            return procedure.Id;
        }
    }
}