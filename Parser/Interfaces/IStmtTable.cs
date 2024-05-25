using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Tables;

namespace Parser.Interfaces;

public interface IStmtTable
{
    int InsertStmt(EntityTypeEnum entityTypeEnum, int codeLine);
    Statement GetStmt(int codeLine);
    int GetSize();
    int SetAstRoot(int codeLine, TNODE node);
    TNODE GetAstRoot(int codeLine);
}