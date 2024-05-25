using Parser.Tables;

namespace Parser.Interfaces;

public interface IModifies
{
        void SetModifies(Statement stmt, Variable var);
     
        void SetModifies(Procedure proc, Variable var);
        
        List<Variable> GetModified(Statement stmt);
        
        List<Variable> GetModified(Procedure proc);
        
        List<Statement> GetModifiesForStmts(Variable var);
        
        List<Procedure> GetModifiesForProcs(Variable var);
        
        bool IsModified(Variable var, Statement stat);
        
        bool IsModified(Variable var, Procedure proc);
}