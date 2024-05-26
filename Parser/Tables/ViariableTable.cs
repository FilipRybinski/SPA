using Parser.Interfaces;

namespace Parser.Tables
{
    public sealed class ViariableTable : IVarTable
    {
        private static ViariableTable _singletonInstance = null;

        public static ViariableTable Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    _singletonInstance = new ViariableTable();
                }
                return _singletonInstance;
            }
        }
        public List<Variable> VariablesList { get; set; }

        private ViariableTable()
        {
            VariablesList = new List<Variable>();
        }

        public int GetSize()
        {
            return VariablesList.Count();
        }

        public Variable GetVar(int index)
        {
            return VariablesList.Where(i => i.Id == index).FirstOrDefault();
        }

        public Variable GetVar(string varName)
        {
            return VariablesList.Where(i => i.Identifier == varName).FirstOrDefault();
        }

        public int GetVarIndex(string varName)
        {
            var variable = GetVar(varName);
            return variable == null ? -1 : variable.Id;
        }

        public int AddVariable(string varName)
        {
            if (VariablesList.Where(i => i.Identifier == varName).Any())
            {
                return -1;
            }
            else
            {
                Variable newVariable = new Variable(varName);
                newVariable.Id = GetSize();
                VariablesList.Add(newVariable);
                return GetVarIndex(varName);
            }
        }
    }
}
