using Parser.Interfaces;
using Parser.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Uses
{
    public sealed class Uses : IUses
    {
        private static Uses _singletonInstance = null;

        public static Uses Instance
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
            List<int> varIndexes = statement.UsesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Variable> GetUsed(Procedure procedure)
        {
            List<int> varIndexes = procedure.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Procedure> GetUsesForProcs(Variable variable)
        {
            List<Procedure> procedures = new List<Procedure>();

            foreach (Procedure procedure in ProcedureTable.Instance.ProceduresList)
            {
                if (IsUsed(variable, procedure))
                {
                    procedures.Add(procedure);
                }
            }

            return procedures;
        }

        public List<Statement> GetUsesForStmts(Variable variable)
        {
            List<Statement> statements = new List<Statement>();

            foreach (Statement statement in StatementTable.Instance.StatementsList)
            {
                if (IsUsed(variable, statement))
                {
                    statements.Add(statement);
                }
            }

            return statements;
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
                return procedure.UsesList.TryGetValue(variable.Id, out bool value) && value;
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
