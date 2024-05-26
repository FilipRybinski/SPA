using Parser.Interfaces;

namespace PKB.Interfaces;

public interface IPkb
{
    IUses? Uses { get;}
    ICalls? Calls { get; }
    IModifies? Modifies { get; }
    IProcTable? ProcTable { get; }
    IStmtTable? StmtTable { get; }
    IVarTable? VarTable { get; }
    
}