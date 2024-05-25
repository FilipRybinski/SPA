using Parser.Interfaces;

namespace Parser.Tables
{
    public sealed class VarTable : IVarTable
    {
        private static VarTable _instance = null;

        public static VarTable Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VarTable();
                }
                return _instance;
            }
        }
        public List<Variable> Variables { get; set; }

        private VarTable()
        {
            Variables = new List<Variable>();
        }

        public int GetSize()
        {
            return Variables.Count();
        }

        public Variable GetVar(int index)
        {
            return Variables.Where(i => i.Index == index).FirstOrDefault();
        }

        public Variable GetVar(string varName)
        {
            return Variables.Where(i => i.Name == varName).FirstOrDefault();
        }

        public int GetVarIndex(string varName)
        {
            var variable = GetVar(varName);
            return variable == null ? -1 : variable.Index;
        }

        public int InsertVar(string varName)
        {
            if (Variables.Where(i => i.Name == varName).Any())
            {
                return -1;
            }
            else
            {
                Variable variable = new Variable(varName);
                variable.Index = GetSize();
                Variables.Add(variable);
                return GetVarIndex(varName);
            }
        }
    }
}
