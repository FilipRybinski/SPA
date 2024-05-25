using Parser.Tables;

namespace Parser.Interfaces;

public interface IVarTable
{
    int InsertVar(string procName);
    Variable GetVar(int index);
    Variable GetVar(string varName);
    int GetVarIndex(string varName);
    int GetSize();
}