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
using System.ComponentModel.Design;
using System.Reflection.Emit;
using System.Xml.Schema;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using System.Drawing;
using System.Diagnostics.Tracing;

namespace NEA
{
    class Machine
    {
        private string sourceCode;

        // Fields for Tokenization
        private Token[] tokens;
        private string[] keyword = { "CREATE", "SET", "CHANGE", "ADD", "TAKE", "AWAY", "MULTIPLY", "DIVIDE", "GET", "THE", "REMAINDER", "OF", 
                                     "MODULO", "IF", "ELSE", "COUNT", "WITH", "FROM", "BY", "WHILE", "LOOP", "REPEAT", "FOR", "EACH", "IN", "FUNCTION",
                                     "PROCEDURE", "INPUTS", "AS", "TO", "STR_LITERAL", "CHAR_LITERAL", "INT_LITERAL", "DEC_LITERAL", "BOOL_LITERAL", "TRUE", "FALSE",
                                     "LEFT_BRACKET", "RIGHT_BRACKET", "ADD", "SUB", "MUL", "DIV", "MOD", "EXP", "THEN", "NEWLINE", "TABSPACE", 
                                     "INPUT", "PROMPT", "PRINT", "AND", "OR", "NOT", "BEGIN", "END", "RETURN", "EOF", "EON" /*/ End of nest /*/ };
        private int current, start, line, counter;

        // Fields for Translation into Intermediate Code
        private string[] intermediate;
        private List<string[]> intermediateSubroutines;
        private Dictionary<string, int> subroutineDict;
        private Variable[] variables;
        private Dictionary<string, int> variablesDict = new Dictionary<string, int>();
        private int counterVar;

        // Fields for Execution
        private int PC;
        private Stack<object> stack;
        private Stack<StackFrame> callStack;
        private bool validProgram;
        private bool isRunning = false;

        public Machine(string sourceCode)
        {
            this.sourceCode = sourceCode;
            callStack = new Stack<StackFrame>();
            counter = 0;
            PC = 0;
            validProgram = true;
            stack = new Stack<object>();
        }

        public string[] GetIntermediateCode()
        {
            return intermediate;
        }

        public void ConsoleWrite(IDE_MainWindow form, string text)
        {
            form.txtConsole.Text += text + "\r\n";
        }

        public void ClearConsole(IDE_MainWindow form)
        {
            form.txtConsole.Text = "";
        }

        public void Interpret()
        {
            // Tokenization
            tokens = Tokenize();

            variables = new Variable[GetNoVariables()];

            OrganiseVariables();

            // Translation

            intermediate = TokensToIntermediate(tokens);
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
                case "=":
                    return TokenType.EQUAL;
                case ">":
                    return TokenType.GREATER;
                case "<":
                    return TokenType.LESS;
                case ">=":
                    return TokenType.GREATER_EQUAL;
                case "<=":
                    return TokenType.LESS_EQUAL;
                case "<>":
                    return TokenType.NOT_EQUAL;
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
                case "TRUE":
                    return TokenType.BOOL_LITERAL;
                case "FALSE":
                    return TokenType.BOOL_LITERAL;
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
                case "PROMPT":
                    return TokenType.PROMPT;
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
            for (int i = 0; i < variables.Length; i++)
            {
                variables[i] = new Variable(KeyByValue(i), null);
            }
        }

        private string GetComparisonOperator()
        {
            while (IsComparisonOperatorChar(Peek()))
            {
                current++;
            }

            return sourceCode.Substring(start, current - start);
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

        private bool IsComparisonOperatorChar(char c)
        {
            return c == '=' || /*/ Temp /*/ c == '!' || c == '>' || c == '<';
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

        public Token[] Tokenize()
        {
            List<Token> tokensList = new List<Token>();
            char[] singleCharKeyword = { ')', '(', '+', '-', '*', '/', '%', '^' };
            string[] multiCharKeywords = {"=", /*/ Temp /*/ "<>", ">", "<", ">=", "<=" };
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
                        TokenType type = GetTokenType(word);
                        if (type == TokenType.END)
                        {
                            tokensList.Add(new Token(TokenType.EON, word, line));
                        }
                        tokensList.Add(new Token(type, word, line));
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
                else if (IsComparisonOperatorChar(c))
                {
                    string op = GetComparisonOperator();
                    tokensList.Add(new Token(GetTokenType(op), op, line));
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
        // https://en.wikipedia.org/wiki/Shunting_yard_algorithm

        // Potential Optimization - Change List<Token> to Token[]
        private string[] ConvertToPostfix(List<Token> tokens)
        {
            List<string> output = new List<string>();
            Stack<Token> stack = new Stack<Token>();

            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP, };
            TokenType[] comparisonOperators = { TokenType.EQUAL, TokenType.NOT_EQUAL, TokenType.GREATER,
                                                TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL };

            string String = "";

            foreach (Token t in tokens)
            {
                String += $"{t.GetTokenType()}\n";
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                Token e = tokens[i];
                if (literals.Contains(e.GetTokenType()))
                {
                    output.Add("LOAD_CONST " + e.GetLiteral());
                }
                else if (e.GetTokenType() == TokenType.VARIABLE)
                {
                    output.Add("LOAD_VAR " + variablesDict[e.GetLiteral()]);
                }
                else if (e.GetTokenType() == TokenType.INPUT)
                {
                    List<Token> inputPrompt = new List<Token>();
                    i++;

                    inputPrompt.Add(tokens[i]);

                    string[] inputStatement = MapInputStatement(inputPrompt);

                    foreach (string statement in inputStatement)
                    {
                        output.Add(statement);
                    }
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
                        if (comparisonOperators.Contains(topToken.GetTokenType()) || mathematicalOperations.Contains(topToken.GetTokenType()))
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
                        if (comparisonOperators.Contains(topToken.GetTokenType()) || mathematicalOperations.Contains(topToken.GetTokenType()))
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
                if (comparisonOperators.Contains(topToken.GetTokenType()) || mathematicalOperations.Contains(topToken.GetTokenType()))
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
                case TokenType.EQUAL:
                    return 0;
                case TokenType.NOT_EQUAL:
                    return 0;
                case TokenType.GREATER:
                    return 0;
                case TokenType.LESS:
                    return 0;
                case TokenType.GREATER_EQUAL:
                    return 0;
                case TokenType.LESS_EQUAL:
                    return 0;
                case TokenType.OR:
                    return 1;
                case TokenType.AND:
                    return 2;
                case TokenType.ADD:
                    return 3;
                case TokenType.SUB: 
                    return 3;
                case TokenType.MUL:
                    return 4;
                case TokenType.DIV:
                    return 4;
                case TokenType.MOD:
                    return 4;
                case TokenType.EXP:
                    return 5;
            }
            return -1;
        }

        private string[] MapInputStatement(List<Token> promptExpression)
        {
            List<string> instructions = new List<string>();
            string instrLine = "ERROR";
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };

            foreach (Token e in promptExpression)
            {
                if (e.GetTokenType() == TokenType.VARIABLE)
                {
                    instrLine = "LOAD_VAR " + variablesDict[e.GetLiteral()];
                }
                else if (literals.Contains(e.GetTokenType()))
                {
                    instrLine = "LOAD_CONST " + e.GetLiteral();
                }
                else
                {
                    throw new Exception("ERROR: Invalid token in string");
                }
                instructions.Add(instrLine);
            }

            instrLine = "CALL INPUT";
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private bool ContainsExpressions(List<Token> expression)
        {
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };

            List<TokenType> tokenTypeExpression = new List<TokenType>();

            foreach (Token e in expression)
            {
                tokenTypeExpression.Add(e.GetTokenType());
            }

            // Does the print statement have an expression
            foreach (TokenType t in mathematicalOperations)
            {
                if (tokenTypeExpression.Contains(t))
                {
                    return true;
                }
            }

            return false;
        }

        // Rewrite the expression handling
        private string[] MapPrintStatement(List<Token> expression)
        {
            List<string> instructions = new List<string>();
            List<string> instrLine;
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };

            List<int> begins = new List<int>();
            List<int> ends = new List<int>();
            bool inExpression = false;
            int nonExpressionTotalMembers = 0;
            int numberOfExpressions = 0;

            if (ContainsExpressions(expression))
            {
                for (int i = 0; i < expression.Count; i++)
                {
                    nonExpressionTotalMembers++;
                    if (expression[i].GetTokenType() != TokenType.STR_LITERAL)
                    {
                        begins.Add(i);
                        inExpression = true;
                        for (; i < expression.Count && inExpression; i++)
                        {
                            if (expression[i].GetTokenType() == TokenType.STR_LITERAL && expression[i - 1].GetTokenType() != TokenType.INPUT)
                            {
                                ends.Add(i);
                                inExpression = false;
                            }
                            if (!inExpression)
                            {
                                numberOfExpressions++;
                            }
                        }
                    }
                }
                if (begins.Count != ends.Count)
                {
                    ends.Add(expression.Count);
                    numberOfExpressions++;
                    nonExpressionTotalMembers--;
                }

                List<Token> expressionForRPN;

                for (int counter = 0; counter < begins.Count; counter++)
                {
                    expressionForRPN = new List<Token>();
                    for (int i = 0; i < begins[counter] & counter == 0; i++)
                    {
                        instrLine = new List<string>();
                        Token e = expression[i];

                        instrLine.AddRange(GetInstructions(e, ref i, expression));
                        instructions.AddRange(instrLine);
                    }
                    for (int i = begins[counter]; i < ends[counter]; i++)
                    {
                        Token e = expression[i];

                        expressionForRPN.Add(e);
                    }
                    instructions.AddRange(ConvertToPostfix(expressionForRPN));
                    for (int i = ends[counter]; counter != ends.Count - 1 && i < begins[counter + 1]; i++)
                    {
                        instrLine = new List<string>();
                        Token e = expression[i];

                        instrLine.AddRange(GetInstructions(e, ref i, expression));
                        instructions.AddRange(instrLine);
                    }
                }

                instrLine = new List<string>();
                for (int i = 0; i < nonExpressionTotalMembers + numberOfExpressions - 1; i++)
                {
                    instrLine = new List<string>();
                    instrLine.Add("ADD");
                    instructions.AddRange(instrLine);
                }
            }
            else
            {
                int inputOffset = 0;
                instructions = new List<string>();
                for (int i = 0; i < expression.Count; i++)
                {
                    instrLine = new List<string>();
                    Token e = expression[i];

                    if (e.GetTokenType() == TokenType.INPUT)
                    {
                        inputOffset++;
                    }

                    instrLine.AddRange(GetInstructions(e, ref i, expression));
                    instructions.AddRange(instrLine);
                }
                for (int i = 0; i < expression.Count - inputOffset - 1; i++)
                {
                    instrLine = new List<string>();
                    instrLine.Add("ADD");
                    instructions.AddRange(instrLine);
                }
            }

            instructions.Add("CALL PRINT");

            return instructions.ToArray();   
        }

        // Returns the correct intermediate code instruction
        private List<string> GetInstructions(Token e, ref int i, List<Token> expression)
        {
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            List<string> instrLine = new List<string>();

            if (e.GetTokenType() == TokenType.VARIABLE)
            {
                instrLine.Add("LOAD_VAR " + variablesDict[e.GetLiteral()]);
            }
            else if (literals.Contains(e.GetTokenType()))
            {
                instrLine.Add("LOAD_CONST " + e.GetLiteral());
            }
            else if (e.GetTokenType() == TokenType.INPUT)
            {
                List<Token> inputPrompt = new List<Token>();

                i++;

                inputPrompt.Add(expression[i]);

                string[] inputStatement = MapInputStatement(inputPrompt);

                foreach (string statement in inputStatement)
                {
                    instrLine.Add(statement);
                }
            }
            else
            {
                MessageBox.Show($"Pre-crash: {e.GetTokenType()}");
                throw new Exception("ERROR: Invalid token in string");
            }

            return instrLine;
        }

        // Add inputs for assignment and reassignment
        private string[] MapAssignment(string variable, List<Token> expression, string type)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            instrLine = "DECLARE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            instructions.AddRange(ConvertToPostfix(expression));

            instrLine = "ADJUST_TYPE " + type;
            instructions.Add(instrLine);

            instrLine = "STORE_VAR " + counterVar.ToString();
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

            instrLine = "DECLARE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            // Figure out a type system in the intermediate language

            //instrLine = "LOAD_VAR " + counterVar.ToString();
            //instructions.Add(instrLine);

            //instrLine = "ADJUST_TYPE " + type;
            //instructions.Add(instrLine);

            

            return instructions.ToArray();
        }

        private string[] MapIfStatement(Token[] mainExpression, Token[] mainBody, List<Token[]> elseIfExpression, List<Token[]> elseIfBodies, bool isElse, Token[] elseBody)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            int length = elseIfExpression.Count + 1;
            if (isElse)
            {
                length++;
            }

            counter += length;

            int localCounter = counter;

            instructions.AddRange(ConvertToPostfix(mainExpression.ToList()));

            if (elseIfExpression.Count > 0)
            {
                instrLine = "JUMP_FALSE " + (localCounter - length + 1).ToString();
            }
            else if (isElse)
            {
                instrLine = "JUMP_FALSE " + (localCounter - 1).ToString();
            }
            else
            {
                instrLine = "JUMP_FALSE " + (localCounter - length).ToString();
            }
            instructions.Add(instrLine);

            string[] statements = TokensToIntermediate(mainBody);
            instructions.AddRange(statements);

            instrLine = "JUMP " + (localCounter - length).ToString();
            instructions.Add(instrLine);

            int i;

            for (i = 0; i < elseIfExpression.Count; i++)
            {
                instrLine = "LABEL " + (localCounter - length + i + 1);
                instructions.Add(instrLine);

                instructions.AddRange(ConvertToPostfix(elseIfExpression[i].ToList()));

                // JUMP_FALSE to the next Else If
                if (i < elseIfExpression.Count - 1)
                {
                    instrLine = "JUMP_FALSE " + (localCounter - length + i + 2).ToString();
                }
                // JUMP_FALSE to the Else statement
                else if (isElse)
                {
                    instrLine = "JUMP_FALSE " + (localCounter - 1).ToString();
                }
                // JUMP_FALSE to the end
                else
                {
                    instrLine = "JUMP_FALSE " + (localCounter - length).ToString();
                }

                instructions.Add(instrLine);

                statements = TokensToIntermediate(elseIfBodies[i]);
                instructions.AddRange(statements);

                instrLine = "JUMP " + (localCounter - length).ToString();
                instructions.Add(instrLine);
            }

            if (isElse)
            {
                instrLine = "LABEL " + (localCounter - 1).ToString();
                instructions.Add(instrLine);

                statements = TokensToIntermediate(elseBody);
                instructions.AddRange(statements);
            }

            instrLine = "LABEL " + (localCounter - length).ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private int FindRelevantEndIndex(int index, Token[] tokens)
        {
            int nestCounter = 1;

            while (nestCounter > 0 && index < tokens.Length)
            {
                index++;

                if (tokens[index].GetTokenType() == TokenType.BEGIN)
                {
                    nestCounter++;
                }
                else if (tokens[index].GetTokenType() == TokenType.END)
                {
                    nestCounter--;
                }
            }

            return index;
        }

        // Implemented list:
        // PRINT
        // ASSIGNMENT
        // REASSIGNMENT
        // DECLARATION
        // IF
        // EON (End of Nest)
        // EOF (End of File)
        private string[] TokensToIntermediate(Token[] internalTokens)
        {
            List<string> intermediateList = new List<string>();

            int i = 0;
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };

            string type, variableName;
            bool noType;
            List<Token> expression;
            int j;

            while (i < internalTokens.Length)
            {
                Token token = internalTokens[i];
                switch (token.GetTokenType())
                {
                    case TokenType.PRINT:
                        if (internalTokens[i + 1].GetTokenType() != TokenType.EOF && internalTokens[i + 1].GetTokenType() == TokenType.VARIABLE || literals.Contains(internalTokens[i + 1].GetTokenType()) || internalTokens[i + 1].GetTokenType() == TokenType.INPUT)
                        {
                            expression = new List<Token>();
                            j = 1;
                            while ((internalTokens[i + j].GetTokenType() != TokenType.EOF && internalTokens[i + j].GetTokenType() != TokenType.EON))
                            {
                                if (internalTokens[i + j].GetTokenType() == TokenType.VARIABLE || literals.Contains(internalTokens[i + j].GetTokenType()))
                                {
                                    expression.Add(internalTokens[i + j]);
                                    j++;
                                }
                                // Limitation: in an if statement the prompt cannot contain multiple strings or variables
                                // Format without punctuation does not support this
                                else if (internalTokens[i + j].GetTokenType() == TokenType.INPUT)
                                {
                                    expression.Add(internalTokens[i + j]);
                                    j += 3;
                                }
                                else if (mathematicalOperations.Contains(internalTokens[i + j].GetTokenType()))
                                {
                                    expression.Add(internalTokens[i + j]);
                                    j++;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("ERROR: No valid text expression following print command");
                        }
                        intermediateList.AddRange(MapPrintStatement(expression));
                        i += j + 1;
                        break;
                    case TokenType.INPUT:
                        expression = new List<Token>();
                        j = 1;
                        if (internalTokens[i + 1].GetTokenType() == TokenType.WITH && internalTokens[i + 2].GetTokenType() == TokenType.PROMPT)
                        {
                            if (internalTokens[i + 3].GetTokenType() != TokenType.EOF && internalTokens[i + 3].GetTokenType() == TokenType.VARIABLE || literals.Contains(internalTokens[i + 3].GetTokenType()))
                            {
                                while ((internalTokens[i + j + 2].GetTokenType() != TokenType.EOF || internalTokens[i + j + 2].GetTokenType() != TokenType.EON) && internalTokens[i + j + 2].GetTokenType() == TokenType.VARIABLE || literals.Contains(internalTokens[i + j + 2].GetTokenType()))
                                {
                                    expression.Add(internalTokens[i + j + 2]);
                                    j++;
                                }
                            }
                            else
                            {
                                Token defaultPrompt = new Token(TokenType.STR_LITERAL, "Input: ", internalTokens[i].GetLine());
                                expression.Add(defaultPrompt);
                            }
                        }
                        intermediateList.AddRange(MapInputStatement(expression));
                        i += j + 3;
                        break;
                    case TokenType.ASSIGNMENT:
                        type = "STRING";
                        noType = true;
                        if (internalTokens[i + 1].GetTokenType() == TokenType.VARIABLE)
                        {
                            variableName = internalTokens[i + 1].GetLiteral();
                            if (internalTokens[i + 2].GetTokenType() == TokenType.TO)
                            {
                                expression = new List<Token>();
                                j = 1;
                                while (internalTokens[i + j + 2].GetTokenType() != TokenType.EOF && internalTokens[i + j + 2].GetLine() == token.GetLine() && internalTokens[i + j + 2].GetTokenType() != TokenType.AS)
                                {
                                    expression.Add(internalTokens[i + j + 2]);
                                    j++;
                                }
                                j = expression.Count;
                                if (internalTokens[i + j + 2].GetTokenType() != TokenType.EOF && internalTokens[i + j + 3].GetTokenType() == TokenType.AS && internalTokens[i + j + 4].GetTokenType() == TokenType.DATA_TYPE)
                                {
                                    type = internalTokens[i + j + 4].GetLiteral();
                                    noType = false;
                                }
                                else if (internalTokens[i + j + 3].GetTokenType() != TokenType.EOF && internalTokens[i + j + 3].GetLine() == token.GetLine())
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
                        if (internalTokens[i + 1].GetTokenType() == TokenType.VARIABLE)
                        {
                            variableName = internalTokens[i + 1].GetLiteral();
                            if (internalTokens[i + 2].GetTokenType() == TokenType.TO)
                            {
                                expression = new List<Token>();
                                j = 1;
                                while (internalTokens[i + j + 2].GetTokenType() != TokenType.EOF && internalTokens[i + j + 2].GetLine() == token.GetLine() && internalTokens[i + j + 2].GetTokenType() != TokenType.AS)
                                {
                                    expression.Add(internalTokens[i + j + 2]);
                                    j++;
                                }
                                j = expression.Count;
                                if (internalTokens[i + j + 2].GetTokenType() != TokenType.EOF && internalTokens[i + j + 3].GetTokenType() == TokenType.AS && internalTokens[i + j + 4].GetTokenType() == TokenType.DATA_TYPE)
                                {
                                    type = internalTokens[i + j + 4].GetLiteral();
                                    noType = false;
                                }
                                else if (internalTokens[i + j + 3].GetTokenType() != TokenType.EOF && internalTokens[i + j + 3].GetLine() == token.GetLine())
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
                        i += j + 3;
                        if (!noType)
                        {
                            i += 2;
                        }
                        break;
                    case TokenType.DECLARATION:
                        type = "STRING";
                        noType = true;
                        if (internalTokens[i + 1].GetTokenType() == TokenType.VARIABLE)
                        {
                            variableName = internalTokens[i + 1].GetLiteral();
                            if (internalTokens[i + 2].GetTokenType() == TokenType.AS &&
                                internalTokens[i + 3].GetTokenType() == TokenType.DATA_TYPE)
                            {
                                noType = false;
                                type = internalTokens[i + 3].GetLiteral();
                            }
                            else if (internalTokens[i + 2].GetLine() == token.GetLine() && internalTokens[i + 2].GetTokenType() != TokenType.EOF)
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
                    case TokenType.IF:
                        // Declare necessary variables
                        j = 0;
                        List<Token> internalTokensList = internalTokens.ToList();
                        List<Token> mainExpression = new List<Token>();
                        List<Token[]> elseIfExpressions = new List<Token[]>();
                        List<Token> mainBody = new List<Token>();
                        List<Token[]> elseIfBodies = new List<Token[]>();
                        List<Token> body = new List<Token>();
                        List<Token> elseBody = new List<Token>();
                        expression = new List<Token>();
                        // Get Main If Expression
                        while (internalTokens[i + j + 1].GetLine() == internalTokens[i + 1].GetLine() && internalTokens[i + j + 1].GetTokenType() != TokenType.THEN)
                        {
                            j++;
                            mainExpression.Add(internalTokens[i + j]);
                        }
                        if (internalTokens[i + j + 1].GetTokenType() != TokenType.THEN)
                        {
                            throw new Exception("ERROR: Missing \"THEN\"");
                        }
                        // Capture Main If Body
                        int bodyStart = i + j + 2;
                        int bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                        mainBody = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                        // Set i to next section
                        i = bodyEnd + 1;

                        // Identify if Else If statement(s) is present
                        while (internalTokens[i].GetTokenType() != TokenType.EOF && internalTokens[i].GetTokenType() == TokenType.ELSE && internalTokens[i + 1].GetTokenType() == TokenType.IF)
                        {
                            // Get Else If 1 Expression
                            // The while statements have + 2 because the j variable has not been updated yet
                            // This is done within the loop
                            j = 0;
                            while (internalTokens[i + j + 2].GetLine() == internalTokens[i].GetLine() && internalTokens[i + j + 2].GetTokenType() != TokenType.THEN)
                            {
                                j++;
                                expression.Add(internalTokens[i + j + 1]);
                            }
                            // + 2 is used in this case because the program is checking for the next token after the final expression token
                            if (internalTokens[i + j + 2].GetTokenType() != TokenType.THEN)
                            {
                                throw new Exception($"ERROR: Missing \"THEN\" - Found {internalTokens[i + j + 2].GetTokenType()}");
                            }
                            // Capture Else If 1 Body
                            bodyStart = i + j + 3;
                            bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                            body = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                            // Add body & expresison to lists
                            elseIfBodies.Add(body.ToArray());
                            elseIfExpressions.Add(expression.ToArray());
                            // Reset expression
                            expression = new List<Token>();
                            // Set i to next section
                            i = bodyEnd + 1;
                        }
                        // Set up Else statement
                        bool isElse = false;
                        // Identify if Else statement is present
                        if (internalTokens[i].GetTokenType() != TokenType.EOF && internalTokens[i].GetTokenType() == TokenType.ELSE)
                        {
                            isElse = true;
                            bodyStart = i;
                            bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                            elseBody = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                            i = bodyEnd + 1;
                        }
                        intermediateList.AddRange(MapIfStatement(mainExpression.ToArray(), mainBody.ToArray(), elseIfExpressions, elseIfBodies, isElse, elseBody.ToArray()));
                        break;
                    case TokenType.EOF:
                        intermediateList.Add("HALT");
                        i++;
                        break;
                    case TokenType.EON:
                        i++;
                        break;
                }
            }

            return intermediateList.ToArray();
        }
        #endregion

        #region Execution

        public void SetRunningStatus(bool status)
        {
            isRunning = status;
        }

        public bool GetRunningStatus()
        {
            return isRunning;
        }

        public bool GetValidity()
        {
            return validProgram;
        }

        public void FetchExecute(string[] intermediateCode, ref TextBox console)
        {
            if (PC < intermediateCode.Length)
            {
                string line = Fetch(intermediateCode);
                string opcode = "";
                int i;
                bool firstWord = true;
                for (i = 0; i < line.Length && firstWord; i++)
                {
                    if (line[i] != ' ')
                    {
                        opcode += line[i];
                    }
                    else
                    {
                        firstWord = false;
                    }
                }
                string operand = null;
                if (line.Length > opcode.Length)
                {
                    operand = "";
                    for (; i < line.Length; i++)
                    {
                        operand += line[i];
                    }
                }

                Execute(opcode, operand, intermediateCode, ref console);
            }
            else
            {
                isRunning = false;
            }
        }

        private string Fetch(string[] intermediateCode)
        {
            string line = intermediateCode[PC];
            PC++;
            return line;
        }

        private void Execute(string opcode, string operand, string[] intermediateCode, ref TextBox console)
        {
            // Opcodes not involving an operand in the instruction
            if (operand == null)
            {
                object object2, object1, result;
                DataType type;
                switch (opcode)
                {
                    case "ADD":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) + Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) + Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                result = Convert.ToChar(object1) + Convert.ToChar(object2);
                                break;
                            case DataType.STRING:
                                result = object1.ToString() + object2.ToString();
                                break;
                            case DataType.BOOLEAN:
                                // Logical addition - OR
                                result = Convert.ToBoolean(object1) | Convert.ToBoolean(object2);
                                break;
                            default:
                                throw new Exception("ERROR: Unknown data type");
                        }
                        stack.Push(result);
                        break;
                    case "SUB":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) - Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) - Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                throw new Exception("ERROR: Cannot subtract two characters");
                            case DataType.STRING:
                                throw new Exception("ERROR: Cannot subtract two strings");
                            case DataType.BOOLEAN:
                                throw new Exception("ERROR: Cannot subtract two strings");
                            default:
                                throw new Exception("ERROR: Unknown data type");
                        }
                        stack.Push(result);
                        break;
                    case "MUL":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) * Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) * Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                throw new Exception("ERROR: Cannot multiply two characters together");
                            case DataType.STRING:
                                throw new Exception("ERROR: Cannot multiply two strings together");
                            case DataType.BOOLEAN:
                                // Logical multiplication - AND
                                result = Convert.ToBoolean(object1) & Convert.ToBoolean(object2);
                                break;
                            default:
                                throw new Exception("ERROR: Unknown data type");
                        }
                        stack.Push(result);
                        break;
                    case "DIV":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) / Convert.ToInt32(object2);
                                double doubleResult = Convert.ToDouble(object1) / Convert.ToDouble(object2);
                                // Make the result a decimal
                                // Language is designed for intuitiveness, not computer scientific shenanigans
                                if (Convert.ToDouble(result) != doubleResult)
                                {
                                    result = doubleResult;
                                }
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) / Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                throw new Exception("ERROR: Cannot divide characters");
                            case DataType.STRING:
                                throw new Exception("ERROR: Cannot divide strings");
                            case DataType.BOOLEAN:
                                throw new Exception("ERROR: Cannot divide booleans");
                            default:
                                throw new Exception("ERROR: Unknown data type");
                        }
                        stack.Push(result);
                        break;
                    case "MOD":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) % Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) % Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                throw new Exception("ERROR: Cannot apply modulo to characters");
                            case DataType.STRING:
                                throw new Exception("ERROR: Cannot apply modulo to strings");
                            case DataType.BOOLEAN:
                                throw new Exception("ERROR: Cannot apply modulo to booleans");
                            default:
                                throw new Exception("ERROR: Unknown data type");
                        }
                        stack.Push(result);
                        break;
                    case "EXP":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Math.Pow(Convert.ToInt32(object1), Convert.ToInt32(object2));
                                break;
                            case DataType.DECIMAL:
                                result = Math.Pow(Convert.ToDouble(object1), Convert.ToDouble(object2));
                                break;
                            case DataType.CHARACTER:
                                throw new Exception("ERROR: Cannot apply exponents to characters");
                            case DataType.STRING:
                                throw new Exception("ERROR: Cannot apply exponents to strings");
                            case DataType.BOOLEAN:
                                throw new Exception("ERROR: Cannot apply exponents to booleans");
                            default:
                                throw new Exception("ERROR: Unknown data type");
                        }
                        stack.Push(result);
                        break;
                }
            }
            // Opcodes involving an operand in the instruction
            else
            {
                int intOp;
                switch (opcode)
                {
                    case "CALL":
                        if (operand == "PRINT")
                        {
                            object object1 = stack.Pop();
                            console.Text += $"{object1}\r\n";
                            // ConsoleWrite the object
                        }
                        else if (operand == "INPUT")
                        {
                            Tuple<DialogResult, string> result = new Tuple<DialogResult, string>(DialogResult.Cancel, null);
                            string prompt = stack.Pop().ToString();
                            while (result.Item1 != DialogResult.OK || result.Item2 == "")
                            {
                                result = ShowInputDialog(ref prompt);
                            }
                            console.Text += $"INPUT: {result.Item2}\r\n";
                            stack.Push(result.Item2);
                        }
                        else
                        {
                            // Subroutines ???
                            int index = subroutineDict[operand];
                            //StartExecution(intermediateSubroutines[index]);
                        }
                        break;
                    case "LOAD_CONST":
                        stack.Push(operand);
                        break;
                    case "LOAD_VAR":
                        intOp = Convert.ToInt32(operand);
                        if (variables[intOp].IsDeclared())
                        {
                            stack.Push(variables[intOp].GetValue());
                        }
                        break;
                    case "STORE_VAR":
                        intOp = Convert.ToInt32(operand);
                        if (variables[intOp].IsDeclared())
                        {
                            variables[intOp].SetValue(stack.Pop());
                        }
                        break;
                    case "DECLARE_VAR":
                        variables[Convert.ToInt32(operand)].Declare();
                        break;
                    case "JUMP":
                        intOp = Convert.ToInt32(operand);
                        PC = intOp;
                        break;
                    case "JUMP_FALSE":
                        object value = stack.Pop();
                        try
                        {
                            bool toJump = Convert.ToBoolean(value);
                            if (toJump)
                            {
                                intOp = Convert.ToInt32(operand);
                                PC = intOp;
                            }
                        }
                        catch
                        {
                            throw new Exception("ERROR: When attempting \"JUMP_FALSE\" stack was not prepped. Top of stack was not a boolean value");
                        }
                        break;
                    case "ADJUST_TYPE":
                        // Figure out what to do with the value in the stack or the variable???
                        // Move the adjust type after storing the variable and apply it to the variable
                        // By reading the previous variable assigned
                        //
                        // LOAD_CONST 5
                        // STORE_VAR 0
                        // ADJUST_TYPE INTEGER
                        // adjust the Data Type of variables[0]
                        DataType type = GetDataType(operand);
                        break;

                }
            }
        }

        private DataType GetDataType(string dataType)
        {
            switch (dataType)
            {
                case "STRING":
                    return DataType.STRING;
                case "CHARACTER":
                    return DataType.CHARACTER;
                case "INTEGER":
                    return DataType.INTEGER;
                case "DECIMAL":
                    return DataType.DECIMAL;
                case "BOOLEAN:":
                    return DataType.BOOLEAN;
                default:
                    throw new Exception("ERROR: Invalid data type parsed");
            }
        }

        // Get the variable name from its index
        // https://stackoverflow.com/questions/2444033/get-dictionary-key-by-value
        private string KeyByValue(int value)
        {
            foreach (KeyValuePair<string, int> pair in variablesDict)
            {
                if (pair.Value == value)
                {
                    return pair.Key;
                }
            }
            return null;
        }

        // Input Dialog Box
        // https://stackoverflow.com/questions/97097/what-is-the-c-sharp-version-of-vb-nets-inputbox
        // Make this a correct size using the prompt length
        private static Tuple<DialogResult, string> ShowInputDialog(ref string input)
        {
            System.Drawing.Size size = new System.Drawing.Size(200, 100);
            Form inputBox = new Form();
            
            Point location = new Point(250, 250);
            inputBox.Location = location;
            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Prompt";

            System.Windows.Forms.Label lblPrompt = new System.Windows.Forms.Label();
            lblPrompt.Size = new System.Drawing.Size(size.Width - 10, 23);
            lblPrompt.Location = new System.Drawing.Point(5, 5);
            lblPrompt.Text = input;
            inputBox.Controls.Add(lblPrompt);

            TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 30);
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 62);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 62);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult dialogResult = inputBox.ShowDialog();
            input = textBox.Text;

            Tuple<DialogResult, string> result = new Tuple<DialogResult, string>(dialogResult, input);
            return result;
        }

        private DataType IdentifyDataType(object object1)
        {
            if (object1.ToString().ToUpper() == "TRUE" || object1.ToString().ToUpper() == "FALSE")
            {
                return DataType.BOOLEAN;
            }
            try
            {
                Convert.ToDouble(object1);
                bool isDecimal = false;
                foreach (char c in object1.ToString())
                {
                    if (c == '.' && !isDecimal)
                    {
                        isDecimal = true;
                    }
                    else if (c == '.' && isDecimal)
                    {
                        throw new Exception("ERROR: More than one decimal point");
                    }
                }
                if (isDecimal)
                {
                    return DataType.DECIMAL;
                }
                return DataType.INTEGER;  
            }
            catch
            {
                try
                {
                    Convert.ToChar(object1);
                    return DataType.CHARACTER;
                }
                catch
                {
                    return DataType.STRING;
                }
            }
        }

        private DataType GetDataTypeFrom(object object1, object object2)
        {
            DataType t1 = IdentifyDataType(object1);
            DataType t2 = IdentifyDataType(object2);


            if (t1 == t2)
            {
                return t1;
            }
            else if (t1 == DataType.STRING || t2 == DataType.STRING)
            {
                return DataType.STRING;
            }
            else if (t1 == DataType.INTEGER && t2 == DataType.DECIMAL || t1 == DataType.DECIMAL && t2 == DataType.INTEGER)
            {
                return DataType.DECIMAL;
            }
            else
            {
                return DataType.STRING;
            }
        }
        #endregion
    }
}
