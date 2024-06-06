using Parser.Tables;
using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IModifies
{
        void SetModifies(Statement stmt, Variable var);
     
        void SetModifies(Procedure proc, Variable var);

        bool IsModified(Variable var, Statement? stat);
        
        bool IsModified(Variable var, Procedure proc);
}