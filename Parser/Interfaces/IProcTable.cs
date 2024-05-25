using Parser.AST.Utils;
using Parser.Tables;

namespace Parser.Interfaces;

public interface IProcTable
{
        int InsertProc(string procName);
        Procedure GetProc(int index);
        Procedure GetProc(string procName);
        int GetProcIndex(string procName);
        int GetSize();
        int SetAstRoot(string procName, TNODE node);
        TNODE GetAstRoot(string procName);
        TNODE GetAstRoot(int index);
}