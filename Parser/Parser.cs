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
    int _lineNumberQuery = 1;
    int _lineNumberOld = 0;
    readonly List<String> _reservedWords;
    private static readonly IPkb Pkb = global::Parser.Pkb.Instance!;

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

    public char GetChar(string line, int index)
    {
        var character = line[index];
        if ((char)9 == character)
            character = ' ';
        return character;
    }

    public string GetToken(List<string> lines, ref int lineNumber, int startIndex, out int endIndex, bool test)
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

            character = GetChar(fileLine, startIndex);
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

                character = GetChar(fileLine, startIndex);
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

    public void ParseProcedure(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, Node parent)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Procedure) throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        var newNode = AST.Ast.Instance!.CreateTNode(EntityType.Procedure);
        if (IsVarName(token))
        {
            ProcedureTable.Instance!.AddProcedure(token);
            ProcedureTable.Instance.SetAstRootNode(token, newNode);
            AST.Ast.Instance.SetChildOfLink(newNode, parent);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        var procedureName = token;
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{")
            throw new Exception(SyntaxDirectory.ERROR);

        Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, newNode);
    }

    public void ParseStmtLst(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);
        var newNode = AST.Ast.Instance!.CreateTNode(EntityType.Stmtlist);
        AST.Ast.Instance.SetChildOfLink(newNode, parent);
        startIndex = endIndex;

        while (lineNumber < lines.Count)
        {
            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, newNode);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token == "}")
            {
                token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                break;
            }
        }

        if (lineNumber == lines.Count && token != "}")
            throw new Exception(SyntaxDirectory.ERROR);
    }

    public void ParseWhile(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.While) throw new Exception(SyntaxDirectory.ERROR);
        StatementTable.Instance.AddStatement(EntityType.While, _lineNumberQuery);
        startIndex = endIndex;

        var whileNode = AST.Ast.Instance!.CreateTNode(EntityType.While); 
        StatementTable.Instance.SetAstRoot(_lineNumberQuery, whileNode);
        AST.Ast.Instance.SetParent(whileNode, parent); 
        SettingFollows(whileNode, stmtListNode, parent);
        AST.Ast.Instance.SetChildOfLink(whileNode, stmtListNode); 

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (IsVarName(token))
        {
            var variableNode =
                AST.Ast.Instance.CreateTNode(EntityType
                    .Variable); 
            AST.Ast.Instance.SetChildOfLink(variableNode, whileNode);

            var var = new Variable(token);
            if (ViariableTable.Instance!.GetVarIndex(token) == -1)
            {
                ViariableTable.Instance.AddVariable(token);
            }

            SetUsesForFamily(whileNode, var);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);

        Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, whileNode);
    }

    public void SettingFollows(Node node, Node stmt, Node parent)
    {
        var siblingsList = AST.Ast.Instance!.GetLinkedNodes(stmt, LinkType.Child);
        if (siblingsList.Count() != 0)
        {
            var prevStmt = siblingsList[siblingsList.Count() - 1];
            AST.Ast.Instance.SetFollows(prevStmt, node);
        }
    }

    public void SetModifiesForFamily(Node node, Variable var)
    {
        if (node.EntityType == EntityType.Procedure)
        {
            var proc = ProcedureTable.Instance.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
            var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
            Modifies.Modifies.Instance.SetModifies(proc, var);
        }
        else
        {
            var stmt = StatementTable.Instance.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
            var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
            Modifies.Modifies.Instance.SetModifies(stmt, var);
        }

        if (AST.Ast.Instance.GetParent(node) != null) SetModifiesForFamily(AST.Ast.Instance.GetParent(node), var);
    }

    public void SetUsesForFamily(Node node, Variable var)
    {
        if (node.EntityType == EntityType.Procedure)
        {
            var proc = ProcedureTable.Instance.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
            var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
            Uses.Uses.Instance.SetUses(proc, var);
        }
        else
        {
            var stmt = StatementTable.Instance.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
            var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
            Uses.Uses.Instance.SetUses(stmt, var);
        }

        if (AST.Ast.Instance.GetParent(node) != null) SetUsesForFamily(AST.Ast.Instance.GetParent(node), var);
    }

    public void ParseAssign(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (!IsVarName(token))
            throw new Exception(SyntaxDirectory.ERROR);
        StatementTable.Instance.AddStatement(EntityType.Assign, _lineNumberQuery);
        startIndex = endIndex;

        var assignNode = AST.Ast.Instance.CreateTNode(EntityType.Assign);
        var var = new Variable(token);
        ViariableTable.Instance.AddVariable(token);
        StatementTable.Instance.SetAstRoot(_lineNumberQuery, assignNode);
        AST.Ast.Instance.SetParent(assignNode, parent);
        SettingFollows(assignNode, stmtListNode, parent);
        AST.Ast.Instance.SetChildOfLink(assignNode, stmtListNode);
        var variableNode = AST.Ast.Instance.CreateTNode(EntityType.Variable);
        AST.Ast.Instance.SetChildOfLink(variableNode, assignNode);
        SetModifiesForFamily(assignNode, var);
        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != "=") throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        Node expressionRoot;
        ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, assignNode, false,
            out expressionRoot);
        AST.Ast.Instance.SetChildOfLink(expressionRoot, assignNode);
        startIndex = endIndex;
    }

    public void ParseAssignOld(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (!IsVarName(token))
            throw new Exception(SyntaxDirectory.ERROR);
        StatementTable.Instance.AddStatement(EntityType.Assign, _lineNumberQuery);
        startIndex = endIndex;

        var assignNode = AST.Ast.Instance.CreateTNode(EntityType.Assign);
        var var = new Variable(token);
        ViariableTable.Instance.AddVariable(token);
        StatementTable.Instance.SetAstRoot(_lineNumberQuery, assignNode);
        AST.Ast.Instance.SetParent(assignNode, parent);

        var stmtListNode = AST.Ast.Instance.GetChildOfIdx(0, parent);
        SettingFollows(assignNode, stmtListNode, parent);
        AST.Ast.Instance.SetChildOfLink(assignNode, stmtListNode);
        var variableNode = AST.Ast.Instance.CreateTNode(EntityType.Variable);
        AST.Ast.Instance.SetChildOfLink(variableNode, assignNode);
        SetModifiesForFamily(assignNode, var);

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != "=") throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = "";
        var expectedOperation = false;
        Node expressionRoot = null;
        while (lineNumber < lines.Count)
        {
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            startIndex = endIndex;
            if (expectedOperation)
            {
                Node oldAssignRoot;
                switch (token)
                {
                    case "+":
                        oldAssignRoot = AST.Ast.Instance.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Plus);
                        AST.Ast.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                        break;
                    case "-":
                        oldAssignRoot = AST.Ast.Instance.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Minus);
                        AST.Ast.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                        break;
                    case "*":
                        break;
                    default:
                        throw new Exception(SyntaxDirectory.ERROR);
                }

                expectedOperation = false;
            }
            else
            {
                if (IsVarName(token))
                {
                    if (expressionRoot == null)
                    {
                        expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Variable);

                        var usesVar = new Variable(token);
                        SetUsesForFamily(assignNode, usesVar);
                    }
                    else
                    {
                        var rightSide = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                        AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);

                        var usesVar = new Variable(token);
                        SetUsesForFamily(assignNode, usesVar);
                    }
                }
                else if (IsConstValue(token))
                {
                    if (expressionRoot == null) expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                    else
                    {
                        var rightSide = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                        AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                    }
                }
                else
                    throw new Exception(SyntaxDirectory.ERROR);

                expectedOperation = true;
            }

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token == ";")
            {
                token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                break;
            }
        }

        if (lineNumber == lines.Count && token != ";")
            throw new Exception(SyntaxDirectory.ERROR);
        
        AST.Ast.Instance.SetChildOfLink(expressionRoot, assignNode);
    }

    public bool ParseExpr(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
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
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            tokenCount++;
            if (expectedOperation)
            {
                Node oldAssignRoot;
                token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                startIndex = endIndex;
                switch (token)
                {
                    case "+":
                        oldAssignRoot = AST.Ast.Instance.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Plus);
                        AST.Ast.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "-":
                        oldAssignRoot = AST.Ast.Instance.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Minus);
                        AST.Ast.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "*":
                        oldAssignRoot = AST.Ast.Instance.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Multiply);
                        AST.Ast.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "/":
                        oldAssignRoot = AST.Ast.Instance.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Divide);
                        AST.Ast.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case ")":
                        if (!possibleBracketClose)
                            throw new Exception(SyntaxDirectory.ERROR);
                        bracketsPaired = true;
                        expectedOperation = true;

                        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);

                        if (token == "*" || token == "/")
                        {
                            if (parent.EntityType == EntityType.Multiply || parent.EntityType == EntityType.Divide)
                            {
                                AST.Ast.Instance.SetChildOfLink(parent, expressionRoot);
                            }
                        }
                        else
                        {
                            AST.Ast.Instance.SetChildOfLink(parent, expressionRoot);
                        }

                        break;
                    default:
                        throw new Exception(SyntaxDirectory.ERROR);
                }
            }
            else
            {
                if (IsVarName(token))
                {
                    if (expressionRoot == null) expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                    else
                    {
                        var nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                        if (nextToken == "*" || nextToken == "/")
                        {
                            if (expressionRoot.EntityType == EntityType.Divide ||
                                expressionRoot.EntityType == EntityType.Multiply)
                            {
                                var rightSide = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                                AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                            }
                            else
                            {
                                Node tinyTreeRoot = null;
                                endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName,
                                    assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                AST.Ast.Instance.SetChildOfLink(tinyTreeRoot, expressionRoot);
                            }
                        }
                        else
                        {
                            var rightSide = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                            AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                        }
                    }

                    var usesVar = new Variable(token);
                    if (ViariableTable.Instance.GetVarIndex(token) == -1)
                    {
                        ViariableTable.Instance.AddVariable(token);
                    }

                    SetUsesForFamily(assignNode, usesVar);
                    startIndex = endIndex;
                    expectedOperation = true;
                }
                else if (IsConstValue(token))
                {
                    if (expressionRoot == null) expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                    else
                    {
                        var nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                        if (nextToken == "*" || nextToken == "/")
                        {
                            if (expressionRoot.EntityType == EntityType.Divide ||
                                expressionRoot.EntityType == EntityType.Multiply)
                            {
                                var rightSide = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                                AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                            }
                            else
                            {
                                Node tinyTreeRoot = null;
                                endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName,
                                    assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                AST.Ast.Instance.SetChildOfLink(tinyTreeRoot, expressionRoot);
                            }
                        }
                        else
                        {
                            var rightSide = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                            AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
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
                        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                        startIndex = endIndex;
                    }
                    else
                    {
                        Node tinyTreeRoot;
                        endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName,
                            assignNode, expressionRoot, true, out tinyTreeRoot);
                        if (expressionRoot == null)
                            expressionRoot = AST.Ast.Instance.GetTNodeDeepCopy(tinyTreeRoot);
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

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token == ";")
            {
                if (inBracket && !bracketsPaired)
                    throw new Exception(SyntaxDirectory.ERROR);
                token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                endAssign = true;
                break;
            }
        }

        if (lineNumber == lines.Count && token != ";")
            throw new Exception(SyntaxDirectory.ERROR);
        return endAssign;
    }

    public void ParseCall(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Call) throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        StatementTable.Instance.AddStatement(EntityType.Call, _lineNumberQuery);
        var callNode = AST.Ast.Instance.CreateTNode(EntityType.Call);
        StatementTable.Instance.SetAstRoot(_lineNumberQuery, callNode);

        AST.Ast.Instance.SetParent(callNode, parent);

        SettingFollows(callNode, stmtListNode, parent);
        AST.Ast.Instance.SetChildOfLink(callNode, stmtListNode);

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (IsVarName(token)) callNode.NodeAttribute.Name = token;
        else throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        startIndex = endIndex;
        if (token != ";") throw new Exception(SyntaxDirectory.ERROR);
    }

    public void ParseIf(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName,
        Node parent, Node stmtListNode)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.If) throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        StatementTable.Instance.AddStatement(EntityType.If, _lineNumberQuery);
        var ifNode = AST.Ast.Instance.CreateTNode(EntityType.If);
        StatementTable.Instance.SetAstRoot(_lineNumberQuery, ifNode);

        AST.Ast.Instance.SetParent(ifNode, parent);
        SettingFollows(ifNode, stmtListNode, parent);
        AST.Ast.Instance.SetChildOfLink(ifNode, stmtListNode);

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (IsVarName(token))
        {
            var var = new Variable(token);
            if (ViariableTable.Instance.GetVarIndex(token) == -1)
            {
                ViariableTable.Instance.AddVariable(token);
            }

            SetUsesForFamily(ifNode, var);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Then) throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);

        Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Else) throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);

        Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
        startIndex = endIndex;
    }

    public void Parse(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName,
        Node parent, [Optional] Node stmtList)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
        switch (token)
        {
            case SyntaxDirectory.Procedure:
                ParseProcedure(lines, startIndex, ref lineNumber, out endIndex, parent);
                break;
            case "{":
                ParseStmtLst(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent);
                break;
            case SyntaxDirectory.While:
                ParseWhile(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                break;
            case SyntaxDirectory.Call:
                ParseCall(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                break;
            case SyntaxDirectory.If:
                ParseIf(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                break;
            default:
                if (IsVarName(token))
                {
                    ParseAssign(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                    break;
                }
                else throw new Exception(SyntaxDirectory.ERROR);
        }
    }

    public bool IsVarName(string name)
    {
        if (name.Length == 0) return false;
        if (!char.IsLetter(name[0])) return false;
        else if (_reservedWords.IndexOf(name) > 0) return false;

        return true;
    }

    public bool IsConstValue(string name) => name.Length != 0 && long.TryParse(name, out _);

    public void StartParse(string code)
    {
        var lines = code.Split(new[] { '\r', '\n' }).ToList();

        if (lines.Count == 0 || (lines.Count == 1 && string.IsNullOrEmpty(lines[0])))
            throw new Exception(SyntaxDirectory.ERROR);

        var lineNumber = 0;
        var index = 0;
        int endIndex;
        string token;
        var countToken = 0;
        while (lineNumber < lines.Count)
        {
            token = GetToken(lines, ref lineNumber, index, out endIndex, true);
            if (token != "") countToken++;
            if (token == "")
            {
                if (countToken > 0) break;
                else throw new Exception(SyntaxDirectory.ERROR);
            }

            var newRoot = AST.Ast.Instance!.CreateTNode(EntityType.Program);
            AST.Ast.Instance.SetRoot(newRoot);

            if (token != SyntaxDirectory.Procedure)
                throw new Exception(SyntaxDirectory.ERROR);
            Parse(lines, index, ref lineNumber, out endIndex, "", newRoot);
            index = endIndex;
        }

        UpdateModifiesAndUsesTablesInProcedures();
        UpdateModifiesAndUsesTablesInWhilesAndIfs();
    }

    public void UpdateModifiesAndUsesTablesInProcedures()
    {
        bool wasChange;
        var sizeOfProcTable = ProcedureTable.Instance!.GetSize();
        do
        {
            wasChange = false;
            for (var i = 0; i < sizeOfProcTable; i++)
            {
                var p1 = ProcedureTable.Instance!.GetProcedure(i);
                if (p1 is null)
                    throw new Exception(SyntaxDirectory.ERROR);
                for (var j = 0; j < sizeOfProcTable; j++)
                {
                    if (i == j) continue;
                    var p2 = ProcedureTable.Instance!.GetProcedure(j);
                    if (p2 is null)
                        throw new Exception(SyntaxDirectory.ERROR);
                    if (!Calls.Calls.Instance!.IsCalls(p1.Identifier, p2.Identifier)) continue;
                    
                    foreach (var variable in p2.ModifiesList.Where(variable => p1.ModifiesList.TryAdd(variable.Key, true)))
                        wasChange = true;

                    foreach (var variable in p2.UsesList)
                        if (!p1.UsesList.ContainsKey(variable.Key))
                        {
                            p1.UsesList[variable.Key] = true;
                            wasChange = true;
                        }
                }
            }
        } while (wasChange);

        foreach (var s in StatementTable.Instance!.StatementsList)
            if (s.StmtType == EntityType.Call)
            {
                var pname = s.AstRoot.NodeAttribute.Name;
                var p = ProcedureTable.Instance.GetProcedure(pname);
                if (p == null) continue;
                s.ModifiesList = p.ModifiesList;
                s.UsesList = p.UsesList;
            }
    }

    public void UpdateModifiesAndUsesTablesInWhilesAndIfs()
    {
        var ifOrWhileStmts = StatementTable.Instance!.StatementsList
            .Where(i => i?.AstRoot.EntityType is EntityType.While or EntityType.If).ToList();

        foreach (var stmt in ifOrWhileStmts.Where(i=>i is not null))
        {
            var node = stmt.AstRoot;
            var stmtLstNodes = AST.Ast.Instance!
                .GetLinkedNodes(node, LinkType.Child)
                .Where(i => i.EntityType == EntityType.Stmtlist).ToList();

            var procedures = new List<Procedure>();
            foreach (var stmtL in stmtLstNodes)
                Calls.Calls.Instance!.GetCalls(procedures, stmtL);

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

    public void CleanData()
    {
        AST.Ast.Instance!.Root = null;
        ViariableTable.Instance!.VariablesList.Clear();
        StatementTable.Instance!.StatementsList.Clear();
        ProcedureTable.Instance!.ProceduresList.Clear();
    }

    private void AddLineNumberQuery(string line, int lineNumber)
    {
        if (line.Contains(SyntaxDirectory.Procedure) || line.Contains(SyntaxDirectory.Else)) return;
        if (lineNumber - _lineNumberOld < 1) return;
        
        _lineNumberQuery++;
        _lineNumberOld = lineNumber;
    }
}