using Parser.Interfaces;
using Parser.Tables;
using PKB.Interfaces;

namespace PKB;

public class Pkb : IPkb
{
    private static Pkb? _singletonInstance;
    public IUses? Uses => Parser.Uses.Uses.Instance;

    public ICalls? Calls => Parser.Calls.Calls.Instance;

    public IModifies? Modifies => Parser.Modifies.Modifies.Instance;

    public IProcTable? ProcTable => ProcedureTable.Instance;

    public IStmtTable? StmtTable => StatementTable.Instance;

    public IVarTable? VarTable => ViariableTable.Instance;

    public static IPkb? Instance 
    {
        get
        {
            if (_singletonInstance == null)
            {
                _singletonInstance = new Pkb();
            }
            return _singletonInstance;
        }
    }
}