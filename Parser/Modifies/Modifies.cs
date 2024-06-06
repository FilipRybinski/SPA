using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;

namespace Parser.Modifies;

public class Modifies : IModifies
{
    private static Modifies? _instance;
    private readonly IVarTable? _varTable = ViariableTable.Instance;
    private readonly IProcTable? _procTable =ProcedureTable.Instance;
    private readonly IStmtTable? _stmtTable = StatementTable.Instance;

    public static IModifies? Instance
    {
        get { return _instance ??= new Modifies(); }
    }

    public List<Variable> GetModified(Statement stmt)
    {
        var varIndexes = stmt.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

        return _varTable!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
    }

    public List<Variable> GetModified(Procedure proc)
    {
        var varIndexes = proc.ModifiesList.Where(i => i.Value == true).Select(i => i.Key).ToList();

        return _varTable!.VariablesList.Where(i => varIndexes.Contains(i.Id)).ToList();
    }

    public List<Procedure> GetModifiesForProcs(Variable var) => _procTable!.ProceduresList
        .Where(procedure => IsModified(var, procedure)).ToList();

    public List<Statement?> GetModifiesForStmts(Variable var) => _stmtTable!.StatementsList
        .Where(statement => IsModified(var, statement)).ToList();

    public bool IsModified(Variable var, Statement? stat) => 
        var == null ? false
            : stat.ModifiesList.TryGetValue(var.Id, out var value) && value;

    public bool IsModified(Variable var, Procedure proc) =>
        proc.ModifiesList.TryGetValue(var.Id, out var value) && value;

    public void SetModifies(Statement stmt, Variable var)
    {
        if (stmt.ModifiesList.TryGetValue(var.Id, out _))
            stmt.ModifiesList[var.Id] = true;
        else
            stmt.ModifiesList.Add(var.Id, true);
    }

    public void SetModifies(Procedure proc, Variable var)
    {
        if (proc.ModifiesList.TryGetValue(var.Id, out _))
            proc.ModifiesList[var.Id] = true;
        else
            proc.ModifiesList.Add(var.Id, true);
    }
}