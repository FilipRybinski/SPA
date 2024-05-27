using Parser.Interfaces;
using Parser.Tables;
using UsesParser = global::Parser.Uses.Uses;
using CallsParser = global::Parser.Calls.Calls;
using ModifiesParser = global::Parser.Modifies.Modifies;

namespace Parser;

public sealed class Pkb : IPkb
{
    private static Pkb? _singletonInstance;
    public IUses? Uses => UsesParser.Instance;
    public ICalls? Calls => CallsParser.Instance;
    public IModifies? Modifies => ModifiesParser.Instance;

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