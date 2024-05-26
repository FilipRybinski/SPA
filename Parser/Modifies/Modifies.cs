using Parser.Interfaces;
using Parser.Tables;

namespace Parser.Modifies;

public class Modifies : IModifies
{
     private static Modifies _instance = null;

        public static Modifies Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Modifies();
                }
                return _instance;
            }
        }
        private Modifies()
        {

        }
        public List<Variable> GetModified(Statement stmt)
        {
            List<int> varIndexes = stmt.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Variable> GetModified(Procedure proc)
        {
            List<int> varIndexes = proc.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

            return ViariableTable.Instance.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
        }

        public List<Procedure> GetModifiesForProcs(Variable var)
        {
            List<Procedure> procedures = new List<Procedure>();

            foreach(Procedure procedure in ProcedureTable.Instance.ProceduresList)
            {
                if (IsModified(var, procedure))
                {
                    procedures.Add(procedure);
                }
            }

            return procedures;
        }

        public List<Statement> GetModifiesForStmts(Variable var)
        {
            List<Statement> statements = new List<Statement>();

            foreach (Statement statement in StatementTable.Instance.StatementsList)
            {
                if (IsModified(var,statement))
                {
                    statements.Add(statement);
                }
            }

            return statements;
        }

        public bool IsModified(Variable var, Statement stat)
        {
            bool flag = false;
            try
            {
                if(stat!=null)
                    flag = stat.ModifiesList.TryGetValue(var.Id, out bool value) && value;
            } catch (Exception e) {}
            return flag;
        }

        public bool IsModified(Variable var, Procedure proc)
        {
            bool flag = false;
            try
            {
                if(proc!=null)
                    flag = proc.ModifiesList.TryGetValue(var.Id, out bool value) && value;
            } catch (Exception e) {}
            return flag;
        }

        public void SetModifies(Statement stmt, Variable var)
        {
            if (stmt.ModifiesList.TryGetValue(var.Id, out bool value))
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
            if (proc.ModifiesList.TryGetValue(var.Id, out bool value))
            {
                proc.ModifiesList[var.Id] = true;
            }
            else
            {
                proc.ModifiesList.Add(var.Id, true);
            }
        }
}