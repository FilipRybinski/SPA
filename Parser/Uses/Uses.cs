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
        private static Uses _instance = null;

        public static Uses Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Uses();
                }
                return _instance;
            }
        }
        private Uses()
        {

        }
        public List<Variable> GetUsed(Statement stmt)
        {
            List<int> varIndexes = stmt.UsesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return VarTable.Instance.Variables.Where(i => varIndexes.Contains(i.Index)).ToList();
        }

        public List<Variable> GetUsed(Procedure proc)
        {
            List<int> varIndexes = proc.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return VarTable.Instance.Variables.Where(i => varIndexes.Contains(i.Index)).ToList();
        }

        public List<Procedure> GetUsesForProcs(Variable var)
        {
            List<Procedure> procedures = new List<Procedure>();

            foreach (Procedure procedure in ProcTable.Instance.Procedures)
            {
                if (IsUsed(var, procedure))
                {
                    procedures.Add(procedure);
                }
            }

            return procedures;
        }

        public List<Statement> GetUsesForStmts(Variable var)
        {
            List<Statement> statements = new List<Statement>();

            foreach (Statement statement in StmtTable.Instance.Statements)
            {
                if (IsUsed(var, statement))
                {
                    statements.Add(statement);
                }
            }

            return statements;
        }

        public bool IsUsed(Variable var, Statement stat)
        {
            if (var != null & stat != null)
                return stat.UsesList.TryGetValue(var.Index, out bool value) && value;
            return false;
        }

        public bool IsUsed(Variable var, Procedure proc)
        {
            if (var != null & proc != null)
                return proc.UsesList.TryGetValue(var.Index, out bool value) && value;
            return false;
        }

        public void SetUses(Statement stmt, Variable var)
        {
            if (stmt.UsesList.ContainsKey(var.Index))
            {
                stmt.UsesList[var.Index] = true;
            }
            else
            {
                stmt.UsesList.Add(var.Index, true);
            }
        }

        public void SetUses(Procedure proc, Variable var)
        {
            if (proc.UsesList.ContainsKey(var.Index))
            {
                proc.UsesList[var.Index] = true;
            }
            else
            {
                proc.UsesList.Add(var.Index, true);
            }
        }
    }
}
