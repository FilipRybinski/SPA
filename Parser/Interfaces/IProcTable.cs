using Parser.AST.Utils;
using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IProcTable
{
        int AddProcedure(string procName);
        Procedure? GetProcedure(int index);
        Procedure? GetProcedure(string procName);
        int GetProcIndex(string procName);
        int GetSize();
        int SetAstRootNode(string procName, Node node);
        Node? GetAstRoot(string procName);
        Node? GetAstRoot(int index);
        List<Procedure> GetProcedureList();
        List<Procedure> ProceduresList { get; }
}