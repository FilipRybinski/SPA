using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IVarTable
{
    int AddVariable(string procName);
    Variable? GetVar(int index);
    Variable? GetVar(string varName);
    int GetVarIndex(string varName);
    List<Variable> GetVariablesList();
    List<Variable> VariablesList { get; }

}