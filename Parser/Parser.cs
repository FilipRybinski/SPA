using System.Runtime.InteropServices;
using Parser.AST.Enums;
using Parser.AST.Utils;
using Parser.Tables;

namespace Parser;

public class Parser
{
        int lineNumberQuery = 1;
        int lineNumberOld = 0;
        List<String> reservedWords; // lista słów kluczowych
        public Parser()
        {
            reservedWords = new List<string>();
            reservedWords.Add("procedure");
            reservedWords.Add("while");
            reservedWords.Add("if");
            reservedWords.Add("then");
            reservedWords.Add("else");
            reservedWords.Add("call");
        }


        /// <summary>
        /// wczytanie znaku z linii
        /// </summary>
        /// <param name="line">odczytywana linia</param>
        /// <param name="index">indeks, ktory ma byc wczytany</param>
        /// <returns></returns>
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

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false); // wczytanie nazwy procedury
            Node newNode = AST.AST.Instance.CreateTNode(EntityType.Procedure);
            if (IsVarName(token))
            {
                ProcTable.Instance.InsertProc(token);
                ProcTable.Instance.SetAstRoot(token, newNode);
                AST.AST.Instance.SetChildOfLink(newNode, parent);
            }
            else throw new Exception("ParseProcedure: Błędna nazwa procedury, " + token + ", linia: " + lineNumber);
            string procedureName = token;
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true); // wczytanie {
            if (token != "{") throw new Exception("ParseProcedure: Brak nawiasu { po nazwie procedury, linia: " + lineNumber);

            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, newNode);
        }


        /// <summary>
        /// Parsowanie listy instrukcji (np. dla procedury albo pętli while)
        /// </summary>
        /// <param name="lines">linie kodu SIMPLE</param>
        /// <param name="lineNumber">aktualnie przetwarzana linia</param>
        /// <param name="startIndex">indeks od którego ma zacząć czytać w danej linii</param>
        /// <param name="endIndex">indeks, na którym skończyło czytać w danej linii</param>
        /// <param name="procedureName">nazwa przetwarzanej procedury</param>
        public void ParseStmtLst(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "{") throw new Exception("ParseStmtLst: Brak znaku {, linia: " + lineNumber);
            Node newNode = AST.AST.Instance.CreateTNode(EntityType.Stmtlist); // tworzenie i łączenie stmtList z parentem
            AST.AST.Instance.SetChildOfLink(newNode, parent);
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

        /// <summary>
        /// Parsowanie pętli while
        /// </summary>
        /// <param name="lines">linie kodu SIMPLE</param>
        /// <param name="lineNumber">aktualnie przetwarzana linia</param>
        /// <param name="startIndex">indeks od którego ma zacząć czytać w danej linii</param>
        /// <param name="endIndex">indeks, na którym skończyło czytać w danej linii</param>
        /// <param name="procedureName">nazwa przetwarzanej procedury</param>
        public void ParseWhile(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "while") throw new Exception("ParseWhile: Brak słowa kluczowego while, linia: " + lineNumber);
            StmtTable.Instance.InsertStmt(EntityType.While, lineNumberQuery);
            startIndex = endIndex;

            Node whileNode = AST.AST.Instance.CreateTNode(EntityType.While); // tworzenie node dla while
            StmtTable.Instance.SetAstRoot(lineNumberQuery, whileNode);
            AST.AST.Instance.SetParent(whileNode, parent); //ustawianie parenta dla while

            //Node stmtListNode = AST.AST.Instance.GetNthChild(0, parent);
            SettingFollows(whileNode, stmtListNode, parent);
            AST.AST.Instance.SetChildOfLink(whileNode, stmtListNode); //łączenie stmlList z while

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (IsVarName(token))
            {
                Node variableNode = AST.AST.Instance.CreateTNode(EntityType.Variable); // tworzenie node dla zmiennej po lewej stronie while node
                AST.AST.Instance.SetChildOfLink(variableNode, whileNode);

                Variable var = new Variable(token);
                if (VarTable.Instance.GetVarIndex(token) == -1)
                {
                    VarTable.Instance.InsertVar(token);
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
            List<Node> siblingsList = AST.AST.Instance.GetLinkedNodes(stmt, LinkType.Child); //pobieranie siblings o ile istnieją
            if (siblingsList.Count() != 0)
            {
                Node prevStmt = siblingsList[siblingsList.Count() - 1];
                //if ((parent.EntityType == EntityType.If && prevStmt.EntityType != EntityType.Stmtlist) || parent.EntityType != EntityType.If)
                AST.AST.Instance.SetFollows(prevStmt, node);
            }

        }

        public void SetModifiesForFamily(Node node, Variable var)
        {
            if (node.EntityType == EntityType.Procedure)
            {
                Procedure proc = ProcTable.Instance.Procedures.Where(i => i.AstRoot == node).FirstOrDefault();
                var.Index = VarTable.Instance.GetVarIndex(var.Name);
                Modifies.Modifies.Instance.SetModifies(proc, var);
            }
            else
            {
                Statement stmt = StmtTable.Instance.Statements.Where(i => i.AstRoot == node).FirstOrDefault();
                var.Index = VarTable.Instance.GetVarIndex(var.Name);
                Modifies.Modifies.Instance.SetModifies(stmt, var);
            }
            if (AST.AST.Instance.GetParent(node) != null) SetModifiesForFamily(AST.AST.Instance.GetParent(node), var);
        }

        public void SetUsesForFamily(Node node, Variable var)
        {
            if (node.EntityType == EntityType.Procedure)
            {
                Procedure proc = ProcTable.Instance.Procedures.Where(i => i.AstRoot == node).FirstOrDefault();
                var.Index = VarTable.Instance.GetVarIndex(var.Name);
                Uses.Uses.Instance.SetUses(proc, var);
            }
            else
            {
                Statement stmt = StmtTable.Instance.Statements.Where(i => i.AstRoot == node).FirstOrDefault();
                var.Index = VarTable.Instance.GetVarIndex(var.Name);
                Uses.Uses.Instance.SetUses(stmt, var);
            }
            if (AST.AST.Instance.GetParent(node) != null) SetUsesForFamily(AST.AST.Instance.GetParent(node), var);
        }
        
        public void ParseAssign(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (!IsVarName(token)) throw new Exception("ParseAssign: Wymagana nazwa zmiennej, " + token + ", linia: " + lineNumber);
            StmtTable.Instance.InsertStmt(EntityType.Assign, lineNumberQuery);
            startIndex = endIndex;

            Node assignNode = AST.AST.Instance.CreateTNode(EntityType.Assign); // tworzenie node dla assign
            Variable var = new Variable(token);
            VarTable.Instance.InsertVar(token);
            StmtTable.Instance.SetAstRoot(lineNumberQuery, assignNode);
            AST.AST.Instance.SetParent(assignNode, parent); //ustawianie parenta dla assign
                                                            //Node stmtListNode = AST.AST.Instance.GetNthChild(0, parent);
            SettingFollows(assignNode, stmtListNode, parent);
            AST.AST.Instance.SetChildOfLink(assignNode, stmtListNode); //łączenie stmlList z assign
            Node variableNode = AST.AST.Instance.CreateTNode(EntityType.Variable); // tworzenie node dla zmiennej po lewej stronie assign node
            AST.AST.Instance.SetChildOfLink(variableNode, assignNode);
            SetModifiesForFamily(assignNode, var); // ustawianie Modifies

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "=") throw new Exception("ParseAssign: Brak znaku =, linia: " + lineNumber);
            startIndex = endIndex;

            // parsowanie wszystkiego po =
            Node expressionRoot;
            ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, assignNode, false, out expressionRoot);
            AST.AST.Instance.SetChildOfLink(expressionRoot, assignNode);
            startIndex = endIndex;
        }
        public void ParseAssignOld(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (!IsVarName(token)) throw new Exception("ParseAssign: Wymagana nazwa zmiennej, " + token + ", linia: " + lineNumber);
            StmtTable.Instance.InsertStmt(EntityType.Assign, lineNumberQuery);
            startIndex = endIndex;

            Node assignNode = AST.AST.Instance.CreateTNode(EntityType.Assign); // tworzenie node dla assign
            Variable var = new Variable(token);
            VarTable.Instance.InsertVar(token);
            StmtTable.Instance.SetAstRoot(lineNumberQuery, assignNode);
            AST.AST.Instance.SetParent(assignNode, parent); //ustawianie parenta dla assign

            Node stmtListNode = AST.AST.Instance.GetNthChild(0, parent);
            SettingFollows(assignNode, stmtListNode, parent);
            AST.AST.Instance.SetChildOfLink(assignNode, stmtListNode); //łączenie stmlList z assign
            Node variableNode = AST.AST.Instance.CreateTNode(EntityType.Variable); // tworzenie node dla zmiennej po lewej stronie assign node
            AST.AST.Instance.SetChildOfLink(variableNode, assignNode);
            SetModifiesForFamily(assignNode, var); // ustawianie Modifies

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "=") throw new Exception("ParseAssign: Brak znaku =, linia: " + lineNumber);
            startIndex = endIndex;

            token = "";
            bool expectedOperation = false;
            Node expressionRoot = null; // zmienna przechowująca aktualny wierzchołek expression po prawej stronie assign
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
                            oldAssignRoot = AST.AST.Instance.GetTNodeDeepCopy(expressionRoot);
                            expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Plus);
                            AST.AST.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                            break;
                        case "-":
                            oldAssignRoot = AST.AST.Instance.GetTNodeDeepCopy(expressionRoot);
                            expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Minus);
                            AST.AST.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                            break;
                        case "*":
                            // tworzenie drzewa dla mnozenia
                            break;
                        default:
                            throw new Exception("ParseAssign: Nieobsługiwane działanie, " + token + ", linia: " + lineNumber);
                    }
                    expectedOperation = false;
                }
                else // factor z gramatyki
                {
                    if (IsVarName(token)) // spodziewana nazwa zmiennej
                    {
                        if (expressionRoot == null)
                        {
                            expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Variable);

                            Variable usesVar = new Variable(token); // ustawianie Uses
                            SetUsesForFamily(assignNode, usesVar);
                        }
                        else
                        {
                            Node rightSide = AST.AST.Instance.CreateTNode(EntityType.Variable);
                            AST.AST.Instance.SetChildOfLink(rightSide, expressionRoot);

                            Variable usesVar = new Variable(token); // ustawianie Uses
                            SetUsesForFamily(assignNode, usesVar);
                        }
                    }
                    else if (IsConstValue(token)) // spodziewana nazwa stalej
                    {
                        if (expressionRoot == null) expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Constant);
                        else
                        {
                            Node rightSide = AST.AST.Instance.CreateTNode(EntityType.Constant);
                            AST.AST.Instance.SetChildOfLink(rightSide, expressionRoot);
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
            AST.AST.Instance.SetChildOfLink(expressionRoot, assignNode);
        }

        /// <summary>
        /// Parsowanie expr z gramatyki
        /// </summary>
        /// <param name="lines">linie kodu SIMPLE</param>
        /// <param name="lineNumber">aktualnie przetwarzana linia</param>
        /// <param name="startIndex">indeks od którego ma zacząć czytać w danej linii</param>
        /// <param name="endIndex">indeks, na którym skończyło czytać w danej linii</param>
        /// <param name="procedureName">nazwa przetwarzanej procedury</param>
        public bool ParseExpr(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node assignNode, Node parent, bool inBracket, out Node expressionRoot, string prevToken = "")
        {
            bool endAssign = false; // czy wykryto zakonczenie assign
            string token = "";
            endIndex = startIndex;
            bool expectedOperation = false; // czy spodziewane jest dzialanie: +, -, *, ()
            bool possibleBracketClose = false; // czy mozliwy jest )
            int tokenCount = 0; // liczba wczytanych tokenow
            bool bracketsPaired = false;
            expressionRoot = null; // zmienna przechowująca aktualny wierzchołek expression po prawej stronie assign
            while (lineNumber < lines.Count)
            {
                token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true); // sprawdzenie kolejnego tokena
                tokenCount++;
                if (expectedOperation)
                {
                    Node oldAssignRoot;
                    token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false); // pobranie tokena
                    startIndex = endIndex;
                    switch (token)
                    {
                        case "+":
                            oldAssignRoot = AST.AST.Instance.GetTNodeDeepCopy(expressionRoot);
                            expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Plus);
                            AST.AST.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                            expectedOperation = false;
                            break;
                        case "-":
                            oldAssignRoot = AST.AST.Instance.GetTNodeDeepCopy(expressionRoot);
                            expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Minus);
                            AST.AST.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                            expectedOperation = false;
                            break;
                        case "*":
                            oldAssignRoot = AST.AST.Instance.GetTNodeDeepCopy(expressionRoot);
                            expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Multiply);
                            AST.AST.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
                            expectedOperation = false;
                            break;
                        case "/":
                            oldAssignRoot = AST.AST.Instance.GetTNodeDeepCopy(expressionRoot);
                            expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Divide);
                            AST.AST.Instance.SetChildOfLink(oldAssignRoot, expressionRoot);
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
                                    AST.AST.Instance.SetChildOfLink(parent, expressionRoot);
                                    //return expressionRoot;
                                }
                            }
                            else
                            {
                                AST.AST.Instance.SetChildOfLink(parent, expressionRoot);
                                //return expressionRoot;
                            }
                            break;
                        default:
                            throw new Exception("ParseExpr: Nieobsługiwane działanie, " + token + ", linia: " + lineNumber);
                    }
                }
                else // factor z gramatyki
                {
                    //if (prevToken != "" && expressionRoot == null) token = prevToken;
                    //else
                    //{
                    //    token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false); // pobranie tokena
                    //   startIndex = endIndex;
                    //}
                    if (IsVarName(token)) // spodziewana nazwa zmiennej
                    {
                        //  Console.WriteLine(startIndex + endIndex);
                        if (expressionRoot == null) expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Variable);
                        else
                        {
                            string nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                            if (nextToken == "*" || nextToken == "/")
                            {
                                if (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply)
                                {
                                    Node rightSide = AST.AST.Instance.CreateTNode(EntityType.Variable);
                                    AST.AST.Instance.SetChildOfLink(rightSide, expressionRoot);
                                }
                                else
                                {
                                    Node tinyTreeRoot = null;
                                    endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                    AST.AST.Instance.SetChildOfLink(tinyTreeRoot, expressionRoot);
                                }
                            }
                            else
                            {
                                Node rightSide = AST.AST.Instance.CreateTNode(EntityType.Variable);
                                AST.AST.Instance.SetChildOfLink(rightSide, expressionRoot);
                                //if (!inBracket && (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply) && nextToken != ";" && parent.EntityType != EntityType.Assign)
                                //    return expressionRoot;
                                //else if (inBracket && (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply) && nextToken == ";" && parent.EntityType != EntityType.Assign)
                                //    return expressionRoot;
                            }
                        }
                        Variable usesVar = new Variable(token); // ustawianie Uses
                        if (VarTable.Instance.GetVarIndex(token) == -1)
                        {
                            VarTable.Instance.InsertVar(token);
                        }
                        SetUsesForFamily(assignNode, usesVar);
                        startIndex = endIndex;
                        expectedOperation = true;
                    }
                    else if (IsConstValue(token)) // spodziewana nazwa stalej
                    {
                        // Console.WriteLine(startIndex + endIndex);
                        if (expressionRoot == null) expressionRoot = AST.AST.Instance.CreateTNode(EntityType.Constant);
                        else
                        {
                            string nextToken = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
                            if (nextToken == "*" || nextToken == "/")
                            {
                                if (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply)
                                {
                                    Node rightSide = AST.AST.Instance.CreateTNode(EntityType.Constant);
                                    AST.AST.Instance.SetChildOfLink(rightSide, expressionRoot);
                                }
                                else
                                {
                                    Node tinyTreeRoot = null;
                                    endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, expressionRoot, false, out tinyTreeRoot, token);
                                    AST.AST.Instance.SetChildOfLink(tinyTreeRoot, expressionRoot);
                                }
                            }
                            else
                            {
                                Node rightSide = AST.AST.Instance.CreateTNode(EntityType.Constant);
                                AST.AST.Instance.SetChildOfLink(rightSide, expressionRoot);
                                //if (!inBracket && (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply) && nextToken != ";")
                                //     return expressionRoot;
                                //else if (inBracket && (expressionRoot.EntityType == EntityType.Divide || expressionRoot.EntityType == EntityType.Multiply) && nextToken == ";" && parent.EntityType != EntityType.Assign)
                                //    return expressionRoot;
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
                            // parsowanie po (
                            Node tinyTreeRoot;
                            endAssign = ParseExpr(lines, startIndex, ref lineNumber, out endIndex, procedureName, assignNode, expressionRoot, true, out tinyTreeRoot);
                            if (expressionRoot == null)
                                expressionRoot = AST.AST.Instance.GetTNodeDeepCopy(tinyTreeRoot);
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
                    //return expressionRoot;
                }
            }
            if (lineNumber == lines.Count && token != ";") throw new Exception("ParseExpr: Spodziewano się znaku ; linia: " + lineNumber);
            //return expressionRoot;
            return endAssign;
        }

        /// <summary>
        /// Parsowanie call
        /// </summary>
        /// <param name="lines">linie kodu SIMPLE</param>
        /// <param name="lineNumber">aktualnie przetwarzana linia</param>
        /// <param name="startIndex">indeks od którego ma zacząć czytać w danej linii</param>
        /// <param name="endIndex">indeks, na którym skończyło czytać w danej linii</param>
        /// <param name="procedureName">nazwa przetwarzanej procedury</param>
        public void ParseCall(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "call") throw new Exception("ParseCall: Brak słowa kluczowego call, linia: " + lineNumber);

            startIndex = endIndex;

            StmtTable.Instance.InsertStmt(EntityType.Call, lineNumberQuery);
            Node callNode = AST.AST.Instance.CreateTNode(EntityType.Call);
            StmtTable.Instance.SetAstRoot(lineNumberQuery, callNode);

            AST.AST.Instance.SetParent(callNode, parent); //ustawianie parenta dla call
                                                          //Node stmtListNode = AST.AST.Instance.GetLinkedNodes(parent, LinkType.Child)
                                                          //    .Where(i => i.EntityType == EntityType.Stmtlist).FirstOrDefault();
            SettingFollows(callNode, stmtListNode, parent); //setting follows
            AST.AST.Instance.SetChildOfLink(callNode, stmtListNode);

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (IsVarName(token)) callNode.NodeAttribute.Name = token;
            else throw new Exception("ParseCall: Wymagana nazwa procedury, " + token + ", linia: " + lineNumber);
            startIndex = endIndex;

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            startIndex = endIndex;
            if (token != ";") throw new Exception("ParseCall: Brak znaku ; linia: " + lineNumber);
        }


        /// <summary>
        /// Parsowanie if then else
        /// </summary>
        /// <param name="lines">linie kodu SIMPLE</param>
        /// <param name="lineNumber">aktualnie przetwarzana linia</param>
        /// <param name="startIndex">indeks od którego ma zacząć czytać w danej linii</param>
        /// <param name="endIndex">indeks, na którym skończyło czytać w danej linii</param>
        /// <param name="procedureName">nazwa przetwarzanej procedury</param>
        public void ParseIf(List<string> lines, int startIndex, ref int lineNumber, out int endIndex, string procedureName, Node parent, Node stmtListNode)
        {
            string token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "if") throw new Exception("ParseIf: Brak słowa kluczowego if, linia: " + lineNumber);

            startIndex = endIndex;

            StmtTable.Instance.InsertStmt(EntityType.If, lineNumberQuery);
            Node ifNode = AST.AST.Instance.CreateTNode(EntityType.If);
            StmtTable.Instance.SetAstRoot(lineNumberQuery, ifNode);

            AST.AST.Instance.SetParent(ifNode, parent); //ustawianie parenta dla if
            //Node stmtListNode = AST.AST.Instance.GetNthChild(0, parent);
            SettingFollows(ifNode, stmtListNode, parent); //setting follows
            AST.AST.Instance.SetChildOfLink(ifNode, stmtListNode);

            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (IsVarName(token))
            {
                //if (VarTable.Instance.GetVarIndex(token) == -1) throw new Exception("ParseAssign: Zmienna nie została przypisana, " + token + ", linia: " + lineNumber);
                //else
                //{
                Variable var = new Variable(token);
                if (VarTable.Instance.GetVarIndex(token) == -1)
                {
                    VarTable.Instance.InsertVar(token);
                }
                SetUsesForFamily(ifNode, var);
                //}
            }
            else throw new Exception("ParseIf: Wymagana nazwa zmiennej, " + token + ", linia: " + lineNumber);
            startIndex = endIndex;

            // pobranie THEN
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "then") throw new Exception("ParseIf: Brak słowa kluczowego then, linia: " + lineNumber);
            startIndex = endIndex;

            // sprawdzenie czy jest {
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token != "{") throw new Exception("ParseIf: Brak znaku {, linia: " + lineNumber);

            // parsowanie STMTLST po THEN
            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
            startIndex = endIndex;

            // musi wystąpić ELSE
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, false);
            if (token != "else") throw new Exception("ParseIf: Brak słowa kluczowego else, linia: " + lineNumber);
            startIndex = endIndex;

            // sprawdzenie czy jest {
            token = GetToken(lines, ref lineNumber, startIndex, out endIndex, true);
            if (token != "{") throw new Exception("ParseIf: Brak znaku {, linia: " + lineNumber);

            // parsowanie STMTLST po ELSE
            Parse(lines, startIndex, ref lineNumber, out endIndex, procedureName, ifNode);
            startIndex = endIndex;
        }

        /// <summary>
        /// Parsowanie
        /// </summary>
        /// <param name="lines">linie kodu SIMPLE</param>
        /// <param name="lineNumber">aktualnie przetwarzana linia</param>
        /// <param name="startIndex">indeks od którego ma zacząć czytać w danej linii</param>
        /// <param name="endIndex">indeks, na którym skończyło czytać w danej linii</param>
        /// <param name="procedureName">nazwa przetwarzanej procedury</param>
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


        /// <summary>
        /// Sprawdzenie czy nazwa zmiennej lub procedury jest zgodna z gramatyką
        /// </summary>
        /// <param name="name">testowana nazwa</param>
        /// <returns>czy nazwa jest prawidłowa</returns>
        public bool IsVarName(string name)
        {
            if (name.Length == 0) return false;
            if (!Char.IsLetter(name[0])) return false;
            else if (reservedWords.IndexOf(name) > 0) return false;

            return true;
        }

        /// <summary>
        /// Sprawdzenie czy zmienna jest stałą
        /// </summary>
        /// <param name="name">testowana nazwa</param>
        /// <returns>czy nazwa jest prawidłowa</returns>
        public bool IsConstValue(string name)
        {
            long test;
            if (name.Length == 0) return false;
            return Int64.TryParse(name, out test);
        }


        /// <summary>
        /// Odczytanie pliku z kodem SIMPLE
        /// </summary>
        /// <param name="filename">ścieżka do pliku</param>
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
            while (lineNumber < lines.Count) // parsowanie programu - min 1 procedura
            {
                token = GetToken(lines, ref lineNumber, index, out endIndex, true);
                if (token != "") countToken++;
                if (token == "")
                {
                    if (countToken > 0) break; // nastapil koniec pliku
                    else throw new Exception("StartParse: Pusty kod");
                }
                Node newRoot = AST.AST.Instance.CreateTNode(EntityType.Program);
                AST.AST.Instance.SetRoot(newRoot);

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
            int sizeOfProcTable = ProcTable.Instance.GetSize();
            do
            {
                wasChange = false;
                for (int i = 0; i < sizeOfProcTable; i++)
                {
                    Procedure p1 = ProcTable.Instance.GetProc(i);
                    for (int j = 0; j < sizeOfProcTable; j++)
                    {
                        if (i != j)
                        {
                            Procedure p2 = ProcTable.Instance.GetProc(j);
                            if (Calls.Calls.Instance.IsCalls(p1.Name, p2.Name))
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

            foreach (Statement s in StmtTable.Instance.Statements)
                if (s.Type == EntityType.Call)
                {
                    string pname = s.AstRoot.NodeAttribute.Name;
                    Procedure p = ProcTable.Instance.GetProc(pname);
                    if (p != null)
                    {
                        s.ModifiesList = p.ModifiesList;
                        s.UsesList = p.UsesList;
                    }
                }
        }

        public void UpdateModifiesAndUsesTablesInWhilesAndIfs()
        {
            List<Statement> ifOrWhileStmts = StmtTable.Instance.Statements
                .Where(i => i.AstRoot.EntityType == EntityType.While || i.AstRoot.EntityType == EntityType.If).ToList();

            foreach(var stmt in ifOrWhileStmts)
            {
                var node = stmt.AstRoot;
                List<Node> stmtLstNodes = AST.AST.Instance
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
                            //wasChange = true;
                        }
                    foreach (KeyValuePair<int, bool> variable in proc.UsesList)
                        if (!stmt.UsesList.ContainsKey(variable.Key))
                        {
                            stmt.UsesList[variable.Key] = true;
                            //wasChange = true;
                        }
                }
            
            }
        }

        public void CleanData()
        {
            AST.AST.Instance.Root = null;
            VarTable.Instance.Variables.Clear();
            StmtTable.Instance.Statements.Clear();
            ProcTable.Instance.Procedures.Clear();
        }

        private void addLineNumberQuery(string line, int lineNumber)
        {
            if (!line.Contains("procedure") && !line.Contains("else"))
            {
                if (lineNumber - lineNumberOld >= 1)
                {
                    lineNumberQuery++;
                    lineNumberOld = lineNumber;
                }
            }
        }
}