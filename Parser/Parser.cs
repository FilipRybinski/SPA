using System.Runtime.InteropServices;
using Parser.AST.Utils;
using Parser.Interfaces;
using Parser.Tables;
using Parser.Tables.Models;
using Utils.Enums;

namespace Parser;

public class Parser
{
        int _lineNumberQuery = 1;
        int _lineNumberOld = 0;
        readonly List<String> _reservedWords;
        private static readonly IPkb Pkb = global::Parser.Pkb.Instance;
        public Parser()
        {
            _reservedWords = new List<string>();
            _reservedWords.Add("procedure");
            _reservedWords.Add("while");
            _reservedWords.Add("if");
            _reservedWords.Add("then");
            _reservedWords.Add("else");
            _reservedWords.Add("call");
        }
        
        public char GetChar(string line, int index)
        {
            char character = line[index];
            if ((char)9 == character)
                character = ' ';
            return character;
        }
        
        public string GetToken(List<string> lines, ref int lineNumber, int startIndex, out int endIndex, bool test)
        {
            int lineNumberIn = lineNumber;

            string fileLine = "";
            if (startIndex == -1)
            {
                lineNumber++;
                startIndex = 0;
            }
            if (lineNumber >= lines.Count)
            {
                throw new Exception("ParseProcedure: Nieoczekiwany koniec pliku, linia: " + lineNumber);
            }

            string token = "";
            char character;
            while (true)
            {
                fileLine = lines.ElementAt(lineNumber);
                if (startIndex >= fileLine.Length)
                {
                    addLineNumberQuery(fileLine, lineNumber);
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
                        addLineNumberQuery(fileLine, lineNumber);
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

            for (int index = startIndex; index < fileLine.Length; index++)
            {
                character = fileLine[index];
                if (Char.IsLetter(character) || Char.IsDigit(character))
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
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "procedure") throw new Exception("ParseProcedure: Brak słowa procedure, linia: " + lineNumber);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false); 
            Node newNode = AST.Ast.Instance!.CreateTNode(EntityType.Procedure);
            if (IsVarName(token))
            {
                ProcedureTable.Instance!.AddProcedure(token);
                ProcedureTable.Instance.SetAstRootNode(token, newNode);
                AST.Ast.Instance.SetChildOfLink(newNode, parent);
            }
            else throw new Exception("ParseProcedure: Błędna nazwa procedury, " + token + ", linia: " + lineNumber);
            string procedureName = token;
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true); 
            if (token != "{") throw new Exception("ParseProcedure: Brak nawiasu { po nazwie procedury, linia: " + lineNumber);

            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, newNode);
        }
        
        public void ParseStmtLst(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "{") throw new Exception("ParseStmtLst: Brak znaku {, linia: " + lineNumber);
            Node newNode = AST.Ast.Instance!.CreateTNode(EntityType.Stmtlist); 
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
            if (lineNumber == lines.Count && token != "}") throw new Exception("ParseStmtLst: Brak znaku }, linia: " + lineNumber);
        }
        
        public void ParseWhile(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "while") throw new Exception("ParseWhile: Brak słowa kluczowego while, linia: " + lineNumber);
            StatementTable.Instance.AddStatement(EntityType.While, _lineNumberQuery);
            startIndex = endIndex;

            Node whileNode = AST.Ast.Instance!.CreateTNode(EntityType.While); // tworzenie node dla while
            StatementTable.Instance.SetAstRoot(_lineNumberQuery, whileNode);
            AST.Ast.Instance.SetParent(whileNode, parent); //ustawianie parenta dla while

            //Node stmtListNode = AST.AST.Instance.GetNthChild(0, parent);
            SettingFollows(whileNode, stmtListNode, parent);
            AST.Ast.Instance.SetChildOfLink(whileNode, stmtListNode); //łączenie stmlList z while

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (IsVarName(token))
            {
                Node variableNode = AST.Ast.Instance.CreateTNode(EntityType.Variable); // tworzenie node dla zmiennej po lewej stronie while node
                AST.Ast.Instance.SetChildOfLink(variableNode, whileNode);

                Variable var = new Variable(token);
                if (ViariableTable.Instance!.GetVarIndex(token) == -1)
                {
                    ViariableTable.Instance.AddVariable(token);
                }
                SetUsesForFamily(whileNode, var);
            }
            else throw new Exception("ParseWhile: Wymagana nazwa zmiennej, " + token + ", linia: " + lineNumber);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token != "{") throw new Exception("ParseWhile: Brak znaku {, linia: " + lineNumber);

            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, whileNode);
        }

        public void SettingFollows(Node node, Node stmt, Node parent)
        {
            List<Node> siblingsList = AST.Ast.Instance!.GetLinkedNodes(stmt, LinkType.Child); 
            if (siblingsList.Count() != 0)
            {
                Node prevStmt = siblingsList[siblingsList.Count() - 1];
                AST.Ast.Instance.SetFollows(prevStmt, node);
            }

        }

        public void SetModifiesForFamily(Node node, Variable var)
        {
            if (node.EntityType == EntityType.Procedure)
            {
                Procedure proc = ProcedureTable.Instance.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
                var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
                Modifies.Modifies.Instance.SetModifies(proc, var);
            }
            else
            {
                Statement stmt = StatementTable.Instance.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
                var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
                Modifies.Modifies.Instance.SetModifies(stmt, var);
            }
            if (AST.Ast.Instance.GetParent(node) != null) SetModifiesForFamily(AST.Ast.Instance.GetParent(node), var);
        }

        public void SetUsesForFamily(Node node, Variable var)
        {
            if (node.EntityType == EntityType.Procedure)
            {
                Procedure proc = ProcedureTable.Instance.ProceduresList.Where(i => i.AstNodeRoot == node).FirstOrDefault();
                var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
                Uses.Uses.Instance.SetUses(proc, var);
            }
            else
            {
                Statement stmt = StatementTable.Instance.StatementsList.Where(i => i.AstRoot == node).FirstOrDefault();
                var.Id = ViariableTable.Instance.GetVarIndex(var.Identifier);
                Uses.Uses.Instance.SetUses(stmt, var);
            }
            if (AST.Ast.Instance.GetParent(node) != null) SetUsesForFamily(AST.Ast.Instance.GetParent(node), var);
        }
        
        public void ParseAssign(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (!IsVarName(token)) throw new Exception("ParseAssign: Wymagana nazwa zmiennej, " + token + ", linia: " + lineNumber);
            StatementTable.Instance.AddStatement(EntityType.Assign, _lineNumberQuery);
            startIndex = endIndex;

            Node assignNode = AST.Ast.Instance.CreateTNode(EntityType.Assign);
            Variable var = new Variable(token);
            ViariableTable.Instance.AddVariable(token);
            StatementTable.Instance.SetAstRoot(_lineNumberQuery, assignNode);
            AST.Ast.Instance.SetParent(assignNode, parent);
            SettingFollows(assignNode, stmtListNode, parent);
            AST.Ast.Instance.SetChildOfLink(assignNode, stmtListNode);
            Node variableNode = AST.Ast.Instance.CreateTNode(EntityType.Variable); 
            AST.Ast.Instance.SetChildOfLink(variableNode, assignNode);
            SetModifiesForFamily(assignNode, var); 
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "=") throw new Exception("ParseAssign: Brak znaku =, linia: " + lineNumber);
            startIndex = endIndex;
            
            Node expressionRoot;
            ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, assignNode, false, out expressionRoot);
            AST.Ast.Instance.SetChildOfLink(expressionRoot, assignNode);
            startIndex = endIndex;
        }
        public void ParseAssignOld(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (!IsVarName(token)) throw new Exception("ParseAssign: Wymagana nazwa zmiennej, " + token + ", linia: " + lineNumber);
            StatementTable.Instance.AddStatement(EntityType.Assign, _lineNumberQuery);
            startIndex = endIndex;

            Node assignNode = AST.Ast.Instance.CreateTNode(EntityType.Assign); 
            Variable var = new Variable(token);
            ViariableTable.Instance.AddVariable(token);
            StatementTable.Instance.SetAstRoot(_lineNumberQuery, assignNode);
            AST.Ast.Instance.SetParent(assignNode, parent); 

            Node stmtListNode = AST.Ast.Instance.GetNthChild(0, parent);
            SettingFollows(assignNode, stmtListNode, parent);
            AST.Ast.Instance.SetChildOfLink(assignNode, stmtListNode);
            Node variableNode = AST.Ast.Instance.CreateTNode(EntityType.Variable);
            AST.Ast.Instance.SetChildOfLink(variableNode, assignNode);
            SetModifiesForFamily(assignNode, var); 

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "=") throw new Exception("ParseAssign: Brak znaku =, linia: " + lineNumber);
            startIndex = endIndex;

            token = "";
            bool expectedOperation = false;
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
                            throw new Exception("ParseAssign: Nieobsługiwane działanie, " + token + ", linia: " + lineNumber);
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

                            Variable usesVar = new Variable(token); 
                            SetUsesForFamily(assignNode, usesVar);
                        }
                        else
                        {
                            Node rightSide = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                            AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);

                            Variable usesVar = new Variable(token); 
                            SetUsesForFamily(assignNode, usesVar);
                        }
                    }
                    else if (IsConstValue(token)) 
                    {
                        if (expressionRoot == null) expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                        else
                        {
                            Node rightSide = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                            AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                        }
                    }
                    else throw new Exception("ParseAssign: Spodziewana zmienna lub stała, " + token + ", linia: " + lineNumber);
                    expectedOperation = true;
                }
                token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
                if (token == ";")
                {
                    token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                    break;
                }
            }
            if (lineNumber == lines.Count && token != ";") throw new Exception("ParseAssign: Spodziewano się znaku ; linia: " + lineNumber);

            //łączenie tyci drzewka expresion z assign
            AST.Ast.Instance.SetChildOfLink(expressionRoot, assignNode);
        }
        public bool ParseExpr(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node assignNode, Node parent, bool inBracket, out Node expressionRoot, string prevToken = "")
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
                            if (!possibleBracketClose) throw new Exception("ParseExpr: niespodziewany znak ), linia: " + lineNumber);
                            bracketsPaired = true;
                            expectedOperation = true;

                            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);

                            if (token == "*" || token == "/")
                            {
                                if (parent.EntityType == EntityType.Multiply || parent.EntityType == EntityType.Divide)
                                {
                                    AST.Ast.Instance.SetChildOfLink(parent, expressionRoot);
                                    //return expressionRoot;
                                }
                            }
                            else
                            {
                                AST.Ast.Instance.SetChildOfLink(parent, expressionRoot);
                                //return expressionRoot;
                            }
                            break;
                        default:
                            throw new Exception("ParseExpr: Nieobsługiwane działanie, " + token + ", linia: " + lineNumber);
                    }
                }
                else 
                {
                    if (IsVarName(token)) 
                    {

                        if (expressionRoot == null) expressionRoot = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                        else
                        {
                            string nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                            if (nextToken == "*" || nextToken == "/")
                            {
                                if (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply)
                                {
                                    Node rightSide = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                                    AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                                }
                                else
                                {
                                    Node tinyTreeRoot = null;
                                    endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                    AST.Ast.Instance.SetChildOfLink(tinyTreeRoot, expressionRoot);
                                }
                            }
                            else
                            {
                                Node rightSide = AST.Ast.Instance.CreateTNode(EntityType.Variable);
                                AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                            }
                        }
                        Variable usesVar = new Variable(token); 
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
                            string nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                            if (nextToken == "*" || nextToken == "/")
                            {
                                if (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply)
                                {
                                    Node rightSide = AST.Ast.Instance.CreateTNode(EntityType.Constant);
                                    AST.Ast.Instance.SetChildOfLink(rightSide, expressionRoot);
                                }
                                else
                                {
                                    Node tinyTreeRoot = null;
                                    endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                    AST.Ast.Instance.SetChildOfLink(tinyTreeRoot, expressionRoot);
                                }
                            }
                            else
                            {
                                Node rightSide = AST.Ast.Instance.CreateTNode(EntityType.Constant);
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
                            endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, expressionRoot, true, out tinyTreeRoot);
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
                    else throw new Exception("ParseExpr: Spodziewana zmienna lub stała, " + token + ", linia: " + lineNumber);
                }

                token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
                if (token == ";")
                {
                    if (inBracket && !bracketsPaired) throw new Exception("ParseExpr: Brak nawiasu zamykajacego, wystapil " + token + ", linia: " + lineNumber);
                    token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                    endAssign = true;
                    break;
                }
            }
            if (lineNumber == lines.Count && token != ";") throw new Exception("ParseExpr: Spodziewano się znaku ; linia: " + lineNumber);
            return endAssign;
        }
        
        public void ParseCall(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "call") throw new Exception("ParseCall: Brak słowa kluczowego call, linia: " + lineNumber);

            startIndex = endIndex;

            StatementTable.Instance.AddStatement(EntityType.Call, _lineNumberQuery);
            Node callNode = AST.Ast.Instance.CreateTNode(EntityType.Call);
            StatementTable.Instance.SetAstRoot(_lineNumberQuery, callNode);

            AST.Ast.Instance.SetParent(callNode, parent);

            SettingFollows(callNode, stmtListNode, parent); 
            AST.Ast.Instance.SetChildOfLink(callNode, stmtListNode);

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (IsVarName(token)) callNode.NodeAttribute.Name = token;
            else throw new Exception("ParseCall: Wymagana nazwa procedury, " + token + ", linia: " + lineNumber);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            startIndex = endIndex;
            if (token != ";") throw new Exception("ParseCall: Brak znaku ; linia: " + lineNumber);
        }
        
        public void ParseIf(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "if") throw new Exception("ParseIf: Brak słowa kluczowego if, linia: " + lineNumber);

            startIndex = endIndex;

            StatementTable.Instance.AddStatement(EntityType.If, _lineNumberQuery);
            Node ifNode = AST.Ast.Instance.CreateTNode(EntityType.If);
            StatementTable.Instance.SetAstRoot(_lineNumberQuery, ifNode);

            AST.Ast.Instance.SetParent(ifNode, parent);
            SettingFollows(ifNode, stmtListNode, parent); 
            AST.Ast.Instance.SetChildOfLink(ifNode, stmtListNode);

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (IsVarName(token))
            {
                Variable var = new Variable(token);
                if (ViariableTable.Instance.GetVarIndex(token) == -1)
                {
                    ViariableTable.Instance.AddVariable(token);
                }
                SetUsesForFamily(ifNode, var);
            }
            else throw new Exception("ParseIf: Wymagana nazwa zmiennej, " + token + ", linia: " + lineNumber);
            startIndex = endIndex;
            
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "then") throw new Exception("ParseIf: Brak słowa kluczowego then, linia: " + lineNumber);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token != "{") throw new Exception("ParseIf: Brak znaku {, linia: " + lineNumber);

            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "else") throw new Exception("ParseIf: Brak słowa kluczowego else, linia: " + lineNumber);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token != "{") throw new Exception("ParseIf: Brak znaku {, linia: " + lineNumber);

            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
            startIndex = endIndex;
        }

        public void Parse(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, [Optional] Node stmtList)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            switch (token)
            {
                case "procedure":
                    ParseProcedure(lines, startIndex, ref lineNumber, out endIndex, parent);
                    break;
                case "{":
                    ParseStmtLst(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent);
                    break;
                case "while":
                    ParseWhile(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                    break;
                case "call":
                    ParseCall(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                    break;
                case "if":
                    ParseIf(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                    break;
                default:
                    if (IsVarName(token))
                    {
                        ParseAssign(lines, startIndex, ref lineNumber, out endIndex, procedureName, parent, stmtList);
                        break;
                    }
                    else throw new Exception("Parse: Niespodziewany token: " + token + ", linia: " + lineNumber);
            }
        }

        public bool IsVarName(string name)
        {
            if (name.Length == 0) return false;
            if (!Char.IsLetter(name[0])) return false;
            else if (_reservedWords.IndexOf(name) > 0) return false;

            return true;
        }

        public bool IsConstValue(string name)
        {
            long test;
            if (name.Length == 0) return false;
            return Int64.TryParse(name, out test);
        }
        public void StartParse(string code)
        {

            List<string> lines = code.Split(new[] { '\r', '\n' }).ToList();

            if (lines.Count == 0 || (lines.Count == 1 && string.IsNullOrEmpty(lines[0])))

                throw new Exception("# StartParse: Pusty kod");

            int lineNumber = 0;
            int index = 0;
            int endIndex;
            string token;
            int countToken = 0;
            while (lineNumber < lines.Count) 
            {
                token = GetToken(lines, ref lineNumber, index, out endIndex, true);
                if (token != "") countToken++;
                if (token == "")
                {
                    if (countToken > 0) break; // nastapil koniec pliku
                    else throw new Exception("StartParse: Pusty kod");
                }
                Node newRoot = AST.Ast.Instance.CreateTNode(EntityType.Program);
                AST.Ast.Instance.SetRoot(newRoot);

                if (token != "procedure") throw new Exception("StartParse: Spodziewano się słowa kluczowego procedure, linia: " + lineNumber);
                Parse(lines, index, ref lineNumber, out endIndex, "", newRoot);
                index = endIndex;
            }
            UpdateModifiesAndUsesTablesInProcedures();
            UpdateModifiesAndUsesTablesInWhilesAndIfs();
        }

        public void UpdateModifiesAndUsesTablesInProcedures()
        {
            bool wasChange;
            int sizeOfProcTable = ProcedureTable.Instance.GetSize();
            do
            {
                wasChange = false;
                for (int i = 0; i < sizeOfProcTable; i++)
                {
                    Procedure p1 = ProcedureTable.Instance.GetProcedure(i);
                    for (int j = 0; j < sizeOfProcTable; j++)
                    {
                        if (i != j)
                        {
                            Procedure p2 = ProcedureTable.Instance.GetProcedure(j);
                            if (Calls.Calls.Instance.IsCalls(p1.Identifier, p2.Identifier))
                            {
                                foreach (KeyValuePair<int, bool> variable in p2.ModifiesList)
                                    if (!p1.ModifiesList.ContainsKey(variable.Key))
                                    {
                                        p1.ModifiesList[variable.Key] = true;
                                        wasChange = true;
                                    }

                                foreach (KeyValuePair<int, bool> variable in p2.UsesList)
                                    if (!p1.UsesList.ContainsKey(variable.Key))
                                    {
                                        p1.UsesList[variable.Key] = true;
                                        wasChange = true;
                                    }

                            }
                        }

                    }
                }
            } while (wasChange);

            foreach (Statement s in StatementTable.Instance.StatementsList)
                if (s.StmtType == EntityType.Call)
                {
                    string pname = s.AstRoot.NodeAttribute.Name;
                    Procedure p = ProcedureTable.Instance.GetProcedure(pname);
                    if (p != null)
                    {
                        s.ModifiesList = p.ModifiesList;
                        s.UsesList = p.UsesList;
                    }
                }
        }

        public void UpdateModifiesAndUsesTablesInWhilesAndIfs()
        {
            List<Statement> ifOrWhileStmts = StatementTable.Instance.StatementsList
                .Where(i => i.AstRoot.EntityType == EntityType.While || i.AstRoot.EntityType == EntityType.If).ToList();

            foreach(var stmt in ifOrWhileStmts)
            {
                var node = stmt.AstRoot;
                List<Node> stmtLstNodes = AST.Ast.Instance
                .GetLinkedNodes(node, LinkType.Child)
                .Where(i => i.EntityType == EntityType.Stmtlist).ToList();

                List<Procedure> procedures = new List<Procedure>();
                foreach(var stmtL in stmtLstNodes)
                {
                    Calls.Calls.Instance.GetCalls(procedures, stmtL);
                }

                foreach(Procedure proc in procedures)
                {
                    foreach (KeyValuePair<int, bool> variable in proc.ModifiesList)
                        if (!stmt.ModifiesList.ContainsKey(variable.Key))
                        {
                            stmt.ModifiesList[variable.Key] = true;
                        }
                    foreach (KeyValuePair<int, bool> variable in proc.UsesList)
                        if (!stmt.UsesList.ContainsKey(variable.Key))
                        {
                            stmt.UsesList[variable.Key] = true;
                        }
                }
            
            }
        }

        public void CleanData()
        {
            AST.Ast.Instance.Root = null;
            ViariableTable.Instance.VariablesList.Clear();
            StatementTable.Instance.StatementsList.Clear();
            ProcedureTable.Instance.ProceduresList.Clear();
        }

        private void addLineNumberQuery(string line, int lineNumber)
        {
            if (!line.Contains("procedure") && !line.Contains("else"))
            {
                if (lineNumber - _lineNumberOld >= 1)
                {
                    _lineNumberQuery++;
                    _lineNumberOld = lineNumber;
                }
            }
        }
}