using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;

namespace Parser.Modifies;

public sealed class Modifies : IModifies
{
    private static Modifies? _instance;
    private readonly IProcTable? _procTable = ProcedureTable.Instance;
    private readonly IStmtTable? _stmtTable = StatementTable.Instance;
    private readonly IVarTable? _varTable = ViariableTable.Instance;

    public static IModifies? Instance
    {
        get { return _instance ??= new Modifies(); }
    }

    public bool AttachValueOfModifies(Variable var, Statement? stat) =>
        var == null
            ? false
            : stat.ModifiesList.TryGetValue(var.Id, out var value) && value;

    public bool AttachValueOfModifies(Variable var, Procedure proc) =>
        var == null || proc == null
            ? false
            : proc.ModifiesList.TryGetValue(var.Id, out var value) && value;

    public void AttachValueOfModifies(Statement stmt, Variable var)
    {
        if (stmt.ModifiesList.TryGetValue(var.Id, out _))
            stmt.ModifiesList[var.Id] = true;
        else
            stmt.ModifiesList.Add(var.Id, true);
    }

    public void AttachValueOfModifies(Procedure proc, Variable var)
    {
        if (proc.ModifiesList.TryGetValue(var.Id, out _))
            proc.ModifiesList[var.Id] = true;
        else
            proc.ModifiesList.Add(var.Id, true);
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
}