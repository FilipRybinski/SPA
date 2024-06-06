using Parser.AST;
using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;
using Utils.Enums;

namespace Parser.Calls;

public class Calls : ICalls
{
    private static Calls? _instance;
    private static readonly IAst? Ast= AST.Ast.Instance;
    private static readonly IProcTable? ProcTable= ProcedureTable.Instance;
    public static ICalls? Instance
    {
        get
        {
            return _instance ??= new Calls();
        }
    }

    public IEnumerable<Procedure> GetCalledBy(string proc)
    {
        return ProcTable!.ProceduresList.Where(procedure => IsCalls(procedure.Identifier, proc)).ToList();
    }

    public IEnumerable<Procedure> GetCalledByStar(string proc) => ProcTable!.ProceduresList
        .Where(procedure => IsCallsStar(procedure.Identifier, proc)).ToList();

    public List<Procedure> GetCalls(List<Procedure> procedures, Node stmtNode)
    {
        foreach (var procName in Ast!
                     .GetLinkedNodes(stmtNode, LinkType.Child)
                     .Where(i => i.EntityType == EntityType.Call)
                     .Select(i => i.NodeAttribute.Name)
                     .ToList())
        {
            var findProcedure = ProcTable!.GetProcedure(procName);
            if (findProcedure is null)
                continue;

            procedures.Add(findProcedure);
        }


        foreach (var node in Ast
                     .GetLinkedNodes(stmtNode, LinkType.Child)
                     .Where(i => i.EntityType is EntityType.While or EntityType.If).ToList())
        foreach (var stmtL in Ast
                     .GetLinkedNodes(node, LinkType.Child)
                     .Where(i => i.EntityType == EntityType.Stmtlist).ToList())
            GetCalls(procedures, stmtL);


        return procedures;
    }


    public List<Procedure> GetCalls(string proc)
    {
        var procedures = new List<Procedure>();
        var procNode = ProcTable!.GetAstRoot(proc);
        var stmtLstChild = Ast!.GetFirstChild(procNode);
        if (stmtLstChild is not null)
            GetCalls(procedures, stmtLstChild);

        return procedures;
    }

    public IEnumerable<Procedure> GetCallsStar(string proc) => GetCallsStar(proc, new List<Procedure>());

    private List<Procedure> GetCallsStar(string proc, List<Procedure> procedures)
    {
        foreach (var procedure in GetCalls(proc))
        {
            procedures.Add(procedure);
            GetCallsStar(procedure.Identifier, procedures);
        }

        return procedures;
    }

    public bool IsCalls(string proc1, string proc2) =>
        GetCalls(proc1)
            .Any(i => i.Identifier == proc2);

    public bool IsCallsStar(string proc1, string proc2) =>
        GetCallsStar(proc1)
            .Any(i => i.Identifier == proc2);

    public bool IsCalledBy(string proc1, string proc2) =>
        GetCalledBy(proc1)
            .Any(i => i.Identifier == proc2);

    public bool IsCalledStarBy(string proc1, string proc2)
    {
        return GetCalledByStar(proc1)
            .Any(i => i.Identifier == proc2);
    }
}