using Parser.AST.Utils;
using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IProcTable
{
        int InsertNewProcedure(string procName);
        Procedure? FindProcedure(int index);
        Procedure? FindProcedure(string procName);
        int FindIndexOfProcedure(string procName);
        int CalculateSize();
        int AttachNewValueOfRootNode(string procName, Node node);
        Node? FindAstRootNode(string procName);
        Node? FindAstRootNode(int index);
        List<Procedure> GetProcedureList();
        List<Procedure> ProceduresList { get; }
}