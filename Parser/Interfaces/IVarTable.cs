using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IVarTable
{
    int InsertVariable(string procName);
    Variable? FindVariable(int index);
    Variable? FindVariable(string varName);
    int FindIndexOfGetIndex(string varName);
    List<Variable> GetVariablesList();
    List<Variable> VariablesList { get; }

}