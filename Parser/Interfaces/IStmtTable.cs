using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Tables;

namespace Parser.Interfaces;

public interface IStmtTable
{
    int AddStatement(EntityType entityType, int codeLine);
    Statement GetStatement(int codeLine);
    int GetSize();
    int SetAstRoot(int codeLine, Node node);
    Node GetAstRoot(int codeLine);
}