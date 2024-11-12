using NEA.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Configuration;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace NEA
{
    class Machine
    {
        private string sourceCode;

        // Fields for Tokenization
        private Token[] tokens;
        private string[] keyword = { "CREATE", "SET", "CHANGE", "ADD", "TAKE", "AWAY", "MULTIPLY", "DIVIDE", "GET", "THE", "REMAINDER", "OF", 
                                     "MODULO", "IF", "ELSE", "COUNT", "WITH", "FROM", "BY", "WHILE", "LOOP", "REPEAT", "FOR", "EACH", "IN", "FUNCTION",
                                     "PROCEDURE", "INPUTS", "AS", "TO", "STR_LITERAL", "CHAR_LITERAL", "INT_LITERAL", "DEC_LITERAL", "BOOL_LITERAL",
                                     "LEFT_BRACKET", "RIGHT_BRACKET", "ADD", "SUB", "MUL", "DIV", "MOD", "EXP", "THEN", "NEWLINE", "TABSPACE", "EQUAL",
                                     "GREATER", "LESS", "THAN", "INPUT", "PRINT", "AND", "OR", "NOT", "BEGIN", "END", "RETURN", "EOF" };
        private int current, start, line, counter;

        // Fields for Translation into Intermediate Code
        private string[] intermediate;
        private List<string> intermediateList;
        private List<string[]> intermediateSubroutines;
        private Dictionary<string, int> subroutineDict;
        private List<Variable> variables;
        private Dictionary<string, int> variablesDict = new Dictionary<string, int>();
        private int counterVar;

        // Fields for Execution
        private int PC;
        private Stack<object> stack;
        private Stack<StackFrame> callStack;
        private bool validProgram;

        public Machine(string sourceCode)
        {
            this.sourceCode = sourceCode;
            callStack = new Stack<StackFrame>();
            counter = 0;
            PC = 0;
            validProgram = true;
        }

        public string Interpret()
        {
            // Tokenization
            tokens = Tokenize();

            variables = new List<Variable>();

            OrganiseVariables();

            // Testing
            string String = "";

            //foreach (Token token in tokens)
            //{
            //    String += token.GetTokenType().ToString() + "\r\n";
            //}

            //// Translation

            intermediate = TokensToIntermediate();

            //String += "\r\nIntermediate\r\n";

            // Testing
            foreach (string line in intermediate)
            {
                String += line + "\r\n";
            }

            return String;

            // Execution

            // Execute();
        }

        #region Tokenization
        #region Utility
        private int GetNoVariables()
        {
            List<string> variables = new List<string>();
            foreach (Token token in tokens)
            {
                if (token.GetTokenType() == TokenType.VARIABLE)
                {
                    if (!variables.Contains(token.GetLiteral()))
                    {
                        variables.Add(token.GetLiteral());
                    }
                }
            }
            return variables.Count;
        }

        private char Peek()
        {
            if (current >= sourceCode.Length)
            {
                return '\0';
            }
            return sourceCode[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= sourceCode.Length)
            {
                return '\0';
            }
            return sourceCode[current + 1];
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsDigit(c) || IsAlpha(c);
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private bool IsDigit(char c)
        {
            return char.IsDigit(c);
        }

        // Not completed
        private TokenType GetTokenType(string token)
        {
            switch (token.ToUpper())
            {
                case "(":
                    return TokenType.LEFT_BRACKET;
                case ")":
                    return TokenType.RIGHT_BRACKET;
                case "+":
                    return TokenType.ADD;
                case "-":
                    return TokenType.SUB;
                case "*":
                    return TokenType.MUL;
                case "/":
                    return TokenType.DIV;
                case "%":
                    return TokenType.MOD;
                case "^":
                    return TokenType.EXP;
                case "CREATE":
                    return TokenType.DECLARATION;
                case "SET":
                    return TokenType.ASSIGNMENT;
                case "CHANGE":
                    return TokenType.REASSIGNMENT;
                case "ADD":
                    return TokenType.ADDITION;
                case "TAKE":
                    return TokenType.TAKE;
                case "AWAY":
                    return TokenType.AWAY;
                case "MULTIPLY":
                    return TokenType.MULTIPLICATION;
                case "DIVIDE":
                    return TokenType.DIVISION;
                case "GET":
                    return TokenType.GET;
                case "THE":
                    return TokenType.THE;
                case "REMAINDER":
                    return TokenType.REMAINDER;
                case "OF":
                    return TokenType.OF;
                case "IF":
                    return TokenType.IF;
                case "ELSE":
                    return TokenType.ELSE;
                case "COUNT":
                    return TokenType.COUNT;
                case "WITH":
                    return TokenType.WITH;
                case "FROM":
                    return TokenType.FROM;
                case "BY":
                    return TokenType.BY;
                case "WHILE":
                    return TokenType.WHILE;
                case "LOOP":
                    return TokenType.LOOP;
                case "REPEAT":
                    return TokenType.REPEAT;
                case "FOR":
                    return TokenType.FOR;
                case "EACH":
                    return TokenType.EACH;
                case "IN":
                    return TokenType.IN;
                case "FUNCTION":
                    return TokenType.FUNCTION;
                case "PROCEDURE":
                    return TokenType.PROCEDURE;
                case "INPUTS":
                    return TokenType.INPUTS;
                case "AS":
                    return TokenType.AS;
                case "TO":
                    return TokenType.TO;
                case "THEN":
                    return TokenType.THEN;
                case "\n":
                    return TokenType.NEWLINE;
                case "\t":
                    return TokenType.TABSPACE;
                case "EQUAL":
                    return TokenType.EQUAL;
                case "GREATER":
                    return TokenType.GREATER;
                case "LESS":
                    return TokenType.LESS;
                case "THAN":
                    return TokenType.THAN;
                case "INPUT":
                    return TokenType.INPUT;
                case "OR":
                    return TokenType.OR;
                case "AND":
                    return TokenType.AND;
                case "NOT":
                    return TokenType.NOT;
                case "BEGIN":
                    return TokenType.BEGIN;
                case "END":
                    return TokenType.END;
                case "PRINT":
                    return TokenType.PRINT;
                case "RETURN":
                    return TokenType.RETURN;
                default:
                    validProgram = false;
                    throw new Exception($"Could NOT find token type\nToken: {token}");
            }
        }
        #endregion

        private void OrganiseVariables()
        {
            int counter = 0;
            foreach (Token token in tokens)
            {
                if (token.GetTokenType() == TokenType.VARIABLE)
                {
                    if (!variablesDict.ContainsKey(token.GetLiteral()))
                    {
                        variablesDict.Add(token.GetLiteral(), counter++);
                    }
                }
            }
        }

        private string GetWord()
        {
            while (IsAlphaNumeric(Peek()))
            {
                current++;
            }

            return sourceCode.Substring(start, current - start);
        }

        private Token GetNumber()
        {
            TokenType type = TokenType.INT_LITERAL;
            while (IsDigit(Peek()))
            {
                current++;
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                type = TokenType.DEC_LITERAL;
                current++;
                while (IsDigit(Peek()))
                {
                    current++;
                }
            }

            string value = sourceCode.Substring(start, current - start);

            return new Token(type, value, line);
        }

        private Token GetString()
        {
            while (Peek() != '"' && current < sourceCode.Length)
            {
                if (Peek() == '\n')
                {
                    line++;
                }
                current++;
            }

            if (current >= sourceCode.Length)
            {
                validProgram = false;
            }

            current++;

            string text = sourceCode.Substring(start + 1, current - start - 2);

            return new Token(TokenType.STR_LITERAL, text, line);
        }

        private void SkipToEndOfLine()
        {
            while (Peek() != '\n' && current < sourceCode.Length)
            {
                current++;
            }
        }

        private string[] FindSubroutineNames()
        {
            List<string> subroutineNames = new List<string>();
            char[] seperators = { ' ', '\n' };
            string[] words = sourceCode.Split(seperators);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] == "FUNCTION")
                {
                    subroutineNames.Add(words[i + 1]);
                }
            }
            return subroutineNames.ToArray();
        }

        private Token[] Tokenize()
        {
            List<Token> tokensList = new List<Token>();
            char[] singleCharKeyword = { ')', '(', '+', '-', '*', '/', '%', '^' };
            string[] dataTypes = { "STRING", "CHARACTER", "INTEGER", "DECIMAL", "BOOLEAN" }; // Add lists and arrays

            string[] subroutineNames = FindSubroutineNames();

            while (current < sourceCode.Length)
            {
                start = current;
                char c = sourceCode[current++];
                if (singleCharKeyword.Contains(c))
                {
                    tokensList.Add(new Token(GetTokenType(c.ToString()), c.ToString(), line));
                }
                else if (c == '\n')
                {
                    line++;
                }
                else if (c == '\t')
                {
                    continue;
                }
                else if (c == '"')
                {
                    tokensList.Add(GetString());
                }
                else if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                else if (IsAlpha(c))
                {
                    string word = GetWord();
                    if (keyword.Contains(word.ToUpper()))
                    {
                        tokensList.Add(new Token(GetTokenType(word), word, line));
                    }
                    else if (dataTypes.Contains(word.ToUpper()))
                    {
                        tokensList.Add(new Token(TokenType.DATA_TYPE, word, line));
                    }
                    else if (subroutineNames.Contains(word))
                    {
                        tokensList.Add(new Token(TokenType.SUBROUTINE_NAME, word, line));
                    }
                    else
                    {
                        tokensList.Add(new Token(TokenType.VARIABLE, word, line));
                    }
                }
                else if (IsDigit(c))
                {
                    tokensList.Add(GetNumber());
                }
                else if (c == '#')
                {
                    SkipToEndOfLine();
                }
            }

            tokensList.Add(new Token(TokenType.EOF, null, line));

            return tokensList.ToArray();
        }
        #endregion

        #region Translation into Intermdiate

        // Shunting Yard Algorithm
        // Converts list of tokens
        // To intermediate code in postfix
        private string[] ConvertToPostfix(List<Token> tokens)
        {
            List<string> output = new List<string>();
            Stack<Token> stack = new Stack<Token>();

            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] operators = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                      TokenType.DIV, TokenType.MOD, TokenType.EXP };

            foreach (var e in tokens)
            {
                if (literals.Contains(e.GetTokenType()))
                {
                    output.Add("LOAD_CONST " + e.GetLiteral());
                }
                else if (e.GetTokenType() == TokenType.VARIABLE)
                {
                    output.Add("LOAD_VAR " + variablesDict[e.GetLiteral()]);
                }
                else if (e.GetTokenType() == TokenType.LEFT_BRACKET)
                {
                    stack.Push(e);
                }
                else if (e.GetTokenType() == TokenType.RIGHT_BRACKET)
                {
                    while (stack.Count > 0 && stack.Peek().GetTokenType() != TokenType.LEFT_BRACKET)
                    {
                        var topToken = stack.Pop();
                        if (operators.Contains(topToken.GetTokenType()))
                        {
                            output.Add(topToken.GetTokenType().ToString());
                        }
                    }
                    stack.Pop();
                }
                else
                {
                    while (stack.Count > 0 && stack.Peek().GetTokenType() != TokenType.LEFT_BRACKET &&
                           Precedence(stack.Peek().GetTokenType()) >= Precedence(e.GetTokenType()))
                    {
                        var topToken = stack.Pop();
                        if (operators.Contains(topToken.GetTokenType()))
                        {
                            output.Add(topToken.GetTokenType().ToString());
                        }
                    }
                    stack.Push(e);
                }
            }

            while (stack.Count > 0)
            {
                var topToken = stack.Pop();
                if (operators.Contains(topToken.GetTokenType()))
                {
                    output.Add(topToken.GetTokenType().ToString());
                }
                else if (literals.Contains(topToken.GetTokenType()))
                {
                    output.Add("LOAD_CONST " + topToken.GetLiteral());
                }
                else if (topToken.GetTokenType() == TokenType.VARIABLE)
                {
                    output.Add("LOAD_VAR " + variablesDict[topToken.GetLiteral()]);
                }
            }

            return output.ToArray();
        }

        private int Precedence(TokenType type)
        {
            switch (type)
            {
                case TokenType.ADD:
                    return 1;
                case TokenType.SUB: 
                    return 1;
                case TokenType.MUL:
                    return 2;
                case TokenType.DIV:
                    return 2;
                case TokenType.MOD:
                    return 2;
                case TokenType.EXP:
                    return 3;
                // Add comparison operators
            }
            return -1;
        }

        private string[] MapAssignment(string variable, List<Token> expression, string type)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            instrLine = "LOAD_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            instructions.AddRange(ConvertToPostfix(expression));

            instrLine = "ADJUST_TYPE " + type;
            instructions.Add(instrLine);

            instrLine = "STORE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "DECLARE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] MapReassignment(string variable, List<Token> expression, string type)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            instructions.AddRange(ConvertToPostfix(expression));

            instrLine = "ADJUST_TYPE " + type;
            instructions.Add(instrLine);

            instrLine = "STORE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] MapDeclaration(string variable, string type)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            instrLine = "LOAD_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "ADJUST_TYPE " + type;
            instructions.Add(instrLine);

            instrLine = "DECLARE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] TokensToIntermediate()
        {
            if (intermediate == null)
            {
                intermediateList = new List<string>();
            }

            int i = 0;
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };

            string type, variableName;
            bool noType;
            List<Token> expression;
            int j;

            while (i < tokens.Length)
            {
                Token token = tokens[i];
                MessageBox.Show($"Entered loop\ni = {i}\ntokens.Length = {tokens.Length}");
                switch (token.GetTokenType())
                {
                    case TokenType.ASSIGNMENT:
                        type = "STRING";
                        noType = true;
                        if (tokens[i + 1].GetTokenType() == TokenType.VARIABLE)
                        {
                            variableName = tokens[i + 1].GetLiteral();
                            if (tokens[i + 2].GetTokenType() == TokenType.TO)
                            {
                                expression = new List<Token>();
                                j = 1;
                                while (tokens[i + j + 2].GetTokenType() != TokenType.EOF && tokens[i + j + 2].GetLine() == token.GetLine() && tokens[i + j + 2].GetTokenType() != TokenType.AS)
                                {
                                    expression.Add(tokens[i + j + 2]);
                                    j++;
                                }
                                j = expression.Count;
                                if (tokens[i + j + 2].GetTokenType() != TokenType.EOF && tokens[i + j + 3].GetTokenType() == TokenType.AS && tokens[i + j + 4].GetTokenType() == TokenType.DATA_TYPE)
                                {
                                    type = tokens[i + j + 4].GetLiteral();
                                    noType = false;
                                }
                                else if (tokens[i + j + 3].GetTokenType() != TokenType.EOF && tokens[i + j + 3].GetLine() == token.GetLine())
                                {
                                    throw new Exception("ERROR: No data type mentioned");
                                }
                            }
                            else
                            {
                                throw new Exception("ERROR: No value was mentioned for assignment");
                            }
                        }
                        else
                        {
                            throw new Exception("ERROR: When assigning no variable was found");
                        }
                        intermediateList.AddRange(MapAssignment(variableName, expression, type));
                        i += j + 3;
                        if (!noType)
                        {
                            i += 2;
                        }
                        break;
                    case TokenType.REASSIGNMENT:
                        type = "STRING";
                        noType = true;
                        if (tokens[i + 1].GetTokenType() == TokenType.VARIABLE)
                        {
                            variableName = tokens[i + 1].GetLiteral();
                            if (tokens[i + 2].GetTokenType() == TokenType.TO)
                            {
                                expression = new List<Token>();
                                j = 1;
                                while (tokens[i + j + 2].GetTokenType() != TokenType.EOF && tokens[i + j + 2].GetLine() == token.GetLine() && tokens[i + j + 2].GetTokenType() != TokenType.AS)
                                {
                                    expression.Add(tokens[i + j + 2]);
                                    j++;
                                }
                                if (tokens[i + j + 2].GetTokenType() != TokenType.EOF && tokens[i + j + 3].GetTokenType() == TokenType.AS && tokens[i + j + 4].GetTokenType() == TokenType.DATA_TYPE)
                                {
                                    type = tokens[i + j + 4].GetLiteral();
                                }
                                else if (tokens[i + j + 2].GetTokenType() != TokenType.EOF && tokens[i + j + 3].GetLine() == token.GetLine())
                                {
                                    throw new Exception("ERROR: No data type mentioned");
                                }
                            }
                            else
                            {
                                throw new Exception("ERROR: No value was mentioned for assignment");
                            }
                        }
                        else
                        {
                            throw new Exception("ERROR: When assigning no variable was found");
                        }
                        intermediateList.AddRange(MapReassignment(variableName, expression, type));
                        i += j + 2;
                        if (!noType)
                        {
                            i += 2;
                        }
                        break;
                    case TokenType.DECLARATION:
                        type = "STRING";
                        noType = true;
                        if (tokens[i + 1].GetTokenType() == TokenType.VARIABLE)
                        {
                            variableName = tokens[i + 1].GetLiteral();
                            if (tokens[i + 2].GetTokenType() == TokenType.AS &&
                                tokens[i + 3].GetTokenType() == TokenType.DATA_TYPE)
                            {
                                noType = false;
                                type = tokens[i + 3].GetLiteral();
                            }
                            else if (tokens[i + 2].GetLine() == token.GetLine() && tokens[i + 2].GetTokenType() != TokenType.EOF)
                            {
                                throw new Exception("ERROR: No data type mentioned.");
                            }
                        }
                        else
                        {
                            throw new Exception("ERROR: When creating no variable was found.");
                        }
                        intermediateList.AddRange(MapDeclaration(variableName, type));
                        i += 2;
                        if (!noType)
                        {
                            i += 2;
                        }
                        break;
                    case TokenType.EOF:
                        intermediateList.Add("HALT");
                        i++;
                        break;
                }
            }

            return intermediateList.ToArray();
        }
        #endregion
    }
}
