using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IUses
{
        void AttachNewUses(Statement stmt, Variable var);
        void AttachNewUses(Procedure proc, Variable var);
        bool CheckUsesUsed(Variable? var, Statement? stat);
        bool CheckUsesUsed(Variable? var, Procedure? proc);
}