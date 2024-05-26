using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IVarTable
{
    int AddVariable(string procName);
    Variable GetVar(int index);
    Variable GetVar(string varName);
    int GetVarIndex(string varName);
    int GetSize();
    List<Variable> GetVariablesList();

}