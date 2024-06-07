using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;
using Utils.Enums;

namespace Parser.Calls;

public sealed class Calls : ICalls
{
    private static Calls? _instance;
    private static readonly IAst? Ast = AST.Ast.Instance;
    private static readonly IProcTable? ProcTable = ProcedureTable.Instance;

    public static ICalls? Instance
    {
        get { return _instance ??= new Calls(); }
    }

    public List<Procedure> FindCalls(List<Procedure> procedures, Node stmtNode)
    {
        foreach (var procName in Ast!
                     .FindLinkedNodes(stmtNode, LinkType.Child)
                     .Where(i => i.EntityType == EntityType.Call)
                     .Select(i => i.NodeAttribute.Name)
                     .ToList())
        {
            var findProcedure = ProcTable!.FindProcedure(procName);
            if (findProcedure is null)
                continue;

            procedures.Add(findProcedure);
        }


        foreach (var node in Ast
                     .FindLinkedNodes(stmtNode, LinkType.Child)
                     .Where(i => i.EntityType is EntityType.While or EntityType.If).ToList())
        foreach (var stmtL in Ast
                     .FindLinkedNodes(node, LinkType.Child)
                     .Where(i => i.EntityType == EntityType.Stmtlist).ToList())
            FindCalls(procedures, stmtL);


        return procedures;
    }

    public bool CheckCalls(string proc1, string proc2) =>
        GetCalls(proc1)
            .Any(i => i.Identifier == proc2);

    public bool CheckCallsStar(string proc1, string proc2) =>
        GetCallsStar(proc1)
            .Any(i => i.Identifier == proc2);

    private IEnumerable<Procedure> GetCalledBy(string proc)
    {
        return ProcTable!.ProceduresList.Where(procedure => CheckCalls(procedure.Identifier, proc)).ToList();
    }

    private IEnumerable<Procedure> GetCalledByStar(string proc) => ProcTable!.ProceduresList
        .Where(procedure => CheckCallsStar(procedure.Identifier, proc)).ToList();


    private List<Procedure> GetCalls(string proc)
    {
        var procedures = new List<Procedure>();
        var procNode = ProcTable!.FindAstRootNode(proc);
        var stmtLstChild = Ast!.ReturnFirstChild(procNode);
        if (stmtLstChild is not null)
            FindCalls(procedures, stmtLstChild);

        return procedures;
    }

    private IEnumerable<Procedure> GetCallsStar(string proc) => GetCallsStar(proc, new List<Procedure>());

    private List<Procedure> GetCallsStar(string proc, List<Procedure> procedures)
    {
        foreach (var procedure in GetCalls(proc))
        {
            procedures.Add(procedure);
            GetCallsStar(procedure.Identifier, procedures);
        }

        return procedures;
    }

    public bool IsCalledBy(string proc1, string proc2) =>
        GetCalledBy(proc1)
            .Any(i => i.Identifier == proc2);

    public bool IsCalledStarBy(string proc1, string proc2)
    {
        return GetCalledByStar(proc1)
            .Any(i => i.Identifier == proc2);
    }
}