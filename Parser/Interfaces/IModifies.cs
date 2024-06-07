using Parser.Tables;
using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface IModifies
{
        void AttachValueOfModifies(Statement stmt, Variable var);
     
        void AttachValueOfModifies(Procedure proc, Variable var);

        bool AttachValueOfModifies(Variable var, Statement? stat);
        
        bool AttachValueOfModifies(Variable var, Procedure proc);
}