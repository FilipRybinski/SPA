using Parser.Interfaces;
using Parser.Tables.Models;

namespace Parser.Tables
{
    public sealed class ViariableTable : IVarTable
    {
        private static ViariableTable? _instance;

        public static IVarTable? Instance
        {
            get { return _instance ??= new ViariableTable(); }
        }

        public List<Variable> VariablesList { get; set; }

        private ViariableTable()
        {
            VariablesList = new List<Variable>();
        }

        public int GetSize() => VariablesList.Count;

        public List<Variable> GetVariablesList() => VariablesList;

        public Variable? FindVariable(int index) => VariablesList.FirstOrDefault(i => i.Id == index);

        public Variable? FindVariable(string varName) => VariablesList.FirstOrDefault(i => i.Identifier == varName);

        public int FindIndexOfGetIndex(string varName)
        {
            var variable = FindVariable(varName);
            return variable is null ? -1 : variable.Id;
        }

        public int InsertVariable(string varName)
        {
            if (VariablesList.Any(i => i.Identifier == varName))
                return -1;
            var newVariable = new Variable(varName);
            newVariable.Id = GetSize();
            VariablesList.Add(newVariable);
            return FindIndexOfGetIndex(varName);
        }
    }
}