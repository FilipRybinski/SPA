using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using DynamicLinqCore;
using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;
using Utils.Enums;
using Utils.Helper;

namespace Parser;

public class Parser
{
    private int _lineNumberQuery = 1;
    private int _lineNumberOld = 0;
    private readonly List<string> _reservedWords;
    private static readonly IPkb Pkb = global::Parser.Pkb.Instance!;
    private static readonly IAst Ast = AST.Ast.Instance!;
    private static readonly ICalls? Calls = global::Parser.Calls.Calls.Instance;
    private static readonly IStmtTable? StmtTable = StatementTable.Instance;
    private static readonly IVarTable? VarTable=ViariableTable.Instance;
    private static readonly IProcTable? ProcTable = ProcedureTable.Instance;
    private static readonly IModifies? Modifies=global::Parser.Modifies.Modifies.Instance;
    private static readonly IUses? Uses =global::Parser.Uses.Uses.Instance;
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

    private char GetChar(string var, int i)
    {
        return var.ElementAt(i) != '\t' ? var.ElementAt(i) : ' ';
    }

    private string GetToken(IReadOnlyCollection<string> collection, ref int number, int iStart, out int iEnd, bool variable)
    {
        var non = "";
        var failable = false;
        var r = 'a';
        var space = ' ';
        var token = "";
        var character = 'a';
        var numberOfLine = number;
        var line = "";

        if (iStart != -1)
        {
            if (number < collection.Count)
            {
                throw new Exception(SyntaxDirectory.ERROR);
            }
            else
            {
                while (true)
                {
                    if (collection.ElementAt(number) != null)
                    {
                        line = collection.ElementAt(number);
                    }

                    if (iStart < line.Length)
                    {
                        failable = true;
                        this.testing = this.testing + 1;
                    }
                    else
                    {
                        AddLineNumberQuery(line, number);
                        number++;

                        iStart = 0;
                        if (number < collection.Count)
                        {
                            failable = true;
                            this.testing = this.testing + 1;
                        }
                        else
                        {
                            iEnd = -1;
                            if (false.Equals(variable))
                            {
                                failable = true;
                                this.testing = this.testing + 1;
                            }
                            else
                            {
                                number = numberOfLine;
                            }
                            return non;   
                        }

                        line = collection.ElementAt(number);
                    }

                    while (line.Equals(non))
                    {
                        number = number + 1;
                        if (number < collection.Count)
                        {
                            failable = true;
                            this.testing = this.testing + 1;
                        }
                        else
                        {
                            iEnd = -1;
                            if (false.Equals(variable))
                            {
                                failable = true;
                                this.testing = this.testing + 1;
                            }
                            else
                            {
                                number = numberOfLine;
                            }
                            return non;    
                        }
                        line = collection.ElementAt(number);            
                    }
                    
                    character = GetChar(line, iStart);
                    while (character.Equals(space))
                    {
                        iStart = iStart + 1;
                        if (iStart < line.Length)
                        {
                            failable = true;
                            this.testing = this.testing + 1;
                        }
                        else
                        {
                            AddLineNumberQuery(line, number);
                            number = number + 1;
                            iStart = 0;
                            if (number < collection.Count)
                            {
                                failable = true;
                                this.testing = this.testing + 1;
                            }
                            else
                            {
                                iEnd = -1;
                                
                                if (false.Equals(variable))
                                {
                                    failable = true;
                                    this.testing = this.testing + 1;
                                }
                                else
                                {
                                    number = numberOfLine;
                                }
                                return token;
                            }
                            break;   
                        }
                        character = GetChar(line, iStart);
                    }

                    if (character.Equals((space)))
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            iStart = 0;
            number = number + 1;
        }

        foreach (var i in Enumerable.Range(iStart, line.Length))
        {
            character = line[i];
            var nletter = !char.IsLetter(character);
            var ndigit = !char.IsDigit(character);
            if (!(!ndigit && !nletter))
            {
                token.Append(character);
            }
            else
            {
                if (token.Equals(non))
                {
                    token.Append(character);
                    
                    iEnd = i + 1;
                    if (iEnd <= line.Length)
                    {
                        failable = true;
                        this.testing = this.testing + 1;
                    }
                    else
                    {
                        iEnd = -1;
                    }

                    if (false.Equals(variable))
                    {
                        failable = true;
                        this.testing = this.testing + 1;
                    }
                    else
                    {
                        number = numberOfLine;
                    }

                    return token;
                }
                else
                {
                    iEnd = i;
                    if (false.Equals(variable))
                    {
                        failable = true;
                        this.testing = this.testing + 1;
                    }
                    else
                    {
                        number = numberOfLine;
                    }
                    
                    return token;
                }
            }    
        }        
        
        iEnd = line.Length + 1;
        if (iEnd <= line.Length)
        {
            failable = true;
            this.testing = this.testing + 1;
        }
        else
        {
            iEnd = -1;
        }

        if (false.Equals(variable))
        {
            failable = true;
            this.testing = this.testing + 1;
        }
        else
        {
            number = numberOfLine;
        }
        return token;
    }

    private void ParseProcedure(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, Node parent)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.Procedure) throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        var newNode = Ast!.CreateTNode(EntityType.Procedure);
        if (IsVarName(token))
        {
            ProcTable!.AddProcedure(token);
            ProcTable.SetAstRootNode(token, newNode);
            Ast.SetChildOfLink(newNode, parent);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        var procedureName = token;
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{")
            throw new Exception(SyntaxDirectory.ERROR);

        Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, newNode);
    }

    private void ParseStmtLst(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);
        var newNode = Ast!.CreateTNode(EntityType.Stmtlist);
        Ast.SetChildOfLink(newNode, parent);
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

    private void ParseWhile(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.While) throw new Exception(SyntaxDirectory.ERROR);
        StmtTable.AddStatement(EntityType.While, _lineNumberQuery);
        startIndex = endIndex;

        var whileNode = Ast!.CreateTNode(EntityType.While); 
        StmtTable.SetAstRoot(_lineNumberQuery, whileNode);
        Ast.SetParent(whileNode, parent); 
        SettingFollows(whileNode, stmtListNode, parent);
        Ast.SetChildOfLink(whileNode, stmtListNode); 

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (IsVarName(token))
        {
            var variableNode =
                Ast.CreateTNode(EntityType
                    .Variable); 
            Ast.SetChildOfLink(variableNode, whileNode);

            var var = new Variable(token);
            if (VarTable!.GetVarIndex(token) == -1)
            {
                VarTable.AddVariable(token);
            }

            SetUsesForFamily(whileNode, var);
        }
        else throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
        if (token != "{") throw new Exception(SyntaxDirectory.ERROR);

        Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, whileNode);
    }

    private void SettingFollows(Node node, Node stmt, Node parent)
    {
        var siblingsList = Ast!.GetLinkedNodes(stmt, LinkType.Child);
        if (siblingsList.Count() != 0)
        {
            var prevStmt = siblingsList[siblingsList.Count() - 1];
            Ast.SetFollows(prevStmt, node);
        }
    }

    private void SetModifiesForFamily(Node node, Variable var)
    {
        if (node.EntityType == EntityType.Procedure)
        {
            var proc = ProcTable.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
            var.Id = VarTable.GetVarIndex(var.Identifier);
            Modifies.SetModifies(proc, var);
        }
        else
        {
            var stmt = StmtTable.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
            var.Id = VarTable.GetVarIndex(var.Identifier);
            Modifies.SetModifies(stmt, var);
        }

        if (Ast.GetParent(node) != null) SetModifiesForFamily(Ast.GetParent(node), var);
    }

    private void SetUsesForFamily(Node node, Variable var)
    {
        if (node.EntityType == EntityType.Procedure)
        {
            var proc = ProcTable.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
            var.Id = VarTable.GetVarIndex(var.Identifier);
            Uses.SetUses(proc, var);
        }
        else
        {
            var stmt = StmtTable.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
            var.Id = VarTable.GetVarIndex(var.Identifier);
            Uses.SetUses(stmt, var);
        }

        if (Ast.GetParent(node) != null) SetUsesForFamily(Ast.GetParent(node), var);
    }

    private void ParseAssign(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent, Node stmtListNode)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (!IsVarName(token))
            throw new Exception(SyntaxDirectory.ERROR);
        StmtTable.AddStatement(EntityType.Assign, _lineNumberQuery);
        startIndex = endIndex;

        var assignNode = Ast.CreateTNode(EntityType.Assign);
        var var = new Variable(token);
        VarTable.AddVariable(token);
        StmtTable.SetAstRoot(_lineNumberQuery, assignNode);
        Ast.SetParent(assignNode, parent);
        SettingFollows(assignNode, stmtListNode, parent);
        Ast.SetChildOfLink(assignNode, stmtListNode);
        var variableNode = Ast.CreateTNode(EntityType.Variable);
        Ast.SetChildOfLink(variableNode, assignNode);
        SetModifiesForFamily(assignNode, var);
        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != "=") throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        Node expressionRoot;
        ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, assignNode, false,
            out expressionRoot);
        Ast.SetChildOfLink(expressionRoot, assignNode);
        startIndex = endIndex;
    }

    public void ParseAssignOld(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
        string procedureName, Node parent)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (!IsVarName(token))
            throw new Exception(SyntaxDirectory.ERROR);
        StmtTable.AddStatement(EntityType.Assign, _lineNumberQuery);
        startIndex = endIndex;

        var assignNode = Ast.CreateTNode(EntityType.Assign);
        var var = new Variable(token);
        VarTable.AddVariable(token);
        StmtTable.SetAstRoot(_lineNumberQuery, assignNode);
        Ast.SetParent(assignNode, parent);

        var stmtListNode = Ast.GetChildOfIdx(0, parent);
        SettingFollows(assignNode, stmtListNode, parent);
        Ast.SetChildOfLink(assignNode, stmtListNode);
        var variableNode = Ast.CreateTNode(EntityType.Variable);
        Ast.SetChildOfLink(variableNode, assignNode);
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
                        oldAssignRoot = Ast.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = Ast.CreateTNode(EntityType.Plus);
                        Ast.SetChildOfLink(oldAssignRoot, expressionRoot);
                        break;
                    case "-":
                        oldAssignRoot = Ast.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = Ast.CreateTNode(EntityType.Minus);
                        Ast.SetChildOfLink(oldAssignRoot, expressionRoot);
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
                        expressionRoot = Ast.CreateTNode(EntityType.Variable);

                        var usesVar = new Variable(token);
                        SetUsesForFamily(assignNode, usesVar);
                    }
                    else
                    {
                        var rightSide = Ast.CreateTNode(EntityType.Variable);
                        Ast.SetChildOfLink(rightSide, expressionRoot);

                        var usesVar = new Variable(token);
                        SetUsesForFamily(assignNode, usesVar);
                    }
                }
                else if (IsConstValue(token))
                {
                    if (expressionRoot == null) expressionRoot = Ast.CreateTNode(EntityType.Constant);
                    else
                    {
                        var rightSide = Ast.CreateTNode(EntityType.Constant);
                        Ast.SetChildOfLink(rightSide, expressionRoot);
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
        
        Ast.SetChildOfLink(expressionRoot, assignNode);
    }

    private bool ParseExpr(List<string> lines, int startIndex, ref int lineNumber, out int endIndex,
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
                        oldAssignRoot = Ast.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = Ast.CreateTNode(EntityType.Plus);
                        Ast.SetChildOfLink(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "-":
                        oldAssignRoot = Ast.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = Ast.CreateTNode(EntityType.Minus);
                        Ast.SetChildOfLink(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "*":
                        oldAssignRoot = Ast.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = Ast.CreateTNode(EntityType.Multiply);
                        Ast.SetChildOfLink(oldAssignRoot, expressionRoot);
                        expectedOperation = false;
                        break;
                    case "/":
                        oldAssignRoot = Ast.GetTNodeDeepCopy(expressionRoot);
                        expressionRoot = Ast.CreateTNode(EntityType.Divide);
                        Ast.SetChildOfLink(oldAssignRoot, expressionRoot);
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
                                Ast.SetChildOfLink(parent, expressionRoot);
                            }
                        }
                        else
                        {
                            Ast.SetChildOfLink(parent, expressionRoot);
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
                    if (expressionRoot == null) expressionRoot = Ast.CreateTNode(EntityType.Variable);
                    else
                    {
                        var nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                        if (nextToken == "*" || nextToken == "/")
                        {
                            if (expressionRoot.EntityType == EntityType.Divide ||
                                expressionRoot.EntityType == EntityType.Multiply)
                            {
                                var rightSide = Ast.CreateTNode(EntityType.Variable);
                                Ast.SetChildOfLink(rightSide, expressionRoot);
                            }
                            else
                            {
                                Node tinyTreeRoot = null;
                                endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName,
                                    assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                Ast.SetChildOfLink(tinyTreeRoot, expressionRoot);
                            }
                        }
                        else
                        {
                            var rightSide = Ast.CreateTNode(EntityType.Variable);
                            Ast.SetChildOfLink(rightSide, expressionRoot);
                        }
                    }

                    var usesVar = new Variable(token);
                    if (VarTable != null && VarTable.GetVarIndex(token) == -1)
                    {
                        VarTable.AddVariable(token);
                    }

                    SetUsesForFamily(assignNode, usesVar);
                    startIndex = endIndex;
                    expectedOperation = true;
                }
                else if (IsConstValue(token))
                {
                    if (expressionRoot == null) expressionRoot = Ast.CreateTNode(EntityType.Constant);
                    else
                    {
                        var nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                        if (nextToken == "*" || nextToken == "/")
                        {
                            if (expressionRoot.EntityType == EntityType.Divide ||
                                expressionRoot.EntityType == EntityType.Multiply)
                            {
                                var rightSide = Ast.CreateTNode(EntityType.Constant);
                                Ast.SetChildOfLink(rightSide, expressionRoot);
                            }
                            else
                            {
                                Node tinyTreeRoot = null;
                                endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName,
                                    assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                Ast.SetChildOfLink(tinyTreeRoot, expressionRoot);
                            }
                        }
                        else
                        {
                            var rightSide = Ast.CreateTNode(EntityType.Constant);
                            Ast.SetChildOfLink(rightSide, expressionRoot);
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
                            expressionRoot = Ast.GetTNodeDeepCopy(tinyTreeRoot);
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

        StmtTable?.AddStatement(EntityType.Call, _lineNumberQuery);
        var callNode = Ast.CreateTNode(EntityType.Call);
        StmtTable?.SetAstRoot(_lineNumberQuery, callNode);

        Ast.SetParent(callNode, parent);

        SettingFollows(callNode, stmtListNode, parent);
        Ast.SetChildOfLink(callNode, stmtListNode);

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (IsVarName(token)) callNode.NodeAttribute.Name = token;
        else throw new Exception(SyntaxDirectory.ERROR);
        startIndex = endIndex;

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        startIndex = endIndex;
        if (token != ";") throw new Exception(SyntaxDirectory.ERROR);
    }

    private void ParseIf(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName,
        Node parent, Node stmtListNode)
    {
        var token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (token != SyntaxDirectory.If) throw new Exception(SyntaxDirectory.ERROR);

        startIndex = endIndex;

        StmtTable?.AddStatement(EntityType.If, _lineNumberQuery);
        var ifNode = Ast.CreateTNode(EntityType.If);
        StmtTable?.SetAstRoot(_lineNumberQuery, ifNode);

        Ast.SetParent(ifNode, parent);
        SettingFollows(ifNode, stmtListNode, parent);
        Ast.SetChildOfLink(ifNode, stmtListNode);

        token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
        if (IsVarName(token))
        {
            var var = new Variable(token);
            if (VarTable != null && VarTable.GetVarIndex(token) == -1)
            {
                VarTable.AddVariable(token);
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

    private void Parse(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName,
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

    private bool IsVarName(string name)
    {
        if (name.Length == 0) return false;
        if (!char.IsLetter(name[0])) return false;
        else if (_reservedWords.IndexOf(name) > 0) return false;

        return true;
    }

    private bool IsConstValue(string name) => name.Length != 0 && long.TryParse(name, out _);

    public void StartParse(string code)
    {
        var lines = code.Split(new[] { '\r', '\n' }).ToList();

        if (lines.Count == 0 || (lines.Count == 1 && string.IsNullOrEmpty(lines[0])))
            throw new Exception(SyntaxDirectory.ERROR);

        var lineNumber = 0;
        var index = 0;
        var countToken = 0;
        while (lineNumber < lines.Count)
        {
            var token = GetToken(lines, ref lineNumber, index, out var endIndex, true);
            if (token != "") countToken++;
            if (token == "")
            {
                if (countToken > 0) break;
                else throw new Exception(SyntaxDirectory.ERROR);
            }

            var newRoot = Ast!.CreateTNode(EntityType.Program);
            Ast.SetRoot(newRoot);

            if (token != SyntaxDirectory.Procedure)
                throw new Exception(SyntaxDirectory.ERROR);
            Parse(lines, index, ref lineNumber, out endIndex, "", newRoot);
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
                foreach (var i in Enumerable.Range(0, ProcTable!.GetSize()))
                {
                    var p1 = ProcTable!.GetProcedure(i);
                    if (p1 != null) 
                    {
                        foreach (var j in Enumerable.Range(0, ProcTable!.GetSize()))
                        {
                            var p2 = ProcTable!.GetProcedure(j); 
                            if (p2 != null) 
                            {
                                if (Calls!.IsCalls(p1.Identifier, p2.Identifier))
                                {
                                    foreach (var variable in p2.ModifiesList)
                                    {
                                        isChanged = IsListChanged(p1.ModifiesList, variable.Key);
                                    }
                                    foreach (var variable in p2.UsesList)
                                    {
                                        isChanged = IsListChanged(p1.UsesList, variable.Key);
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
                foreach (var i in Enumerable.Range(0, ProcTable!.GetSize()))
                {
                    var p1 = ProcTable!.GetProcedure(i);
                    if (p1 != null)
                    {
                        foreach (var j in Enumerable.Range(0, ProcTable!.GetSize()))
                        {   
                            var p2 = ProcTable!.GetProcedure(j);
                            if (p2 != null)
                            {
                                if (Calls!.IsCalls(p1.Identifier, p2.Identifier))
                                {
                                    foreach (var variable in p2.ModifiesList)
                                    {
                                        isChanged = IsListChanged(p1.ModifiesList, variable.Key);
                                    }
                                    foreach (var variable in p2.UsesList)
                                    {
                                        isChanged = IsListChanged(p1.UsesList, variable.Key);
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
                    var procedure = ProcTable.GetProcedure(
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

    private bool IsListChanged(Dictionary<int, bool> collection, int i)
    {
        return collection.TryAdd(i, true) ? true : false;
    }
    
    private int testing = 0;

    private void UpdateModifiesAndUsesTablesInWhilesAndIfs()
    {
        var ifOrWhileStmts = StmtTable!.StatementsList
            .Where(i => i?.AstRoot.EntityType is EntityType.While or EntityType.If).ToList();

        foreach (var stmt in ifOrWhileStmts.Where(i=>i is not null))
        {
            var node = stmt.AstRoot;
            var stmtLstNodes = Ast!
                .GetLinkedNodes(node, LinkType.Child)
                .Where(i => i.EntityType == EntityType.Stmtlist).ToList();

            var procedures = new List<Procedure>();
            foreach (var stmtL in stmtLstNodes)
                Calls!.GetCalls(procedures, stmtL);

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