using Parser.Interfaces;
using Parser.Tables.Models;

namespace Parser.Tables
{
    public sealed class ViariableTable : IVarTable
    {
        private static ViariableTable? _singletonInstance;

        public static ViariableTable? Instance
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

        public List<Variable> GetVariablesList()
        {
            return VariablesList;
        }

        public Variable GetVar(int index)
        {
            return VariablesList.FirstOrDefault(i => i.Id == index)!;
        }

        public Variable GetVar(string varName)
        {
            return VariablesList.FirstOrDefault(i => i.Identifier == varName)!;
        }

        public int GetVarIndex(string varName)
        {
            var variable = GetVar(varName);
            return variable == null ? -1 : variable.Id;
        }

        public int AddVariable(string varName)
        {
            if (VariablesList.Any(i => i.Identifier == varName))
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
