using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;

namespace Parser.Uses
{
    public sealed class Uses : IUses
    {
        private static Uses? _instance;
        private static IVarTable? VarTable = ViariableTable.Instance;
        private static IProcTable? ProcTable = ProcedureTable.Instance;
        private static IStmtTable? StmtTable = StatementTable.Instance;

        private Uses()
        {
        }

        public static IUses? Instance
        {
            get { return _instance ??= new Uses(); }
        }

        public bool CheckUsesUsed(Variable? variable, Statement? statement)
        {
            if (variable != null & statement != null)
                return statement!.UsesList.TryGetValue(variable!.Id, out var value) && value;
            return false;
        }

        public bool CheckUsesUsed(Variable? variable, Procedure? procedure)
        {
            if (variable != null & procedure != null)
                return procedure!.UsesList.TryGetValue(variable!.Id, out var value) && value;
            return false;
        }

        public void AttachNewUses(Statement statement, Variable variable)
        {
            if (statement.UsesList.ContainsKey(variable.Id))
                statement.UsesList[variable.Id] = true;
            else
                statement.UsesList.Add(variable.Id, true);
        }

        public void AttachNewUses(Procedure procedure, Variable variable)
        {
            if (procedure.UsesList.ContainsKey(variable.Id))
                procedure.UsesList[variable.Id] = true;
            else
                procedure.UsesList.Add(variable.Id, true);
        }
    }
}