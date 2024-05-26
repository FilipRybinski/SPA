using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IUses
{
        void SetUses(Statement stmt, Variable var);
        void SetUses(Procedure proc, Variable var);
        List<Variable> GetUsed(Statement stmt);
        List<Variable> GetUsed(Procedure proc);
        List<Statement> GetUsesForStmts(Variable var);
        bool IsUsed(Variable var, Statement stat);
        bool IsUsed(Variable var, Procedure proc);
}