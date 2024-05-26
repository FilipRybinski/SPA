using Parser.AST.Utils;
using Parser.Tables;

namespace Parser.Interfaces;

public interface IProcTable
{
        int AddProcedure(string procName);
        Procedure GetProcedure(int index);
        Procedure GetProcedure(string procName);
        int GetProcIndex(string procName);
        int GetSize();
        int SetAstRootNode(string procName, Node node);
        Node GetAstRoot(string procName);
        Node GetAstRoot(int index);
}