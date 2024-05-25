using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Tables;

namespace Parser.Interfaces;

public interface IStmtTable
{
    int InsertStmt(EntityType entityType, int codeLine);
    Statement GetStmt(int codeLine);
    int GetSize();
    int SetAstRoot(int codeLine, Node node);
    Node GetAstRoot(int codeLine);
}