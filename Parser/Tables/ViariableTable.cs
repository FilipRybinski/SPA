using Parser.Interfaces;
using Parser.Tables.Models;

namespace Parser.Tables
{
    public sealed class ViariableTable : IVarTable
    {
        private static ViariableTable? _singletonInstance;

        public static ViariableTable? Instance
        {
            get { return _singletonInstance ?? (_singletonInstance = new ViariableTable()); }
        }

        public List<Variable> VariablesList { get; set; }

        private ViariableTable()
        {
            VariablesList = new List<Variable>();
        }

        public int GetSize() => VariablesList.Count;

        public List<Variable> GetVariablesList() => VariablesList;

        public Variable? GetVar(int index) => VariablesList.FirstOrDefault(i => i.Id == index);

        public Variable? GetVar(string varName) => VariablesList.FirstOrDefault(i => i.Identifier == varName);

        public int GetVarIndex(string varName)
        {
            var variable = GetVar(varName);
            return variable is null ? -1 : variable.Id;
        }

        public int AddVariable(string varName)
        {
            if (VariablesList.Any(i => i.Identifier == varName))
                return -1;
            var newVariable = new Variable(varName);
            newVariable.Id = GetSize();
            VariablesList.Add(newVariable);
            return GetVarIndex(varName);
        }
    }
}