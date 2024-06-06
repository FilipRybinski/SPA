using Parser.AST.Utils;
using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface ICalls
{
    bool IsCalls(string proc1, string proc2);
    bool IsCallsStar(string proc1, string proc2);
    List<Procedure> GetCalls(List<Procedure> procedures, Node stmtNode);
}