using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;

namespace Parser.Uses
{
    public sealed class Uses : IUses
    {
        private static Uses? _singletonInstance;

        public static Uses? Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new Uses();
                }
                return _singletonInstance;
            }
        }
        private Uses()
        {

        }
        public List<Variable> GetUsed(Statement statement)
        {
            var varIndexes = statement.UsesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Variable> GetUsed(Procedure procedure)
        {
            List<int> varIndexes = procedure.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Procedure> GetUsesForProcs(Variable variable)
        {
            return ProcedureTable.Instance!.ProceduresList.Where(procedure => IsUsed(variable, procedure)).ToList();
        }

        public List<Statement> GetUsesForStmts(Variable variable)
        {
            return StatementTable.Instance!.StatementsList.Where(statement => IsUsed(variable, statement)).ToList();
        }

        public bool IsUsed(Variable variable, Statement statement)
        {
            if (variable != null & statement != null)
                return statement.UsesList.TryGetValue(variable.Id, out bool value) && value;
            return false;
        }

        public bool IsUsed(Variable variable, Procedure procedure)
        {
            if (variable != null & procedure != null)
                return procedure!.UsesList.TryGetValue(variable!.Id, out var value) && value;
            return false;
        }

        public void SetUses(Statement statement, Variable variable)
        {
            if (statement.UsesList.ContainsKey(variable.Id))
            {
                statement.UsesList[variable.Id] = true;
            }
            else
            {
                statement.UsesList.Add(variable.Id, true);
            }
        }

        public void SetUses(Procedure procedure, Variable variable)
        {
            if (procedure.UsesList.ContainsKey(variable.Id))
            {
                procedure.UsesList[variable.Id] = true;
            }
            else
            {
                procedure.UsesList.Add(variable.Id, true);
            }
        }
    }
}
