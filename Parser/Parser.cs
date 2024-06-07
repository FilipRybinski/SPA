using System.Runtime.InteropServices;
using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;
using Utils.Enums;
using Utils.Helper;

namespace Parser;

public class Parser
{
    private static readonly IPkb Pkb = global::Parser.Pkb.Instance!;
    private static readonly IAst Ast = AST.Ast.Instance!;
    private static readonly ICalls? Calls = global::Parser.Calls.Calls.Instance;
    private static readonly IStmtTable? StmtTable = StatementTable.Instance;
    private static readonly IVarTable? VarTable = ViariableTable.Instance;
    private static readonly IProcTable? ProcTable = ProcedureTable.Instance;
    private static readonly IModifies? Modifies = global::Parser.Modifies.Modifies.Instance;
    private static readonly IUses? Uses = global::Parser.Uses.Uses.Instance;
    private readonly List<string> _reservedWords;
    private int _lineNumberOld = 0;
    private int _lineNumberQuery = 1;

    public Parser()
    {
        _reservedWords = new List<string>
        {
            SyntaxDirectory.Procedure,
            SyntaxDirectory.While,
            SyntaxDirectory.If,
            SyntaxDirectory.Then,
            SyntaxDirectory.Else,
            SyntaxDirectory.Call
        };
    }

    private static char GetValueOfChar(string line, int index)
    {
        var character = line[index];
        if ((char)9 == character)
            character = ' ';
        return character;
    }

    private string RetrieveTokenValue(IReadOnlyCollection<string> lines, ref int lineNumber, int startIndex,
        out int endIndex, bool test)
    {
        var lineNumberIn = lineNumber;

        var fileLine = "";
        if (startIndex == -1)
        {
            lineNumber++;
            startIndex = 0;
        }

        if (lineNumber >= lines.Count)
            throw new Exception(SyntaxDirectory.ERROR);

        var token = "";
        char character;
        while (true)
        {
            fileLine = lines.ElementAt(lineNumber);
            if (startIndex >= fileLine.Length)
            {
                AddLineNumberQuery(fileLine, lineNumber);
                lineNumber++;

                startIndex = 0;
                if (lineNumber >= lines.Count)
                {
                    endIndex = -1;
                    if (test) lineNumber = lineNumberIn;
                    return "";
                }

                fileLine = lines.ElementAt(lineNumber);
            }

            while (fileLine == "")
            {
                lineNumber++;
                if (lineNumber >= lines.Count)
                {
                    endIndex = -1;
                    if (test) lineNumber = lineNumberIn;
                    return "";
                }

                fileLine = lines.ElementAt(lineNumber);
            }

            character = GetValueOfChar(fileLine, startIndex);
            while (character == ' ')
            {
                startIndex++;
                if (startIndex >= fileLine.Length)
                {
                    AddLineNumberQuery(fileLine, lineNumber);
                    lineNumber++;

                    startIndex = 0;
                    if (lineNumber >= lines.Count)
                    {
                        endIndex = -1;
                        if (test) lineNumber = lineNumberIn;
                        return token;
                    }

                    break;
                }

                character = GetValueOfChar(fileLine, startIndex);
            }

            if (character != ' ') break;
        }

        for (var index = startIndex; index < fileLine.Length; index++)
        {
            character = fileLine[index];
            if (char.IsLetter(character) || char.IsDigit(character))
            {
                token += character;
            }
            else
            {
                if (token == "")
                {
                    token += character;
                    endIndex = index + 1;
                    if (endIndex > fileLine.Length) endIndex = -1;
                    if (test) lineNumber = lineNumberIn;
                    return token;
                }
                else
                {
                    endIndex = index;
                    if (test) lineNumber = lineNumberIn;
                    return token;
                }
            }
        }

        endIndex = fileLine.Length + 1;
        if (endIndex > fileLine.Length) endIndex = -1;
        if (test) lineNumber = lineNumberIn;
        return token;
    }

    private void DecodeProcedure(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, Node parent)
    {
        var token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Procedure) throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        var newNode = Ast!.GenerateNode(EntityType.Procedure);
        if (CheckVariableName(token))
        {
            ProcTable!.InsertNewProcedure(token);
            ProcTable.AttachNewValueOfRootNode(token, newNode);
            Ast.AttachChildToLinkType(newNode, parent);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        var procedureName = token;
        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{")
            throw new Exception(SyntaxDirectory.ERROR);

        Decode(lines, startIndex, ref lineNumber, out endIndex, procedureName, newNode);
    }

    private void DecodeStatementList(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent)
    {
        var token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);
        var newNode = Ast!.GenerateNode(EntityType.Stmtlist);
        Ast.AttachChildToLinkType(newNode, parent);
        startIndex = endIndex;

        while (lineNumber < lines.Count)
        {
            Decode(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, newNode);
            startIndex = endIndex;

            token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token == "}")
            {
                token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
                break;
            }
        }

        if (lineNumber == lines.Count && token != "}")
            throw new Exception(SyntaxDirectory.ERROR);
    }

    private void DecodeWhile(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.While) throw new Exception(SyntaxDirectory.ERROR);
        StmtTable.InsertNewStatement(EntityType.While, _lineNumberQuery);
        startIndex = endIndex;

        var whileNode = Ast!.GenerateNode(EntityType.While);
        StmtTable.AttachNewValueOfAstRoot(_lineNumberQuery, whileNode);
        Ast.AttachValueToParentNode(whileNode, parent);
        DefineFollowingFollows(whileNode, stmtListNode, parent);
        Ast.AttachChildToLinkType(whileNode, stmtListNode);

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (CheckVariableName(token))
        {
            var variableNode =
                Ast.GenerateNode(EntityType
                    .Variable);
            Ast.AttachChildToLinkType(variableNode, whileNode);

            var var = new Variable(token);
            if (VarTable!.FindIndexOfGetIndex(token) == -1)
            {
                VarTable.InsertVariable(token);
            }

            AttachNewValueForUsesInTheSameFamily(whileNode, var);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);

        Decode(lines, startIndex, ref lineNumber, out endIndex, procedureName, whileNode);
    }

    private void DefineFollowingFollows(Node node, Node stmt, Node parent)
    {
        var siblingsList = Ast!.FindLinkedNodes(stmt, LinkType.Child);
        if (siblingsList.Count() != 0)
        {
            var prevStmt = siblingsList[siblingsList.Count() - 1];
            Ast.AssignFollows(prevStmt, node);
        }
    }

    private void AtachNewModifies(Node node, Variable var)
    {
        if (node.EntityType == EntityType.Procedure)
        {
            var proc = ProcTable.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
            var.Id = VarTable.FindIndexOfGetIndex(var.Identifier);
            Modifies.AttachValueOfModifies(proc, var);
        }
        else
        {
            var stmt = StmtTable.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
            var.Id = VarTable.FindIndexOfGetIndex(var.Identifier);
            Modifies.AttachValueOfModifies(stmt, var);
        }

        if (Ast.GetValueOfParentNode(node) != null) AtachNewModifies(Ast.GetValueOfParentNode(node), var);
    }

    private void AttachNewValueForUsesInTheSameFamily(Node node, Variable var)
    {
        if (node.EntityType == EntityType.Procedure)
        {
            var proc = ProcTable.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
            var.Id = VarTable.FindIndexOfGetIndex(var.Identifier);
            Uses.AttachNewUses(proc, var);
        }
        else
        {
            var stmt = StmtTable.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
            var.Id = VarTable.FindIndexOfGetIndex(var.Identifier);
            Uses.AttachNewUses(stmt, var);
        }

        if (Ast.GetValueOfParentNode(node) != null)
            AttachNewValueForUsesInTheSameFamily(Ast.GetValueOfParentNode(node), var);
    }

    private void DecodeAssign(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (!CheckVariableName(token))
            throw new Exception(SyntaxDirectory.ERROR);
        StmtTable.InsertNewStatement(EntityType.Assign, _lineNumberQuery);
        startIndex = endIndex;

        var assignNode = Ast.GenerateNode(EntityType.Assign);
        var var = new Variable(token);
        VarTable.InsertVariable(token);
        StmtTable.AttachNewValueOfAstRoot(_lineNumberQuery, assignNode);
        Ast.AttachValueToParentNode(assignNode, parent);
        DefineFollowingFollows(assignNode, stmtListNode, parent);
        Ast.AttachChildToLinkType(assignNode, stmtListNode);
        var variableNode = Ast.GenerateNode(EntityType.Variable);
        Ast.AttachChildToLinkType(variableNode, assignNode);
        AtachNewModifies(assignNode, var);
        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != "=") throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        Node expressionRoot;
        DecodeExpresion(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, assignNode, false,
            out expressionRoot);
        Ast.AttachChildToLinkType(expressionRoot, assignNode);
        startIndex = endIndex;
    }

    private bool DecodeExpresion(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node assignNode, Node parent, bool inBracket, out Node expressionRoot,
        string prevToken = "")
    {
        var endAssign = false;
        var token = "";
        endIndex = startIndex;
        var expectedOperation = false;
        var possibleBracketClose = false;
        var tokenCount = 0;
        var bracketsPaired = false;
        expressionRoot = null;
        while (lineNumber < lines.Count)
        {
            token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
            tokenCount++;
            if (expectedOperation)
            {
                Node oldAssignRoot;
                token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
                startIndex = endIndex;
                switch (token)
                {
                    case "+":
                        oldAssignRoot = Ast.ReplicateNode(expressionRoot);
                        expressionRoot = Ast.GenerateNode(EntityType.Plus);
                        Ast.AttachChildToLinkType(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "-":
                        oldAssignRoot = Ast.ReplicateNode(expressionRoot);
                        expressionRoot = Ast.GenerateNode(EntityType.Minus);
                        Ast.AttachChildToLinkType(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "*":
                        oldAssignRoot = Ast.ReplicateNode(expressionRoot);
                        expressionRoot = Ast.GenerateNode(EntityType.Multiply);
                        Ast.AttachChildToLinkType(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "/":
                        oldAssignRoot = Ast.ReplicateNode(expressionRoot);
                        expressionRoot = Ast.GenerateNode(EntityType.Divide);
                        Ast.AttachChildToLinkType(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case ")":
                        if (!possibleBracketClose)
                            throw new Exception(SyntaxDirectory.ERROR);
                        bracketsPaired = true;
                        expectedOperation = true;

                        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);

                        if (token == "*" || token == "/")
                        {
                            if (parent.EntityType == EntityType.Multiply || parent.EntityType == EntityType.Divide)
                            {
                                Ast.AttachChildToLinkType(parent, expressionRoot);
                            }
                        }
                        else
                        {
                            Ast.AttachChildToLinkType(parent, expressionRoot);
                        }

                        break;
                    default:
                        throw new Exception(SyntaxDirectory.ERROR);
                }
            }
            else
            {
                if (CheckVariableName(token))
                {
                    if (expressionRoot == null) expressionRoot = Ast.GenerateNode(EntityType.Variable);
                    else
                    {
                        var nextToken = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
                        if (nextToken == "*" || nextToken == "/")
                        {
                            if (expressionRoot.EntityType == EntityType.Divide ||
                                expressionRoot.EntityType == EntityType.Multiply)
                            {
                                var rightSide = Ast.GenerateNode(EntityType.Variable);
                                Ast.AttachChildToLinkType(rightSide, expressionRoot);
                            }
                            else
                            {
                                Node tinyTreeRoot = null;
                                endAssign = DecodeExpresion(lines, startIndex, ref lineNumber, out endIndex,
                                    procedureName,
                                    assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                Ast.AttachChildToLinkType(tinyTreeRoot, expressionRoot);
                            }
                        }
                        else
                        {
                            var rightSide = Ast.GenerateNode(EntityType.Variable);
                            Ast.AttachChildToLinkType(rightSide, expressionRoot);
                        }
                    }

                    var usesVar = new Variable(token);
                    if (VarTable != null && VarTable.FindIndexOfGetIndex(token) == -1)
                    {
                        VarTable.InsertVariable(token);
                    }

                    AttachNewValueForUsesInTheSameFamily(assignNode, usesVar);
                    startIndex = endIndex;
                    expectedOperation = true;
                }
                else if (CheckReadOnlyValue(token))
                {
                    if (expressionRoot == null) expressionRoot = Ast.GenerateNode(EntityType.Constant);
                    else
                    {
                        var nextToken = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
                        if (nextToken == "*" || nextToken == "/")
                        {
                            if (expressionRoot.EntityType == EntityType.Divide ||
                                expressionRoot.EntityType == EntityType.Multiply)
                            {
                                var rightSide = Ast.GenerateNode(EntityType.Constant);
                                Ast.AttachChildToLinkType(rightSide, expressionRoot);
                            }
                            else
                            {
                                Node tinyTreeRoot = null;
                                endAssign = DecodeExpresion(lines, startIndex, ref lineNumber, out endIndex,
                                    procedureName,
                                    assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                Ast.AttachChildToLinkType(tinyTreeRoot, expressionRoot);
                            }
                        }
                        else
                        {
                            var rightSide = Ast.GenerateNode(EntityType.Constant);
                            Ast.AttachChildToLinkType(rightSide, expressionRoot);
                        }
                    }

                    startIndex = endIndex;
                    expectedOperation = true;
                }
                else if (token == "(")
                {
                    if (tokenCount == 1)
                    {
                        possibleBracketClose = true;
                        bracketsPaired = false;
                        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
                        startIndex = endIndex;
                    }
                    else
                    {
                        Node tinyTreeRoot;
                        endAssign = DecodeExpresion(lines, startIndex, ref lineNumber, out endIndex, procedureName,
                            assignNode, expressionRoot, true, out tinyTreeRoot);
                        if (expressionRoot == null)
                            expressionRoot = Ast.ReplicateNode(tinyTreeRoot);
                        startIndex = endIndex;
                        expectedOperation = true;
                        if (endAssign)
                        {
                            token = ";";
                            break;
                        }
                    }
                }
                else
                    throw new Exception(SyntaxDirectory.ERROR);
            }

            token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token == ";")
            {
                if (inBracket && !bracketsPaired)
                    throw new Exception(SyntaxDirectory.ERROR);
                token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
                endAssign = true;
                break;
            }
        }

        if (lineNumber == lines.Count && token != ";")
            throw new Exception(SyntaxDirectory.ERROR);
        return endAssign;
    }

    private void DecodeCall(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Call) throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        StmtTable?.InsertNewStatement(EntityType.Call, _lineNumberQuery);
        var callNode = Ast.GenerateNode(EntityType.Call);
        StmtTable?.AttachNewValueOfAstRoot(_lineNumberQuery, callNode);

        Ast.AttachValueToParentNode(callNode, parent);

        DefineFollowingFollows(callNode, stmtListNode, parent);
        Ast.AttachChildToLinkType(callNode, stmtListNode);

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (CheckVariableName(token)) callNode.NodeAttribute.Name = token;
        else throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        startIndex = endIndex;
        if (token != ";") throw new Exception(SyntaxDirectory.ERROR);
    }

    private void DecodeIf(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName,
        Node parent, Node stmtListNode)
    {
        var token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.If) throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        StmtTable?.InsertNewStatement(EntityType.If, _lineNumberQuery);
        var ifNode = Ast.GenerateNode(EntityType.If);
        StmtTable?.AttachNewValueOfAstRoot(_lineNumberQuery, ifNode);

        Ast.AttachValueToParentNode(ifNode, parent);
        DefineFollowingFollows(ifNode, stmtListNode, parent);
        Ast.AttachChildToLinkType(ifNode, stmtListNode);

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (CheckVariableName(token))
        {
            var var = new Variable(token);
            if (VarTable != null && VarTable.FindIndexOfGetIndex(token) == -1)
            {
                VarTable.InsertVariable(token);
            }

            AttachNewValueForUsesInTheSameFamily(ifNode, var);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Then) throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);

        Decode(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Else) throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);

        Decode(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
        startIndex = endIndex;
    }

    private void Decode(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName,
        Node parent, [Optional] Node stmtList)
    {
        var token = RetrieveTokenValue(lines, ref lineNumber, startIndex, out endIndex, true);
        switch (token)
        {
            case SyntaxDirectory.Procedure:
                DecodeProcedure(lines, startIndex, ref lineNumber, out endIndex, parent);
                break;
            case "{":
                DecodeStatementList(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent);
                break;
            case SyntaxDirectory.While:
                DecodeWhile(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                break;
            case SyntaxDirectory.Call:
                DecodeCall(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                break;
            case SyntaxDirectory.If:
                DecodeIf(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                break;
            default:
                if (CheckVariableName(token))
                {
                    DecodeAssign(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                    break;
                }
                else throw new Exception(SyntaxDirectory.ERROR);
        }
    }

    private bool CheckVariableName(string name)
    {
        if (name.Length == 0) return false;
        if (!char.IsLetter(name[0])) return false;
        else if (_reservedWords.IndexOf(name) > 0) return false;

        return true;
    }

    private bool CheckReadOnlyValue(string name) => name.Length != 0 && long.TryParse(name, out _);

    public void StartDecoding(string code)
    {
        var lines = code.Split(new[] { '\r', '\n' }).ToList();

        if (lines.Count == 0 || (lines.Count == 1 && string.IsNullOrEmpty(lines[0])))
            throw new Exception(SyntaxDirectory.ERROR);

        var lineNumber = 0;
        var index = 0;
        var countToken = 0;
        while (lineNumber < lines.Count)
        {
            var token = RetrieveTokenValue(lines, ref lineNumber, index, out var endIndex, true);
            if (token != "") countToken++;
            if (token == "")
            {
                if (countToken > 0) break;
                else throw new Exception(SyntaxDirectory.ERROR);
            }

            var newRoot = Ast!.GenerateNode(EntityType.Program);
            Ast.AssignToRootNode(newRoot);

            if (token != SyntaxDirectory.Procedure)
                throw new Exception(SyntaxDirectory.ERROR);
            Decode(lines, index, ref lineNumber, out endIndex, "", newRoot);
            index = endIndex;
        }

        UpdateModifiesAndUsesTablesInProcedures();
        UpdateModifiesAndUsesTablesInWhilesAndIfs();
    }

    private void UpdateModifiesAndUsesTablesInProcedures()
    {
        try
        {
            var isChanged = false;

            do
            {
                isChanged = false;
                foreach (var i in Enumerable.Range(0, ProcTable!.CalculateSize()))
                {
                    var p1 = ProcTable!.FindProcedure(i);
                    if (p1 != null)
                    {
                        foreach (var j in Enumerable.Range(0, ProcTable!.CalculateSize()))
                        {
                            var p2 = ProcTable!.FindProcedure(j);
                            if (p2 != null)
                            {
                                if (Calls!.CheckCalls(p1.Identifier, p2.Identifier))
                                {
                                    foreach (var variable in p2.ModifiesList)
                                    {
                                        isChanged = CheckListChanged(p1.ModifiesList, variable.Key);
                                    }

                                    foreach (var variable in p2.UsesList)
                                    {
                                        isChanged = CheckListChanged(p1.UsesList, variable.Key);
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                throw new Exception(SyntaxDirectory.ERROR);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(SyntaxDirectory.ERROR);
                    }
                }

                //# b
                foreach (var i in Enumerable.Range(0, ProcTable!.CalculateSize()))
                {
                    var p1 = ProcTable!.FindProcedure(i);
                    if (p1 != null)
                    {
                        foreach (var j in Enumerable.Range(0, ProcTable!.CalculateSize()))
                        {
                            var p2 = ProcTable!.FindProcedure(j);
                            if (p2 != null)
                            {
                                if (Calls!.CheckCalls(p1.Identifier, p2.Identifier))
                                {
                                    foreach (var variable in p2.ModifiesList)
                                    {
                                        isChanged = CheckListChanged(p1.ModifiesList, variable.Key);
                                    }

                                    foreach (var variable in p2.UsesList)
                                    {
                                        isChanged = CheckListChanged(p1.UsesList, variable.Key);
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                throw new Exception(SyntaxDirectory.ERROR);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(SyntaxDirectory.ERROR);
                    }
                }
            } while (isChanged);

            foreach (var variable in StmtTable!.StatementsList)
            {
                if (!EntityType.Call.Equals(variable.StmtType))
                {
                    continue;
                }
                else
                {
                    var procedure = ProcTable.FindProcedure(
                        variable.AstRoot.NodeAttribute.Name
                    );

                    if (procedure != null)
                    {
                        variable.ModifiesList = procedure.ModifiesList;
                        variable.UsesList = procedure.UsesList;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception(SyntaxDirectory.ERROR);
        }
    }

    private bool CheckListChanged(Dictionary<int, bool> collection, int i)
    {
        return collection.TryAdd(i, true) ? true : false;
    }

    private void UpdateModifiesAndUsesTablesInWhilesAndIfs()
    {
        var ifOrWhileStmts = StmtTable!.StatementsList
            .Where(i => i?.AstRoot.EntityType is EntityType.While or EntityType.If).ToList();

        foreach (var stmt in ifOrWhileStmts.Where(i => i is not null))
        {
            var node = stmt.AstRoot;
            var stmtLstNodes = Ast!
                .FindLinkedNodes(node, LinkType.Child)
                .Where(i => i.EntityType == EntityType.Stmtlist).ToList();

            var procedures = new List<Procedure>();
            foreach (var stmtL in stmtLstNodes)
                Calls!.FindCalls(procedures, stmtL);

            foreach (var proc in procedures)
            {
                foreach (var variable in proc.ModifiesList)
                    if (!stmt.ModifiesList.ContainsKey(variable.Key))
                        stmt.ModifiesList[variable.Key] = true;

                foreach (var variable in proc.UsesList)
                    if (!stmt.UsesList.ContainsKey(variable.Key))
                        stmt.UsesList[variable.Key] = true;
            }
        }
    }

    public static void CleanData()
    {
        Ast.Root = null;
        VarTable!.VariablesList.Clear();
        StmtTable!.StatementsList.Clear();
        ProcTable!.ProceduresList.Clear();
    }

    private void AddLineNumberQuery(string line, int lineNumber)
    {
        if (line.Contains(SyntaxDirectory.Procedure) || line.Contains(SyntaxDirectory.Else)) return;
        if (lineNumber - _lineNumberOld < 1) return;

        _lineNumberQuery++;
        _lineNumberOld = lineNumber;
    }
}