namespace SPATests.ProcedureMain;

public class ProcedureMain
{
    
    private const string SimpleCodePath = "C:\\Users\\rybci\\Desktop\\SPA\\BasicSimpleCode.txt";
    private const string ResultsPath = "C:\\Users\\rybci\\Desktop\\SPA\\BasicQueries.txt";
    private const string ResultsPath2 = "C:\\Users\\rybci\\Desktop\\SPA\\Posortowane_testy.txt";
        
    public static IEnumerable<object[]> GetTestData()
    {
        var data = new List<object[]>();
        using var sr = new StreamReader(ResultsPath);
        while (!sr.EndOfStream)
        {
            var variables = sr.ReadLine();
            var query = sr.ReadLine();
            var properResult = sr.ReadLine();
            data.Add(new object[] { variables, query, properResult });
        }
        return data;
    }
    public static IEnumerable<object[]> GetTestData2()
    {
        var data = new List<object[]>();
        using var sr = new StreamReader(ResultsPath2);
        while (!sr.EndOfStream)
        {
            var variables = sr.ReadLine();
            var query = sr.ReadLine();
            var properResult = sr.ReadLine();
            data.Add(new object[] { variables, query, properResult });
        }
        return data;
    }
    
    [Theory]
    [MemberData(nameof(GetTestData))]
    public Task TestQueries(string variables, string query, string properResult)
    {
        var code = Utils.Utils.PrepareSimpleCode(SimpleCodePath);
        Utils.Utils.ParseCode(code);

        var results = QueryProcessor.QueryProcessor.ProcessQuery(variables + query, testing: true);
        var convertedResults = Utils.Utils.PrepareResults(results);
        Assert.Equal(properResult, convertedResults);
        return Task.CompletedTask;
    }
    [Theory]
    [MemberData(nameof(GetTestData2))]
    public Task TestQueries2(string variables, string query, string properResult)
    {
        var code = Utils.Utils.PrepareSimpleCode(SimpleCodePath);
        Utils.Utils.ParseCode(code);

        var results = QueryProcessor.QueryProcessor.ProcessQuery(variables + query, testing: true);
        var convertedResults = Utils.Utils.PrepareResults(results);
        Assert.Equal(properResult, convertedResults);
        return Task.CompletedTask;
    }
}