using Parser.AST.Utils;
using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface ICalls
{
    bool CheckCalls(string proc1, string proc2);
    bool CheckCallsStar(string proc1, string proc2);
    List<Procedure> FindCalls(List<Procedure> procedures, Node stmtNode);
}