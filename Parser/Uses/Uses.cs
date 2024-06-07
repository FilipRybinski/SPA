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
        public static IUses? Instance
        {
            get { return _instance ??= new Uses(); }
        }
        private Uses()
        {

        }
        public List<Variable> GetUsed(Statement statement)
        {
            var varIndexes = statement.UsesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return VarTable!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Variable> GetUsed(Procedure procedure)
        {
            var varIndexes = procedure.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return VarTable!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Procedure> GetUsesForProcs(Variable variable) => ProcTable!.ProceduresList.Where(procedure => CheckUsesUsed(variable, procedure)).ToList();

        public List<Statement?> GetUsesForStmts(Variable? variable) => StatementTable.Instance!.StatementsList.Where(statement => CheckUsesUsed(variable, statement)).ToList();

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
