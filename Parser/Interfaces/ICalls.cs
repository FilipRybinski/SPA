using Parser.Tables;

namespace Parser.Interfaces;

public interface ICalls
{
    List<Procedure> GetCalls(string proc);
    List<Procedure> GetCallsStar(string proc);
    List<Procedure> GetCalledBy(string proc);
    List<Procedure> GetCalledByStar(string proc);
    bool IsCalls(string proc1, string proc2);
    bool IsCallsStar(string proc1, string proc2);
    bool IsCalledBy(string proc1, string proc2);
    bool IsCalledStarBy(string proc1, string proc2);
}