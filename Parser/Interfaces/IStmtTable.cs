using Parser.AST.Utils;
using Parser.Tables.Models;
using Utils.Enums;

namespace Parser.Interfaces;

public interface IStmtTable
{
    int InsertNewStatement(EntityType entityType, int codeLine);
    Statement? FindStatement(int codeLine);
    int AttachNewValueOfAstRoot(int codeLine, Node node);
    Node FindAstRootNode(int codeLine);
    List<Statement?> GetStatementsList();
    List<Statement?> StatementsList { get;}
}