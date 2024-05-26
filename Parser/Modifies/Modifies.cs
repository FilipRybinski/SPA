using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;

namespace Parser.Modifies;

public class Modifies : IModifies
{
     private static Modifies? _singletonInstance;

        public static Modifies? Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new Modifies();
                }
                return _singletonInstance;
            }
        }
        private Modifies()
        {

        }
        public List<Variable> GetModified(Statement stmt)
        {
            var varIndexes = stmt.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Variable> GetModified(Procedure proc)
        {
            var varIndexes = proc.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Procedure> GetModifiesForProcs(Variable var)
        {
            return ProcedureTable.Instance!.ProceduresList.Where(procedure => IsModified(var, procedure)).ToList();
        }

        public List<Statement> GetModifiesForStmts(Variable var)
        {
            return StatementTable.Instance.StatementsList.Where(statement => IsModified(var, statement)).ToList();
        }

        public bool IsModified(Variable var, Statement stat)
        {
            var flag = false;
            try
            {
                if(stat!=null)
                    flag = stat.ModifiesList.TryGetValue(var.Id, out var value) && value;
            }
            catch (Exception e)
            {
                // ignored
            }

            return flag;
        }

        public bool IsModified(Variable var, Procedure proc)
        {
            bool flag = false;
            try
            {
                if(proc!=null)
                    flag = proc.ModifiesList.TryGetValue(var.Id, out var value) && value;
            }
            catch (Exception e)
            {
                // ignored
            }

            return flag;
        }

        public void SetModifies(Statement stmt, Variable var)
        {
            if (stmt.ModifiesList.TryGetValue(var.Id, out var value))
            {
                stmt.ModifiesList[var.Id] = true;
            }
            else
            {
                stmt.ModifiesList.Add(var.Id, true);
            }
        }

        public void SetModifies(Procedure proc, Variable var)
        {
            if (proc.ModifiesList.TryGetValue(var.Id, out var value))
            {
                proc.ModifiesList[var.Id] = true;
            }
            else
            {
                proc.ModifiesList.Add(var.Id, true);
            }
        }
}