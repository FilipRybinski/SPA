using Parser.Tables.Models;

namespace Parser.Interfaces;

public interface ICalls
{
    List<Procedure> GetCalls(string proc);
    IEnumerable<Procedure> GetCallsStar(string proc);
    IEnumerable<Procedure> GetCalledBy(string proc);
    IEnumerable<Procedure> GetCalledByStar(string proc);
    bool IsCalls(string proc1, string proc2);
    bool IsCallsStar(string proc1, string proc2);
    bool IsCalledBy(string proc1, string proc2);
    bool IsCalledStarBy(string proc1, string proc2);
}