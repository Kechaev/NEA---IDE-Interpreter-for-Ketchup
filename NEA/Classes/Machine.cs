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
using System.CodeDom;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Net.NetworkInformation;
using System.Data;

namespace NEA
{
    class Machine
    {
        private string sourceCode;

        // Fields for Tokenization
        private Token[] tokens;
        private string[] keyword = { "CREATE", "SET", "ADD", "TAKE", "AWAY", "MULTIPLY", "DIVIDE", "GET", "THE", "REMAINDER", "OF",
                                     "MODULO", "IF", "ELSE", "COUNT", "WITH", "FROM", "GOING", "UP", "DOWN", "BY", "WHILE", "DO", "REPEAT", "FOR", "EACH", "IN", "FUNCTION",
                                     "PROCEDURE", "INPUTS", "AS", "TO", "STR_LITERAL", "CHAR_LITERAL", "INT_LITERAL", "DEC_LITERAL", "BOOL_LITERAL", "TRUE", "FALSE",
                                     "LEFT_BRACKET", "RIGHT_BRACKET", "ADD", "SUB", "MUL", "DIV", "MOD", "EXP", "IS", "A", "FACTOR",  "SQUARE_LEFT_BRACKET", "SQUARE_RIGHT_BRACKET",
                                     "MULTIPLE", "THEN", "NEWLINE", "TABSPACE", "TIMES", "DIVIDED", "RAISE", "POWER",
                                     "INPUT", "MESSAGE", "PRINT", "AND", "OR", "NOT", "END", "RETURN", "EOF"/*, "EON" /*/ };
        private int current, start, line, counter;

        public Machine(string sourceCode, string console)
        {
            Variable.ResetVariables();
            this.sourceCode = sourceCode;
            callStack = new Stack<StackFrame>();
            counterSubroutine = 0;
            subroutineDict = new Dictionary<string, int>();
            intermediateSubroutines = new List<string[]>();
            counter = 0;
            PC = 0;
            validProgram = true;
            stack = new Stack<object>();
            consoleText = console;

            fixedLoopCounter = 0;
        }

        public string GetSourceCode()
        {
            return sourceCode;
        }

        public string[] GetIntermediateCode()
        {
            return intermediate;
        }

        public List<string[]> GetSubroutinesIntermediateCode()
        {
            return intermediateSubroutines;
        }

        public Dictionary<string, int> GetSubroutineDictionary()
        {
            return subroutineDict;
        }

        private string[] FindLocalVariables(string subroutineName)
        {
            bool inFunction = false;
            bool loop = true;
            List<string> variablesFound = new List<string>();
            for (int i = 0; i < tokens.Length - 1 && loop; i++)
            {
                if (tokens[i].GetTokenType() == TokenType.FUNCTION && tokens[i + 1].GetLiteral().ToUpper() == subroutineName.ToUpper())
                {
                    inFunction = !inFunction;
                    if (inFunction == false)
                    {
                        loop = false;
                    }
                }
                if (inFunction && tokens[i].GetTokenType() == TokenType.VARIABLE && !variablesFound.Contains(tokens[i].GetLiteral()))
                {
                    variablesFound.Add(tokens[i].GetLiteral());
                }
            }

            return variablesFound.ToArray();
        }

        public void Interpret()
        {
            // Tokenization
            tokens = Tokenize();

            variables = new Variable[GetNoVariables() + GetNoUnnamedVariables()];

            OrganiseVariables();

            // Translation

            intermediate = TokensToIntermediate(tokens, false);

            // Execution is done from the form
            // To acommodate for outputs (& later inputs)
        }

        #region Tokenization - Credit to Robert Nystrom (Crafting Interpreters)

        // Tokenization heavily inspired by Nystrom's tokenization algorithms

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

        private int GetNoUnnamedVariables()
        {
            int counter = 0;
            for (int i = 0; i < tokens.Length; i++) 
            {
                Token token = tokens[i];
                if (token.GetTokenType() == TokenType.TIMES)
                {
                    counter++;
                }
            }
            return counter;
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

        // Token definitions
        public TokenType GetTokenType(string token)
        {
            switch (token.ToUpper())
            {
                case "[":
                    return TokenType.SQUARE_LEFT_BRACKET;
                case "]":
                    return TokenType.SQUARE_RIGHT_BRACKET;
                case "(":
                    return TokenType.LEFT_BRACKET;
                case ")":
                    return TokenType.RIGHT_BRACKET;
                case ",":
                    return TokenType.COMMA;
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
                //case "IS":
                //    return TokenType.IS;
                //case "A":
                //    return TokenType.A;
                //case "FACTOR":
                //    return TokenType.FACTOR;
                //case "MULTIPLE":
                //    return TokenType.MULTIPLE;
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
                case "GOING":
                    return TokenType.GOING;
                case "UP":
                    return TokenType.UP;
                case "DOWN":
                    return TokenType.DOWN;
                case "BY":
                    return TokenType.BY;
                case "WHILE":
                    return TokenType.WHILE;
                case "DO":
                    return TokenType.DO;
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
                case "MESSAGE":
                    return TokenType.MESSAGE;
                case "OR":
                    return TokenType.OR;
                case "AND":
                    return TokenType.AND;
                case "NOT":
                    return TokenType.NOT;
                case "END":
                    return TokenType.END;
                case "PRINT":
                    return TokenType.PRINT;
                case "RETURN":
                    return TokenType.RETURN;
                case "TIMES":
                    return TokenType.TIMES;
                case "DIVIDED":
                    return TokenType.DIVIDED;
                case "RAISE":
                    return TokenType.RAISE;
                case "POWER":
                    return TokenType.POWER;
                default:
                    throw new Exception($"SYNTAX ERROR: Unkown keyword: {token}.");
            }
        }

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
            int noNormalVariables = GetNoVariables();
            for (int i = 0; i < noNormalVariables; i++)
            {
                variables[i] = new Variable(KeyByValue(i), null, false);
            }
            for (int i = noNormalVariables; i < noNormalVariables + GetNoUnnamedVariables(); i++)
            {
                variablesDict.Add($"CounterVariable{i - noNormalVariables}", counter++);
                variables[i] = new Variable($"CounterVariable{i - noNormalVariables}", null, false);
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
                throw new Exception($"SYNTAX ERROR: String does not have a \" to end on.");
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
            return c == '=' || c == '>' || c == '<';
        }

        public string[] FindSubroutineNames()
        {
            List<string> subroutineNames = new List<string>();
            char[] seperators = { ' ', '\n', '(', ')' };
            string[] words = sourceCode.Split(seperators);
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i] == "FUNCTION" && !keyword.Contains(words[i + 1]))
                {
                    subroutineNames.Add(words[i + 1]);
                }
            }
            return subroutineNames.ToArray();
        }
        #endregion

        public Token[] Tokenize()
        {
            List<Token> tokensList = new List<Token>();
            char[] singleCharKeyword = { ')', '(', '+', '-', '*', '/', '%', '^', ',', '[', ']' };
            string[] multiCharKeywords = { "=", /*/ Temp /*/ "<>", ">", "<", ">=", "<=" };
            string[] dataTypes = { "STRING", "CHARACTER", "INTEGER", "DECIMAL", "BOOLEAN", "LIST" }; // Add lists and arrays

            string[] subroutineNames = FindSubroutineNames();
            subroutineParametersCount = new int[subroutineNames.Length];
            subroutineLocalVariableCounter = new int[subroutineNames.Length];

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
                    //tokensList.Add(new Token(TokenType.NEWLINE, "\n", line));
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
                            tokensList.Add(new Token(TokenType.EON, null, line));
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
                        tokensList.Add(new Token(TokenType.VARIABLE, word.ToUpper(), line));
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

            tokensList.Add(new Token(TokenType.EOF, null, line + 1));

            return tokensList.ToArray();
        }

        #endregion

        // Fields for Translation into Intermediate Code
        private string[] intermediate;
        private List<string[]> intermediateSubroutines;
        private Dictionary<string, int> subroutineDict;
        private int[] subroutineParametersCount, subroutineLocalVariableCounter;
        private Variable[] variables;
        private Dictionary<string, int> variablesDict = new Dictionary<string, int>();
        private int counterVar, counterSubroutine;
        private int fixedLoopCounter;

        #region Translation into Intermdiate

        #region Utility for Translation
        // Shunting Yard Algorithm
        // Converts list of tokens
        // To intermediate code in postfix (RPN)
        // https://en.wikipedia.org/wiki/Shunting_yard_algorithm

        private string[] ConvertToPostfix(List<Token> tokens)
        {
            List<string> output = new List<string>();
            Stack<Token> stack = new Stack<Token>();

            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] number = { TokenType.INT_LITERAL, TokenType.DEC_LITERAL };
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP, };
            TokenType[] comparisonOperators = { TokenType.EQUAL, TokenType.NOT_EQUAL, TokenType.GREATER,
                                                TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL };
            TokenType[] binaryBitwiseOperations = { TokenType.AND, TokenType.OR };
            TokenType[] unaryBitwiseOperation = { TokenType.NOT };

            // Dealing with an expression beginning with a negative number
            // -1 expressed as 0 - 1
            if (Is(tokens[0], TokenType.SUB))
            {
                output.Add("LOAD_CONST 0");
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];
                
                if (IsLiteral(token))
                {
                    output.Add("LOAD_CONST " + token.GetLiteral());
                }
                if (IsVariable(token))
                {
                    output.Add("LOAD_VAR " + variablesDict[token.GetLiteral()]);
                }
                else if (IsUnary(token))
                {
                    stack.Push(token);
                }
                else if (IsBinary(token))
                {
                    while ((stack.Count > 0) && ((Precedence(token) < Precedence(stack.Peek())) ||
                    ((Precedence(token) == Precedence(stack.Peek())) && IsLeftAssociative(token) &&
                    (!Is(stack.Peek(), TokenType.LEFT_BRACKET)))))
                    {
                        output.Add(stack.Pop().GetTokenType().ToString());
                    }
                    stack.Push(token);
                }
                else if (Is(token, TokenType.LEFT_BRACKET))
                {
                    stack.Push(token);
                }
                else if (Is(token, TokenType.RIGHT_BRACKET))
                {
                    while (stack.Count > 0 && !Is(stack.Peek(), TokenType.LEFT_BRACKET))
                    {
                        output.Add(stack.Pop().GetTokenType().ToString());
                    }
                    stack.Pop();
                    if (stack.Count > 0 && IsUnary(stack.Peek()))
                    {
                        output.Add(stack.Pop().GetTokenType().ToString());
                    }
                }
                // DO THIS LATER 
                // UNFINISHED
                //else if (false/*/Is(token, TokenType.IS)/*/)
                //{
                //    if (i + 4 < tokens.Count)
                //    {
                //        Token nextToken = tokens[i + 1];
                //        if (!Is(nextToken, TokenType.A))
                //        {
                //            throw new Exception($"DEV ERROR: Parsed \"A\" without enough tokens.");
                //        }
                //        nextToken = tokens[i + 2];
                //        if (Is(nextToken, TokenType.MULTIPLE))
                //        {
                //            while ((stack.Count > 0) && ((Precedence(token) <= Precedence(stack.Peek())) || IsUnary(stack.Peek()) && (!Is(stack.Peek(), TokenType.LEFT_BRACKET)) && IsLeftAssociative(token)))
                //            {
                //                output.Add(stack.Pop().GetTokenType().ToString());
                //            }
                //            stack.Push(new Token(TokenType.MOD, "%", nextToken.GetLine()));
                //            nextToken = tokens[i + 3];
                //            if (!Is(nextToken, TokenType.OF))
                //            {
                //                throw new Exception($"DEV ERROR: Parsed \"OF\" without enough tokens.");
                //            }
                //            nextToken = tokens[i + 4];
                //            if (IsLiteral(nextToken))
                //            {
                //                output.Add("LOAD_CONST " + nextToken.GetLiteral());
                //            }
                //        }
                //        else if (Is(nextToken, TokenType.FACTOR))
                //        {
                //            // DONT FORGET TO PUT IT BACK ON THE OUTPUT
                //            string instructionForLargerNumber = output[output.Count - 1];


                //        }
                //    }
                //    else
                //    {
                //        throw new Exception($"DEV ERROR: Parsed \"IS\" without enough tokens.");
                //    }
                //}
            }

            while (stack.Count > 0)
            {
                if (Is(stack.Peek(), TokenType.LEFT_BRACKET))
                {
                    throw new Exception($"SYNTAX ERROR on Line {stack.Peek().GetLine() + 1}: Closing bracket \")\" missing");
                }

                output.Add(stack.Pop().GetTokenType().ToString());
            }

            return output.ToArray();
        }

        private int Precedence(Token token)
        {
            switch (token.GetTokenType())
            {
                case TokenType.NOT:
                    return 8;
                case TokenType.EXP:
                    return 7;
                case TokenType.MUL:
                case TokenType.DIV:
                case TokenType.MOD:
                    return 6;
                case TokenType.ADD:
                case TokenType.SUB:
                    return 5;
                case TokenType.LESS_EQUAL:
                case TokenType.LESS:
                case TokenType.GREATER_EQUAL:
                case TokenType.GREATER:
                    return 4;
                case TokenType.EQUAL:
                case TokenType.NOT_EQUAL:
                    return 3;
                case TokenType.AND:
                    return 2;
                case TokenType.OR:
                    return 1;
                default:
                    return -1;
            }
        }

        private bool IsLeftAssociative(Token op)
        {
            switch (op.GetTokenType())
            {
                case TokenType.EXP:
                    return false;
                default:
                    return true;
            }
        }

        private bool ContainsExpressions(List<Token> expression)
        {
            #region Operations Array Declarations
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            TokenType[] comparisonOperation = { TokenType.GREATER, TokenType.LESS, TokenType.EQUAL,
                                                TokenType.GREATER_EQUAL, TokenType.LESS_EQUAL, TokenType.NOT_EQUAL };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };
            #endregion

            List<TokenType> tokenTypeExpression = new List<TokenType>();

            foreach (Token e in expression)
            {
                tokenTypeExpression.Add(e.GetTokenType());
            }

            // Does the print statement have an expression: mathematical or boolean
            foreach (TokenType t in mathematicalOperations)
            {
                if (tokenTypeExpression.Contains(t))
                {
                    return true;
                }
            }
            foreach (TokenType t in comparisonOperation)
            {
                if (tokenTypeExpression.Contains(t))
                {
                    return true;
                }
            }
            foreach (TokenType t in bitwiseOperations)
            {
                if (tokenTypeExpression.Contains(t))
                {
                    return true;
                }
            }

            return false;
        }

        private string[] GetIntermediateFromExpression(List<Token> expression)
        {
            List<string> instructions = new List<string>();

            if (ContainsExpressions(expression))
            {
                ProcessExpression(expression, ref instructions);
            }
            else
            {
                ProcessSimpleExpression(expression, ref instructions);
            }

            return instructions.ToArray();
        }

        private void ProcessExpression(List<Token> expression, ref List<string> instruction)
        {
            List<int> begins = new List<int>();
            List<int> ends = new List<int>();
            int NoOfNonSimpleExpressionTokens = 0;
            int totalTokens = expression.Count;
            IdentifyExpressions(expression, ref begins, ref ends, ref NoOfNonSimpleExpressionTokens);
            

            for (int i = 0; i < begins.Count; i++)
            {
                List<Token> expressionForRPN = SubexpressionFrom(expression, begins[i], ends[i]);
                ProcessNonExpressionParts(expression, ref instruction, i, begins, ends);
                instruction.AddRange(ConvertToPostfix(expressionForRPN));
            }

            AppendConcatenationOperators(ref instruction, NoOfNonSimpleExpressionTokens, totalTokens, begins.Count);
        }

        private void ProcessSimpleExpression(List<Token> expression, ref List<string> instructions)
        {
            int inputOffset = 0;

            for (int i = 0; i < expression.Count; i++)
            {
                Token token = expression[i];
                if (Is(token, TokenType.INPUT))
                {
                    inputOffset++;
                }

                instructions.AddRange(GetInstructions(token, ref i, expression));
            }

            for (int i = 0; i < expression.Count - inputOffset - 1; i++)
            {
                instructions.Add("ADD");
            }
        }

        private void IdentifyExpressions(List<Token> expression, ref List<int> begins, ref List<int> ends, ref int NoOfNonSimpleExpressionTokens)
        {
            bool inExpresison = false;

            for (int i = 0; i < expression.Count; i++)
            {
                Token token = expression[i];
                Console.WriteLine($"Token = {token.GetLiteral()}\nIsnt STR_LIT = {!Is(token, TokenType.STR_LITERAL)}");
                if (!Is(token, TokenType.STR_LITERAL))
                {
                    begins.Add(i);
                    inExpresison = true;

                    while (i < expression.Count && inExpresison)
                    {
                        Console.WriteLine(i);
                        token = expression[i];
                        // Avoid crash on first iteration (i = -1)
                        Token prevToken = expression[i];
                        if (i - 1 >= 0)
                        {
                            prevToken = expression[i - 1];
                        }
                        if (Is(token, TokenType.STR_LITERAL) && !Is(prevToken, TokenType.INPUT))
                        {
                            ends.Add(i);
                            inExpresison = false;
                        }
                        if (!inExpresison)
                        {
                            // Out of expression step back one token
                            i--;
                            NoOfNonSimpleExpressionTokens--;
                        }

                        NoOfNonSimpleExpressionTokens++;
                        i++;
                    }
                }
            }

            if (begins.Count != ends.Count)
            {
                ends.Add(expression.Count);
            }
        }

        private List<Token> SubexpressionFrom(List<Token> expression, int start, int end)
        {
            return expression.GetRange(start, end - start);
        }

        private void ProcessNonExpressionParts(List<Token> expression, ref List<string> instructions, int index, List<int> begins, List<int> ends)
        {
            int start;
            if (index == 0)
            {
                start = 0;
            }
            else
            {
                start = ends[index - 1];
            }

            int end;
            if (index < begins.Count)
            {
                end = begins[index];
            }
            else
            {
                end = expression.Count;
            }

            Console.WriteLine($"Start = {start}\nEnd = {end}");
            for (int i = start; i < end; i++)
            {
                instructions.AddRange(GetInstructions(expression[i], ref i, expression));
            }
        }

        private void AppendConcatenationOperators(ref List<string> instructions, int NoOfNonSimpleExpressionTokens, int totalTokens, int NoOfExpressions)
        {
            for (int i = 0; i < totalTokens - NoOfNonSimpleExpressionTokens + NoOfExpressions - 1; i++)
            {
                instructions.Add("ADD");
            }
        }

        // Returns the correct intermediate code instruction
        private List<string> GetInstructions(Token e, ref int i, List<Token> expression)
        {
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };

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

                try
                {
                    inputPrompt.Add(expression[i]);
                }
                catch
                {
                    throw new Exception($"SYNTAX ERROR on Line {e.GetLine() + 1}: No message for the user was given after \"MESSAGE\".");
                }

                string[] inputStatement = MapInputStatement(inputPrompt);

                foreach (string statement in inputStatement)
                {
                    instrLine.Add(statement);
                }
            }
            else if (bitwiseOperations.Contains(e.GetTokenType()))
            {
                if (e.GetTokenType() == TokenType.AND)
                {
                    instrLine.Add("MUL");
                }
                else if (e.GetTokenType() == TokenType.OR)
                {
                    instrLine.Add("ADD");
                }
                else if (e.GetTokenType() == TokenType.NOT)
                {
                    instrLine.Add("NOT");
                }
            }
            else
            {
                throw new Exception($"SYNTAX ERROR on Line {e.GetLine() + 1}: Invalid keyword \"{e.GetLiteral()}\" in the expression. ");
            }

            return instrLine;
        }
        #endregion

        #region Built-in Functions
        private string[] MapInputStatement(List<Token> promptExpression)
        {
            List<string> instructions = new List<string>();
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };

            foreach (Token e in promptExpression)
            {
                if (e.GetTokenType() == TokenType.VARIABLE)
                {
                    instructions.Add("LOAD_VAR " + variablesDict[e.GetLiteral()]);
                }
                else if (literals.Contains(e.GetTokenType()))
                {
                    instructions.Add("LOAD_CONST " + e.GetLiteral());
                }
                else
                {
                    throw new Exception($"SYNTAX ERROR on Line {e.GetLine() + 1}: Invalid token ({e.GetLiteral()}) in string.");
                }
            }

            instructions.Add("CALL INPUT");

            return instructions.ToArray();
        }

        private string[] MapPrintStatement(List<Token> expression)
        {
            List<string> instructions = new List<string>();

            instructions.AddRange(GetIntermediateFromExpression(expression));

            instructions.Add("CALL PRINT");

            return instructions.ToArray();
        }
        #endregion

        #region Functions

        // Anything else required for functions

        private string[] MapSubroutineCall(string subroutineName, List<Variable> parameters, List<bool> isLiteral)
        {
            List<string> instructions = new List<string>();

            for (int i = 0; i < parameters.Count; i++)
            {
                Variable p = parameters[i];
                if (isLiteral[i])
                {
                    instructions.Add($"LOAD_CONST {p.GetValue()}");
                }
                else
                {
                    instructions.Add($"LOAD_VAR {variablesDict[p.GetValue().ToString()]}");
                }
            }

            instructions.Add($"CALL {subroutineName}");

            return instructions.ToArray();
        }

        private string[] MapParameterDeclaration(List<Variable> parameters)
        {
            List<string> instructions = new List<string>();

            foreach (Variable p in parameters)
            {
                instructions.Add($"DECLARE_VAR {p.GetID()}");
            }

            return instructions.ToArray();
        }

        private string[] MapReturn(List<Token> expression)
        {
            List<string> instructions = new List<string>();

            instructions.AddRange(GetIntermediateFromExpression(expression));

            instructions.Add("RETURN");

            // Question:
            // What is the most optimal way to transfer a variable/value from within a function to outside of it.
            // Using?:
            // A register
            // Pushing it to the stack
            // other methods?

            return instructions.ToArray();
        }
        #endregion

        #region Assignment
        private string[] MapAssignment(string variable, List<Token> expression, string type)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            instructions.Add("DECLARE_VAR " + counterVar.ToString());

            instructions.AddRange(GetIntermediateFromExpression(expression));

            instructions.Add("STORE_VAR " + counterVar.ToString());

            instructions.Add("ADJUST_TYPE " + type);

            return instructions.ToArray();
        }

        private string[] MapListAssignment(string variable, List<List<Token>> listOfExpressions)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            instructions.Add("DECLARE_VAR " + counterVar.ToString());

            instructions.Add("CREATE_LIST " + counterVar.ToString());

            foreach (List<Token> expression in listOfExpressions)
            {
                instructions.AddRange(GetIntermediateFromExpression(expression));

                instructions.Add("STORE_LIST_ITEM " + counterVar.ToString());
            }

            instructions.Add("ADJUST_TYPE LIST");

            return instructions.ToArray();
        }

        private string[] MapDeclaration(string variable, string type)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            instructions.Add("DECLARE_VAR " + counterVar.ToString());

            instructions.Add("ADJUST_TYPE " + type);

            return instructions.ToArray();
        }
        #endregion

        #region If Statement
        private string[] MapIfStatement(Token[] mainExpression, Token[] mainBody, List<Token[]> elseIfExpression, List<Token[]> elseIfBodies, bool isElse, Token[] elseBody, bool inFunction)
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

            string[] statements = TokensToIntermediate(mainBody, inFunction);
            instructions.AddRange(statements);

            instructions.Add("JUMP " + (localCounter - length).ToString());

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

                statements = TokensToIntermediate(elseIfBodies[i], inFunction);
                instructions.AddRange(statements);

                instrLine = "JUMP " + (localCounter - length).ToString();
                instructions.Add(instrLine);
            }

            if (isElse)
            {
                instrLine = "LABEL " + (localCounter - 1).ToString();
                instructions.Add(instrLine);

                statements = TokensToIntermediate(elseBody, inFunction);
                instructions.AddRange(statements);
            }

            instrLine = "LABEL " + (localCounter - length).ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }
        #endregion

        #region Loops
        private string[] MapForLoop(string variable, Token[] startExpression, Token[] endExpression, Token[] stepExpression, bool negativeStep, Token[] body, bool inFunction)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            counter += 2;

            int localCounter = counter;
            int localCounterVar = counterVar;

            instructions.AddRange(ConvertToPostfix(startExpression.ToList()));

            instructions.Add("DECLARE_VAR " + localCounterVar.ToString());

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            instructions.Add("LABEL " + (localCounter - 2).ToString());

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            instructions.AddRange(ConvertToPostfix(endExpression.ToList()));

            if (negativeStep)
            {
                instructions.Add("GREATER_EQUAL");
            }
            else
            {
                instructions.Add("LESS_EQUAL");
            }

            instructions.Add("JUMP_FALSE " + (localCounter - 1).ToString());

            string[] statement = TokensToIntermediate(body, inFunction);

            instructions.AddRange(statement);

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            instructions.AddRange(ConvertToPostfix(stepExpression.ToList()));

            if (negativeStep)
            {
                instructions.Add("SUB");
            }
            else
            {
                instructions.Add("ADD");
            }

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            instructions.Add("JUMP " + (localCounter - 2).ToString());

            instructions.Add("LABEL " + (localCounter - 1).ToString());

            return instructions.ToArray();
        }

        private string[] MapWhileLoop(Token[] expression, Token[] body, bool inFunction)
        {
            List<string> instructions = new List<string>();
            string instrLine;

            counter += 2;

            int localCounter = counter;

            instrLine = "LABEL " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instrLine = "JUMP_FALSE " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            instructions.AddRange(TokensToIntermediate(body, inFunction));

            instrLine = "JUMP " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instrLine = "LABEL " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] MapDoWhileLoop(Token[] expression, Token[] body, bool inFunction)
        {
            List<string> instructions = new List<string>();
            string instrLine;

            counter += 2;

            int localCounter = counter;

            instrLine = "LABEL " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instructions.AddRange(TokensToIntermediate(body, inFunction));

            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instrLine = "JUMP_FALSE " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            instrLine = "JUMP " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instrLine = "LABEL " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] MapFixedLengthLoop(string variable, Token[] expression,Token[] body, bool inFunction)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            counter += 2;

            int localCounter = counter;
            int localCounterVar = counterVar;

            instrLine = "LOAD_CONST 1";
            instructions.Add(instrLine);

            instrLine = "DECLARE_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "STORE_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "LABEL " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instrLine = "LOAD_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instrLine = "LESS_EQUAL";
            instructions.Add(instrLine);

            instrLine = "JUMP_FALSE " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            instructions.AddRange(TokensToIntermediate(body, inFunction));

            instrLine = "LOAD_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "LOAD_CONST 1";
            instructions.Add(instrLine);

            instrLine = "ADD";
            instructions.Add(instrLine);

            instrLine = "STORE_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "JUMP " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instrLine = "LABEL " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }
        #endregion

        #region Assignment with operations
        private string[] MapAddition(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            // Intermediate code: Combines the entire expression into one value
            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("ADD");

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        private string[] MapSubtraction(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            // Intermediate code: Combines the entire expression into one value
            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("SUB");

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        private string[] MapMultiplication(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            // Intermediate code: Combines the entire expression into one value
            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("MUL");

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        private string[] MapDivision(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            // Intermediate code: Combines the entire expression into one value
            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("DIV");

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        private string[] MapModulo(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            // Intermediate code: Combines the entire expression into one value
            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("MOD");

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        private string[] MapExponentiation(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_VAR " + localCounterVar.ToString());

            // Intermediate code: Combines the entire expression into one value
            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("EXP");

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }
        #endregion

        #region Extra Token Utility

        private int FindEndIndex(int index, string structure, Token[] tokens)
        {
            //MessageBox.Show($"FindEndIndex({index}, {structure}, {tokens[tokens.Length - 1].GetTokenType()})");
            string[] structures = { "IF", "COUNT", "WHILE", "DO", "REPEAT", "ELSE", "FUNCTION", "PROCEDURE" };

            int nestCounter = 1;

            int initialIndex = index;
            index--;


            while (nestCounter > 0 && index < tokens.Length - 1)
            {
                index++;
                Token nextToken = tokens[index];
                if (Is(nextToken, TokenType.ELSE))
                {
                    nestCounter--;
                    // Exit the loop and return the index if you meet a ELSE at nestCounter = 0
                    if (nestCounter == 0)
                    {
                        continue;
                    }
                }
                if (structures.Contains(nextToken.GetTokenType().ToString().ToUpper()) && !Is(tokens[index - 1], TokenType.END))
                {
                    if (Is(nextToken, TokenType.ELSE) && Is(tokens[index + 1], TokenType.IF))
                    {
                        // Skip IF in ELSE IF
                        index++;
                    }
                    nestCounter++;
                }
                else if (Is(nextToken, TokenType.END))
                {
                    nestCounter--;
                }
            }

            if (tokens[index + 1].GetLiteral() == null)
            {
                throw new Exception($"SYNTAX ERROR following {tokens[index].GetLine() + 1}: No structure specified after \"{tokens[index].GetLiteral()}\".");
            }

            if (Is(tokens[index], TokenType.END) && tokens[index + 1].GetLiteral().ToUpper() == structure)
            {
                return index;
            }
            if (Is(tokens[index], TokenType.ELSE))
            {
                return index;
            }
            throw new Exception($"SYNTAX ERROR following {tokens[initialIndex].GetLine()}: No \"END {structure}\" command was found.");
        }

        private bool ValidLengthForIndexing(int index, int arrayLength)
        {
            return index < arrayLength;
        }

        private bool IsEndOfToken(Token token)
        {
            return token.GetTokenType() == TokenType.EOF || token.GetTokenType() == TokenType.EON;
        }

        private bool IsInput(Token token)
        {
            return token.GetTokenType() == TokenType.INPUT;
        }

        private bool IsSubroutineCall(Token token)
        {
            return token.GetTokenType() == TokenType.SUBROUTINE_NAME;
        }

        private bool IsVariable(Token token)
        {
            return token.GetTokenType() == TokenType.VARIABLE;
        }

        private bool IsLiteral(Token token)
        {
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL, TokenType.SUB };
            // Subtract to account for negative DEC or INT
            return literals.Contains(token.GetTokenType());
        }

        private bool IsLeftBracket(Token token)
        {
            return token.GetTokenType() == TokenType.LEFT_BRACKET;
        }
        
        private bool IsBracket(Token token)
        {
            return IsLeftBracket(token) || token.GetTokenType() == TokenType.RIGHT_BRACKET;
        }

        private bool IsMathsOperator(Token token)
        {
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            return mathematicalOperations.Contains(token.GetTokenType());
        }

        private bool IsBitwise(Token token)
        {
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };
            return bitwiseOperations.Contains(token.GetTokenType());
        }

        private bool IsBinaryBitwise(Token token)
        {
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR };
            return bitwiseOperations.Contains(token.GetTokenType());
        }

        private bool IsUnary(Token token)
        {
            return token.GetTokenType() == TokenType.NOT || token.GetTokenType() == TokenType.SUB;
        }

        private bool IsBinary(Token token)
        {
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            return IsBinaryBitwise(token) || mathematicalOperations.Contains(token.GetTokenType()) || IsComparison(token);
        }

        private bool IsComparison(Token token)
        {
            TokenType[] comparisonOperations = { TokenType.GREATER, TokenType.LESS, TokenType.EQUAL,
                                                 TokenType.GREATER_EQUAL, TokenType.LESS_EQUAL, TokenType.NOT_EQUAL };
            return comparisonOperations.Contains(token.GetTokenType());
        }

        private bool IsSameLine(Token token1, Token token2)
        {
            return token1.GetLine() == token2.GetLine();
        }

        private bool Is(Token token, TokenType type)
        {
            return token.GetTokenType() == type;
        }

        private Token GetLastTokenInLine(int line, Token[] internalTokens)
        {
            bool inLine = false;
            int tokenCounter = 0;
            int firstTokenIndex = 0;
            for (int i = 0; i < internalTokens.Length; i++)
            {
                Token t = internalTokens[i];
                if (t.GetLine() == line)
                {
                    if (!inLine)
                    {
                        firstTokenIndex = i;
                    }
                    inLine = true;
                    tokenCounter++;
                }
                else if (inLine && t.GetLine() != line)
                {
                    return internalTokens[firstTokenIndex + tokenCounter - 1];
                }
            }
            // This should not be reached at any point
            throw new Exception($"DEV ERROR: No token found.");
        }

        private bool PreviouslyDeclared(string variableName, int upperRange, Token[] internalToken)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                Token currentToken = tokens[i];

                if (Is(currentToken, internalToken[0].GetTokenType()) && IsSameLine(currentToken, internalToken[0]))
                {
                    for (int j = 0; j < internalToken.Length; j++)
                    {
                        Token currentInternalToken = internalToken[j];
                        currentToken = tokens[i + j];
                        if (currentInternalToken != currentToken)
                        {
                            break;
                        }
                        if (j == internalToken.Length - 1)
                        {
                            upperRange += i;
                        }
                    }
                }
            }
            for (int i = 0; i < upperRange; i++)
            {
                if (Is(tokens[i],TokenType.DECLARATION) || Is(tokens[i], TokenType.ASSIGNMENT))
                {
                    if (tokens[i + 1].GetLiteral() == variableName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool DeclaredAfter(string variableName, int lowerRange, Token[] internalToken)
        {
            for (int i = lowerRange + 1; i < tokens.Length; i++)
            {
                if (Is(tokens[i], TokenType.DECLARATION) || Is(tokens[i], TokenType.ASSIGNMENT))
                {
                    if (tokens[i + 1].GetLiteral() == variableName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        // Implemented:
        // PRINT (with Function calls)
        // ASSIGNMENT
        // REASSIGNMENT
        // DECLARATION
        // IF
        // COUNT Loop (FOR)
        // WHILE Loop
        // DO-WHILE Loop
        // Fixed-Length Loop
        // Addtion assignment
        // Subtraction assignment
        // Multiplication assignment
        // Division assignment
        // Modulo assignment
        // Exponential assignment
        // Function declaration
        // Function call (independant of assignment and/or printing) - EFFECTIVELY NOT WORKING
        // EON (End of Nest)
        // EOF (End of File)
        private string[] TokensToIntermediate(Token[] internalTokens, bool inFunction)
        {
            #region Variable Setup
            List<string> intermediateList = new List<string>();
            List<Token> internalTokensList = internalTokens.ToList();

            int i = 0;
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };
            TokenType[] comparisonOperations = { TokenType.GREATER, TokenType.LESS, TokenType.EQUAL,
                                                 TokenType.GREATER_EQUAL, TokenType.LESS_EQUAL, TokenType.NOT_EQUAL };
            TokenType[] brackets = { TokenType.LEFT_BRACKET, TokenType.RIGHT_BRACKET };

            string type, variableName, subroutineName;
            bool noType;
            List<Token> expression;
            List<List<Token>> expressions;
            int j, k, l;
            int inputOffset;
            Token nextToken;
            List<Token> body = new List<Token>();
            int currentLine;
            Token finalTokenOfLine;
            int prevI = -1;
            int bodyStart, bodyEnd;
            bool areParamsToRead = true, readyForNextParam = true;
            Stack<Variable> arguementsStack;
            Stack<bool> isLiteralArguementStack;
            int paramCounter;
            int localCounter;
            List<Variable> arguements;
            List<bool> isLiteralList;
            #endregion

            while (i < internalTokens.Length)
            {
                // Catch a repeating reading of the same token
                if (prevI == i)
                {
                    throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: Unkown token \"{internalTokens[i].GetLiteral()}\", unable to process.");
                }
                prevI = i;

                Token token = internalTokens[i];
                switch (token.GetTokenType())
                {
                    case TokenType.PRINT:
                        // Current Syntax
                        // PRINT [expression]
                        //
                        // Reject:
                        // - End of file
                        // Accept:
                        // - Variable
                        // - Any literal
                        // - An input
                        // - Left Bracket
                        // - Function call
                        nextToken = internalTokens[i + 1];
                        if (!IsEndOfToken(nextToken) && IsVariable(nextToken) || IsLiteral(nextToken) || IsInput(nextToken) || IsLeftBracket(nextToken) || IsUnary(nextToken) || IsSubroutineCall(nextToken))
                        {
                            expression = new List<Token>();
                            j = 1;
                            nextToken = internalTokens[i + j];
                            while (!IsEndOfToken(nextToken) && nextToken.GetLine() == token.GetLine())
                            {
                                if (IsVariable(nextToken) || IsLiteral(nextToken) || IsMathsOperator(nextToken) || IsBracket(nextToken) || IsBitwise(nextToken) || IsComparison(nextToken))
                                {
                                    expression.Add(nextToken);
                                    j++;
                                }
                                // Limitation: in an if statement the prompt cannot contain multiple strings or variable - not necessarily an issue for a KS3 usage
                                else if (IsInput(nextToken))
                                {
                                    // Correct Syntax:
                                    // INPUT WITH MESSAGE
                                    if (!Is(internalTokens[i + j + 1], TokenType.WITH))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"WITH\" after \"INPUT\".");
                                    }
                                    if (!Is(internalTokens[i + j + 2], TokenType.MESSAGE))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"MESSAGE\" after \"WITH\".");
                                    }
                                    expression.Add(nextToken);
                                    // Increment by 2 more to skip filler "WITH MESSAGE"
                                    // Continue onto the following string prompt
                                    j += 3;
                                }
                                else if (IsSubroutineCall(nextToken))
                                {
                                    subroutineName = nextToken.GetLiteral().ToUpper();

                                    arguementsStack = new Stack<Variable>();
                                    isLiteralArguementStack = new Stack<bool>();
                                    paramCounter = 0;
                                    nextToken = internalTokens[i + 2];

                                    if (!Is(nextToken, TokenType.LEFT_BRACKET))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Missing \"(\" after {token.GetLiteral()}");
                                    }
                                    areParamsToRead = true;
                                    readyForNextParam = true;
                                    for (j = 1; areParamsToRead; j++)
                                    {
                                        // Read as an expression until a comma then convert to instructions with Shunting Yard
                                        nextToken = internalTokens[i + j + 2];
                                        if (Is(nextToken, TokenType.RIGHT_BRACKET))
                                        {
                                            areParamsToRead = false;
                                        }
                                        else if (!IsEndOfToken(nextToken) && IsLiteral(nextToken) && readyForNextParam)
                                        {
                                            arguementsStack.Push(new Variable($"localParameter{paramCounter++}", new List<object> { nextToken.GetLiteral() }, false));
                                            isLiteralArguementStack.Push(true);
                                            readyForNextParam = false;
                                        }
                                        else if (!IsEndOfToken(nextToken) && IsVariable(nextToken) && readyForNextParam)
                                        {
                                            arguementsStack.Push(new Variable($"localParameter{paramCounter++}", new List<object> { nextToken.GetLiteral() }, false));
                                            isLiteralArguementStack.Push(false);
                                            readyForNextParam = false;
                                        }
                                        else if (!IsEndOfToken(nextToken) && Is(nextToken, TokenType.COMMA) && !readyForNextParam)
                                        {
                                            readyForNextParam = true;
                                        }
                                        else
                                        {
                                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Unknown keyword \"{nextToken.GetTokenType()}\" in the arguement.");
                                        }
                                    }

                                    localCounter = FindLocalVariables(subroutineName).Length;
                                    subroutineParametersCount[subroutineDict[subroutineName]] = paramCounter;
                                    subroutineLocalVariableCounter[subroutineDict[subroutineName]] = localCounter;
                                    arguements = new List<Variable>();
                                    isLiteralList = new List<bool>();
                                    while (arguementsStack.Count > 0)
                                    {
                                        arguements.Add(arguementsStack.Pop());
                                        isLiteralList.Add(isLiteralArguementStack.Pop());

                                    }

                                    intermediateList.AddRange(MapSubroutineCall(subroutineName, arguements, isLiteralList));

                                    i += 2;
                                }
                                try
                                {
                                    nextToken = internalTokens[i + j];
                                }
                                catch
                                {
                                    nextToken = new Token(TokenType.EOF, null, -1);
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: No valid text expression following print command.");
                        }

                        intermediateList.AddRange(MapPrintStatement(expression));
                        // Set the counter to the end of the print statement
                        i += j;
                        break;
                    case TokenType.ASSIGNMENT:
                        // Current Syntax
                        // SET variable TO [expression]
                        type = "STRING";
                        noType = true;

                        // Verify Valid Syntax

                        // Checking for valid variable to which we are assigning
                        nextToken = internalTokens[i + 1];
                        if (!IsVariable(nextToken))
                        {
                            string literal = nextToken.GetLiteral();
                            if (literal == null)
                            {
                                if (i + 2 < internalTokens.Length)
                                {
                                    literal = internalTokens[i + 2].GetLiteral();
                                }
                            }    
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: No variable found after \"SET\". \"{literal}\" is a keyword in Ketchup and cannot be used as a variable name.");
                        }
                        nextToken = internalTokens[i + 2];
                        if (nextToken.GetTokenType() != TokenType.TO)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: \"TO\" keyword not found after variable.");
                        }
                        // Get Variable Name & Expression
                        variableName = internalTokens[i + 1].GetLiteral();
            
                        j = 1;
                        inputOffset = 0;
                        nextToken = internalTokens[i + j + 2];
                        expression = new List<Token>();
                        expressions = new List<List<Token>>();
                        Console.WriteLine($"GENERAL TRIGGER\nnextToken = {nextToken.GetTokenType()} | {nextToken.GetLiteral()}");
                        if (Is(nextToken, TokenType.SQUARE_LEFT_BRACKET))
                        {
                            // List
                            Console.WriteLine("TRIGGERED");
                            nextToken = internalTokens[i + j + 3];
                            if (!IsSameLine(nextToken, token))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Variable {variableName} not set to anything.");
                            }
                            areParamsToRead = true;
                            readyForNextParam = true;
                            for (j = 1; areParamsToRead; j++)
                            {
                                nextToken = internalTokens[i + j + 3];
                                if (Is(nextToken, TokenType.SQUARE_RIGHT_BRACKET))
                                {
                                    areParamsToRead = false;
                                }
                                else if (!IsEndOfToken(nextToken) && (IsVariable(nextToken) || IsLiteral(nextToken)) && readyForNextParam)
                                {
                                    // Collect an entire expression and continue on
                                    while (ValidLengthForIndexing(i + j + 3, internalTokens.Length) && !IsEndOfToken(nextToken) && IsSameLine(internalTokens[i + j + 2], token) && !Is(nextToken, TokenType.COMMA))
                                    {
                                        Console.WriteLine($"Token: {nextToken.GetTokenType()}");
                                        expression.Add(nextToken);
                                        j++;
                                        nextToken = internalTokens[i + j + 2];
                                    }
                                    j += expression.Count;

                                    expressions.Add(expression);
                                    Console.WriteLine("Added:");
                                    foreach (Token t in expression)
                                    {
                                        Console.WriteLine($"- {t.GetTokenType()}");
                                    }

                                    readyForNextParam = false;
                                }
                                else if (!IsEndOfToken(nextToken) && Is(nextToken, TokenType.COMMA) && !readyForNextParam)
                                {
                                    readyForNextParam = true;
                                }
                                else
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Unknown keyword \"{nextToken.GetLiteral()}\" in the arguement.");
                                }
                            }
                        }
                        else
                        {
                            // Single element data type
                            if (!IsSameLine(nextToken, token))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Variable {variableName} not set to anything.");
                            }
                            while (ValidLengthForIndexing(i + j + 2, internalTokens.Length) && !IsEndOfToken(nextToken) && IsSameLine(internalTokens[i + j + 2], token) && !Is(nextToken, TokenType.AS))
                            {
                                // Limitation: in an if statement the prompt cannot contain multiple strings or variables
                                // Format without punctuation does not support this
                                if (IsInput(nextToken))
                                {
                                    if (!Is(internalTokens[i + j + 3], TokenType.WITH))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"WITH\" after \"INPUT\".");
                                    }
                                    if (!Is(internalTokens[i + j + 4], TokenType.MESSAGE))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"MESSAGE\" after \"WITH\".");
                                    }
                                    expression.Add(nextToken);
                                    j += 3;
                                    inputOffset += 2;
                                }
                                else
                                {
                                    expression.Add(nextToken);
                                    j++;
                                }
                                nextToken = internalTokens[i + j + 2];
                            }
                            j = expression.Count + inputOffset;
                        }
                        
                        // Check for type declaration
                        nextToken = internalTokens[i + j + 2];
                        if (ValidLengthForIndexing(i + j + 3, internalTokens.Length) && !IsEndOfToken(nextToken) && Is(internalTokens[i + j + 3],TokenType.AS) && Is(internalTokens[i + j + 4],TokenType.DATA_TYPE))
                        {
                            type = internalTokens[i + j + 4].GetLiteral().ToUpper();
                            noType = false;
                        }
                        else if (ValidLengthForIndexing(i + j + 3, internalTokens.Length) && !IsEndOfToken(internalTokens[i + j + 3]) && IsSameLine(internalTokens[i + j + 3],token))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: No data type mentioned after \"AS\" keyword.");
                        }

                        if (type == "LIST")
                        {
                            intermediateList.AddRange(MapListAssignment(variableName, expressions));
                        }
                        else
                        {
                            intermediateList.AddRange(MapAssignment(variableName, expression, type));
                        }
                        i += j + 3;
                        if (!noType)
                        {
                            i += 2;
                        }
                        break;
                    case TokenType.DECLARATION:
                        type = "STRING";
                        noType = true;

                        // Checking Valid Syntax - Not much to check here
                        nextToken = internalTokens[i + 1];
                        if (!IsVariable(nextToken))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: When creating no variable was found.");
                        }
                        // Getting Variable Name
                        variableName = nextToken.GetLiteral();

                        if (PreviouslyDeclared(variableName, i, internalTokens))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: Cannot create variable {variableName} more than once.");
                        }

                        if (internalTokens[i + 2].GetTokenType() == TokenType.AS &&
                                internalTokens[i + 3].GetTokenType() == TokenType.DATA_TYPE)
                        {
                            noType = false;
                            type = internalTokens[i + 3].GetLiteral();
                        }
                        else if (internalTokens[i + 2].GetLine() == token.GetLine() && internalTokens[i + 2].GetTokenType() != TokenType.EOF)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No data type specified.");
                        }
                        intermediateList.AddRange(MapDeclaration(variableName, type));
                        i += 2;
                        if (!noType)
                        {
                            i += 2;
                        }
                        break;
                    case TokenType.IF:
                        // Correct Syntax:
                        // IF <condition> THEN
                        // statement
                        // END IF
                        // ELSE IF <condition> THEN
                        // statement
                        // END IF
                        // ELSE
                        // statement
                        // END IF

                        // Declare necessary variables
                        j = 0;
                        #region Variable Declarations
                        List<Token> mainExpression = new List<Token>();
                        List<Token> mainBody = new List<Token>();
                        List<Token[]> elseIfExpressions = new List<Token[]>();
                        List<Token[]> elseIfBodies = new List<Token[]>();
                        List<Token> elseBody = new List<Token>();
                        expression = new List<Token>();
                        #endregion
                        // Check Valid Syntax - IF
                        currentLine = token.GetLine();
                        finalTokenOfLine = GetLastTokenInLine(currentLine, internalTokens);
                        if (!Is(finalTokenOfLine, TokenType.THEN))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {finalTokenOfLine.GetLine() + 1}: Missing \"THEN\" at the end.");
                        }
                        // Get Main If Expression
                        nextToken = internalTokens[i + j + 1];
                        while (!IsEndOfToken(nextToken) && IsSameLine(nextToken, token) && !Is(nextToken, TokenType.THEN))
                        {
                            mainExpression.Add(internalTokens[i + j + 1]);
                            j++;
                            nextToken = internalTokens[i + j + 1];
                        }
                        // Capture Main If Body
                        bodyStart = i + j + 2;
                        bodyEnd = FindEndIndex(bodyStart, "IF", internalTokens);

                        mainBody = internalTokensList.GetRange(bodyStart, bodyEnd - bodyStart);

                        // Set i to next section
                        i = bodyEnd;

                        Token startToken = internalTokens[i];
                        // Identify if Else If statement(s)
                        while (!IsEndOfToken(startToken) && Is(startToken, TokenType.ELSE) && Is(internalTokens[i + 1], TokenType.IF))
                        {
                            // Verify Valid Syntax - ELSE IF
                            currentLine = startToken.GetLine();
                            finalTokenOfLine = GetLastTokenInLine(currentLine, internalTokens);
                            if (!Is(finalTokenOfLine, TokenType.THEN))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {finalTokenOfLine.GetLine() + 1}: Missing \"THEN\".");
                            }
                            // Reset expression
                            expression = new List<Token>();
                            j = 0;
                            // + 2 represents ELSE IF
                            nextToken = internalTokens[i + j + 2];
                            while (IsSameLine(nextToken, startToken) && !Is(nextToken, TokenType.THEN))
                            {
                                expression.Add(nextToken);
                                j++;
                                nextToken = internalTokens[i + j + 2];
                            }
                            // + 2 is used in this case because the program is checking for the next token after the final expression token
                            if (!Is(nextToken, TokenType.THEN))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"THEN\".");
                            }
                            // Capture Else If 1 Body
                            bodyStart = i + j + 3;
                            bodyEnd = FindEndIndex(bodyStart, "IF", internalTokens);
                            body = internalTokensList.GetRange(bodyStart, bodyEnd - bodyStart);
                            // Add body & expresison to lists
                            elseIfBodies.Add(body.ToArray());
                            elseIfExpressions.Add(expression.ToArray());
                            // Set i to next section
                            i = bodyEnd;
                            // Needed for preparing the next iteration of ELSE IF
                            startToken = internalTokens[i];
                        }
                        // Set up Else statement
                        bool isElse = false;
                        // Identify if Else statement is present
                        if (!IsEndOfToken(internalTokens[i]) && Is(internalTokens[i], TokenType.ELSE))
                        {
                            isElse = true;
                            bodyStart = i + 1;
                            bodyEnd = FindEndIndex(bodyStart, "IF", internalTokens);

                            elseBody = internalTokensList.GetRange(bodyStart, bodyEnd - bodyStart);
                            i = bodyEnd;
                        }
                        // Account for the final END IF
                        // Irregardless of what terms were used
                        i += 2;
                        intermediateList.AddRange(MapIfStatement(mainExpression.ToArray(), mainBody.ToArray(), elseIfExpressions, elseIfBodies, isElse, elseBody.ToArray(), inFunction));
                        break; 
                    case TokenType.COUNT:
                        // Current Syntax:
                        // COUNT WITH variable FROM begin TO limit (BY steps)
                        // statement
                        // END COUNT
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken,TokenType.WITH))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"WITH\" keyword.");
                        }
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken,TokenType.VARIABLE))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: Missing variable from \"COUNT WITH\".");
                        }
                        nextToken = internalTokens[i + 3];
                        if (!Is(nextToken,TokenType.FROM))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 2].GetLine() + 1}: Missing \"FROM\" keyword.");
                        }
                        List<Token> expression1 = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j + 3];
                        while (IsSameLine(nextToken,token) && !Is(nextToken,TokenType.TO))
                        {
                            expression1.Add(internalTokens[i + j + 3]);
                            j++;
                            nextToken = internalTokens[i + j + 3];
                        }
                        nextToken = internalTokens[i + j + 3];
                        if (!Is(nextToken,TokenType.TO))
                        {
                            throw new Exception($"SYNTAX ERROR on {internalTokens[i + j + 2].GetLine() + 1}: Missing \"TO\" keyword.");
                        }
                        List<Token> expression2 = new List<Token>();
                        k = 1;
                        while (IsSameLine(internalTokens[i + j + k + 3],token) && !Is(internalTokens[i + j + k + 3],TokenType.GOING))
                        {
                            expression2.Add(internalTokens[i + j + k + 3]);
                            k++;
                        }
                        bool negativeStep = true;
                        nextToken = internalTokens[i + j + k + 3];
                        if (IsSameLine(nextToken, internalTokens[i + j + k + 2]))
                        {
                            if (!Is(nextToken, TokenType.GOING))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"GOING\" keyword.");
                            }
                            nextToken = internalTokens[i + j + k + 4];
                            if (!Is(nextToken, TokenType.UP) && !Is(nextToken, TokenType.DOWN))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"UP\" or \"DOWN\" keyword after \"GOING\".");
                            }
                            if (Is(nextToken, TokenType.UP))
                            {
                                negativeStep = false;
                            }
                            nextToken = internalTokens[i + j + k + 5];
                            if (!Is(nextToken, TokenType.BY))
                            {
                                if (negativeStep)
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"BY\" keyword after \"DOWN\" keyword.");
                                }
                                else
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"BY\" keyword after \"UP\" keyword.");
                                }
                            }
                        }
                        l = 0;
                        List<Token> expression3 = new List<Token>();
                        if (!IsSameLine(internalTokens[i + j + k + 4],token))
                        {
                            negativeStep = false;
                            expression3.Add(new Token(TokenType.INT_LITERAL, "1", token.GetLine()));
                        }
                        else
                        {
                            l = 1;
                            while (internalTokens[i + j + k + l + 3].GetLine() == token.GetLine())
                            {
                                expression3.Add(internalTokens[i + j + k + l + 3]);
                                l++;
                            }
                        }
                        variableName = internalTokens[i + 2].GetLiteral();
                        bodyStart = i + j + k + l + 3;
                        bodyEnd = FindEndIndex(bodyStart, "COUNT", internalTokens);
                        nextToken = internalTokens[bodyEnd + 1];
                        body = internalTokensList.GetRange(bodyStart, bodyEnd - bodyStart - 1);

                        intermediateList.AddRange(MapForLoop(variableName, expression1.ToArray(), expression2.ToArray(), expression3.ToArray(), negativeStep, body.ToArray(), inFunction));
                        i = bodyEnd + 2;
                        break;
                    case TokenType.WHILE:
                        // Current Syntax:
                        // WHILE condition THEN
                        // statements
                        // END WHILE
                        currentLine = token.GetLine();
                        finalTokenOfLine = GetLastTokenInLine(currentLine, internalTokens);
                        if (!Is(finalTokenOfLine, TokenType.THEN))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {finalTokenOfLine.GetLine() + 1}: Missing \"THEN\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j];
                        while (IsSameLine(nextToken,token) && !Is(nextToken,TokenType.THEN))
                        {
                            expression.Add(internalTokens[i + j]);
                            j++;
                            nextToken = internalTokens[i + j];
                        }
                        bodyStart = i + j + 1;
                        bodyEnd = FindEndIndex(bodyStart, "WHILE", internalTokens);
                        body = internalTokensList.GetRange(bodyStart, bodyEnd - bodyStart - 1);
                        intermediateList.AddRange(MapWhileLoop(expression.ToArray(), body.ToArray(), inFunction));
                        i = bodyEnd + 2;
                        break;
                    case TokenType.DO:
                        // Current Syntax:
                        // DO
                        // statement
                        // END DO
                        // REPEAT IF condition
                        nextToken = internalTokens[i + 1];
                        bodyStart = i + 1;
                        bodyEnd = FindEndIndex(bodyStart, "DO", internalTokens);
                        body = internalTokensList.GetRange(bodyStart, bodyEnd - bodyStart - 1);
                        i = bodyEnd + 2;
                        nextToken = internalTokens[i];
                        if (!Is(nextToken, TokenType.REPEAT))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: No \"REPEAT\" keyword found after the body.");
                        }
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken, TokenType.IF))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: No \"IF\" keyword found after \"REPEAT\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j + 1];
                        while (nextToken.GetTokenType() != TokenType.EOF && internalTokens[i + j + 1].GetLine() == internalTokens[i].GetLine())
                        {
                            expression.Add(nextToken);
                            j++;
                            nextToken = internalTokens[i + j + 1];
                        }
                        intermediateList.AddRange(MapDoWhileLoop(expression.ToArray(), body.ToArray(), inFunction));
                        i = i + j + 1;
                        break;
                    case TokenType.REPEAT:
                        // Current Syntax:
                        // REPEAT n TIMES
                        // statement
                        // END REPEAT
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j];
                        while (!IsEndOfToken(nextToken) && IsSameLine(nextToken, token) && !Is(internalTokens[i + j], TokenType.TIMES))
                        {
                            expression.Add(internalTokens[i + j]);
                            j++;
                            nextToken = internalTokens[i + j];
                        }
                        if (!Is(internalTokens[i + j],TokenType.TIMES))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + j].GetLine() + 1}: No \"TIMES\" keyword found after expression.");
                        }
                        bodyStart = i + j + 1;
                        bodyEnd = FindEndIndex(bodyStart, "REPEAT", internalTokens);
                        body = internalTokensList.GetRange(bodyStart, bodyEnd - bodyStart - 1);
                        variableName = $"CounterVariable{fixedLoopCounter++}";
                        intermediateList.AddRange(MapFixedLengthLoop(variableName, expression.ToArray(), body.ToArray(), inFunction));
                        i = bodyEnd + 2;
                        break;
                    case TokenType.ADDITION:
                        // Current Syntax:
                        // ADD x TO variable
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j];
                        while (IsSameLine(nextToken, token) && !Is(nextToken, TokenType.TO))
                        {
                            expression.Add(internalTokens[i + j]);
                            j++;
                            nextToken = internalTokens[i + j];
                        }
                        variableName = internalTokens[i + j + 1].GetLiteral();
                        intermediateList.AddRange(MapAddition(variableName, expression.ToArray()));
                        i += j + 2;
                        break;
                    case TokenType.TAKE:
                        // Current Syntax:
                        // TAKE AWAY x FROM variable
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken, TokenType.AWAY))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"AWAY\" keyword found after \"TAKE\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j + 1];
                        while (IsSameLine(nextToken, token) && !Is(nextToken, TokenType.FROM))
                        {
                            expression.Add(internalTokens[i + j + 1]);
                            j++;
                            nextToken = internalTokens[i + j + 1];
                        }
                        nextToken = internalTokens[i + j + 1];
                        if (!Is(internalTokens[i + j + 1], TokenType.FROM))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + j].GetLine() + 1}: No \"FROM\" keyword after {expression[expression.Count - 1].GetLiteral()}.");
                        }
                        variableName = internalTokens[i + j + 2].GetLiteral();
                        intermediateList.AddRange(MapSubtraction(variableName, expression.ToArray()));
                        i += j + 3;
                        break;
                    case TokenType.MULTIPLICATION:
                        // Current Syntax:
                        // MULTIPLY variable BY x
                        variableName = internalTokens[i + 1].GetLiteral();
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken, TokenType.BY))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"BY\" keyword after {variableName}.");
                        }
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j + 2];
                        while (!IsEndOfToken(nextToken) && IsSameLine(nextToken, token))
                        {
                            expression.Add(nextToken);
                            j++;
                            nextToken = internalTokens[i + j + 2];
                        }
                        intermediateList.AddRange(MapMultiplication(variableName, expression.ToArray()));
                        i += j + 2;
                        break;
                    case TokenType.DIVISION:
                        // Current Syntax:
                        // DIVIDE variable BY x
                        variableName = internalTokens[i + 1].GetLiteral();
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken, TokenType.BY))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"BY\" keyword after {variableName}.");
                        }
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j + 2];
                        while (!IsEndOfToken(nextToken) && IsSameLine(nextToken, token))
                        {
                            expression.Add(nextToken);
                            j++;
                            nextToken = internalTokens[i + j + 2];
                        }
                        intermediateList.AddRange(MapDivision(variableName, expression.ToArray()));
                        i += j + 2;
                        break;
                    case TokenType.GET:
                        // Current syntax
                        // GET (THE) REMAINDER OF variable DIVDED BY expression
                        // GET (THE) REMAINDER FROM x DIVIDED BY expression
                        int theOffset = 0;
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken, TokenType.THE))
                        {
                            theOffset = -1;
                        }
                        nextToken = internalTokens[i + 2 + theOffset];
                        if (!Is(nextToken, TokenType.REMAINDER))
                        {
                            if (theOffset == 0)
                            {
                                throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"REMAINDER\" keyword after \"THE\".");
                            }
                            else
                            {
                                throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1 + theOffset].GetLine() + 1}: No \"REMAINDER\" keyword after \"GET\".");
                            }
                        }
                        nextToken = internalTokens[i + 3 + theOffset];
                        if (!Is(nextToken, TokenType.FROM) && !Is(nextToken, TokenType.OF))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 2 + theOffset].GetLine() + 1}: No \"OF\" or \"FROM\" keyword after \"REMAINDER\".");
                        }
                        variableName = internalTokens[i + 4 + theOffset].GetLiteral();
                        nextToken = internalTokens[i + 5 + theOffset];
                        if (!Is(nextToken, TokenType.DIVIDED))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 4 + theOffset].GetLine() + 1}: No \"DIVIDED\" keyword after \"{variableName}\".");
                        }
                        nextToken = internalTokens[i + 6 + theOffset];
                        if (!Is(nextToken, TokenType.BY))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 5 + theOffset].GetLine() + 1}: No \"BY\" keyword after \"DIVIDED\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j + 6 + theOffset];
                        while (!IsEndOfToken(nextToken) && IsSameLine(nextToken, token))
                        {
                            expression.Add(nextToken);
                            j++;
                            nextToken = internalTokens[i + j + 6 + theOffset];
                        }
                        intermediateList.AddRange(MapModulo(variableName, expression.ToArray()));
                        i += j + 6 + theOffset;
                        break;
                    case TokenType.RAISE:
                        // Current Syntax:
                        // RAISE variable TO THE POWER (OF) 2
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken, TokenType.VARIABLE))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: No variable found after \"RAISE\".");
                        }
                        variableName = internalTokens[i + 1].GetLiteral();
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken, TokenType.TO))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"TO\" after \"{variableName}\".");
                        }
                        nextToken = internalTokens[i + 3];
                        if (!Is(nextToken, TokenType.THE))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 2].GetLine() + 1}: No \"THE\" keyword after \"TO\".");
                        }
                        nextToken = internalTokens[i + 4];
                        if (!Is(nextToken, TokenType.POWER))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 3].GetLine() + 1}: No \"POWER\" keyword after \"THE\".");
                        }
                        nextToken = internalTokens[i + 5];
                        if (!Is(nextToken, TokenType.OF))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 4].GetLine() + 1}: No \"OF\" keyword after \"POWER\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j + 5];
                        while (!IsEndOfToken(nextToken) && IsSameLine(nextToken, token))
                        {
                            expression.Add(nextToken);
                            j++;
                            nextToken = internalTokens[i + j + 5];
                        }
                        intermediateList.AddRange(MapExponentiation(variableName, expression.ToArray()));
                        i += j + 5;
                        break;
                    case TokenType.FUNCTION:
                        // Current Function Syntax
                        // FUNCTION subroutine WITH INPUTS a
                        //   statements
                        // END FUNCTION
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken, TokenType.SUBROUTINE_NAME))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: No valid function name found after \"FUNCTION\" keyword.");
                        }
                        subroutineName = nextToken.GetLiteral();
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken, TokenType.LEFT_BRACKET))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Missing \"(\" after {token.GetLiteral()}");
                        }
                        List<string> variableNames = new List<string>();
                        areParamsToRead = true;
                        readyForNextParam = true;
                        for (j = 1; areParamsToRead; j++)
                        {
                            nextToken = internalTokens[i + j + 2];
                            //MessageBox.Show($"Token = {nextToken.GetLiteral()}\nType = {nextToken.GetTokenType()}\nReady: {readyForNextParam}\n\n{!IsEndOfToken(nextToken)}\n{IsVariable(nextToken)} {nextToken.GetTokenType()}\n{readyForNextParam}");
                            if (Is(nextToken, TokenType.RIGHT_BRACKET))
                            {
                                areParamsToRead = false;
                            }
                            else if (!IsEndOfToken(nextToken) && IsVariable(nextToken) && readyForNextParam)
                            {
                                variableNames.Add(nextToken.GetLiteral());
                                readyForNextParam = false;
                            }
                            else if (!IsEndOfToken(nextToken) && Is(nextToken, TokenType.COMMA) && !readyForNextParam)
                            {
                                readyForNextParam = true;
                            }
                            else
                            {
                                throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Unknown keyword \"{nextToken.GetLiteral()}\" in the arguement.");
                            }
                        }
                        // Func(a,b,c,...)


                        //nextToken = internalTokens[i + 2];
                        //if (!Is(nextToken, TokenType.VARIABLE))
                        //{
                        //    throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: No variable name found after \"INPUTS\".");
                        //}

                        nextToken = internalTokens[i + j + 2];
                        bodyStart = i + j + 2;
                        //if (!IsEndOfToken(nextToken) && Is(nextToken, TokenType.AS) && !Is(internalTokens[i + 5], TokenType.DATA_TYPE))
                        //{
                        //    type = internalTokens[i + j + 3].GetLiteral();
                        //    noType = false;
                        //    bodyStart = i + j + 5;
                        //}
                        bodyEnd = FindEndIndex(bodyStart, "FUNCTION", internalTokens);

                        //MessageBox.Show($"Added FUNCTION {internalTokens[i + 1].GetLiteral()}");
                        subroutineDict.Add(subroutineName.ToUpper(), counterSubroutine);

                        List<Token> functionsTokens = new List<Token>();

                        for (int x = bodyStart; x < bodyEnd; x++)
                        {
                            functionsTokens.Add(internalTokens[x]);
                        }

                        intermediateSubroutines.Add(TokensToIntermediate(functionsTokens.ToArray(), true));

                        //string output = "Subroutines\n";
                        //int subCounter = 1;

                        //foreach (string[] subroutine in intermediateSubroutines)
                        //{
                        //    output += $"Subroutine {subCounter++}:\n\n";
                        //    foreach (string line in subroutine)
                        //    {
                        //        output += $"{line}\n";
                        //    }
                        //    output += $"\n";
                        //}

                        //MessageBox.Show($"{output}");

                        counterSubroutine++;

                        // Incorrect calculation here
                        i = bodyEnd + 2;
                        break;
                    case TokenType.RETURN:
                        if (!inFunction)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Unable to return when outside of a function.");
                        }

                        // Capture expression
                        expression = new List<Token>();
                        j = 1;
                        inputOffset = 0;
                        nextToken = internalTokens[i + j];
                        if (!IsSameLine(nextToken, token))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Cannot \"RETURN\" nothing.");
                        }

                        while (!IsEndOfToken(nextToken) && IsSameLine(internalTokens[i + j], nextToken))
                        {
                            // Limitation: in an if statement the prompt cannot contain multiple strings or variables
                            // Format without punctuation does not support this
                            if (IsInput(nextToken))
                            {
                                if (!Is(internalTokens[i + j + 1], TokenType.WITH))
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"WITH\" after \"INPUT\".");
                                }
                                if (!Is(internalTokens[i + j + 2], TokenType.MESSAGE))
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"MESSAGE\" after \"WITH\".");
                                }
                                expression.Add(nextToken);
                                j += 1;
                                inputOffset += 2;
                            }
                            else if (IsSubroutineCall(nextToken))
                            {
                                subroutineName = nextToken.GetLiteral().ToUpper();

                                arguementsStack = new Stack<Variable>();
                                isLiteralArguementStack = new Stack<bool>();
                                paramCounter = 0;
                                nextToken = internalTokens[i + 2];

                                if (!Is(nextToken, TokenType.LEFT_BRACKET))
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Missing \"(\" after {token.GetLiteral()}");
                                }
                                areParamsToRead = true;
                                readyForNextParam = true;
                                for (j = 1; areParamsToRead; j++)
                                {
                                    // Read as an expression until a comma then convert to instructions with Shunting Yard
                                    nextToken = internalTokens[i + j + 2];
                                    if (Is(nextToken, TokenType.RIGHT_BRACKET))
                                    {
                                        areParamsToRead = false;
                                    }
                                    else if (!IsEndOfToken(nextToken) && IsLiteral(nextToken) && readyForNextParam)
                                    {
                                        arguementsStack.Push(new Variable($"localParameter{paramCounter++}", new List<object> { nextToken.GetLiteral() }, false));
                                        isLiteralArguementStack.Push(true);
                                        readyForNextParam = false;
                                    }
                                    else if (!IsEndOfToken(nextToken) && IsVariable(nextToken) && readyForNextParam)
                                    {
                                        arguementsStack.Push(new Variable($"localParameter{paramCounter++}", new List<object> { nextToken.GetLiteral() }, false));
                                        isLiteralArguementStack.Push(false);
                                        readyForNextParam = false;
                                    }
                                    else if (!IsEndOfToken(nextToken) && Is(nextToken, TokenType.COMMA) && !readyForNextParam)
                                    {
                                        readyForNextParam = true;
                                    }
                                    else
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Unknown keyword \"{nextToken.GetTokenType()}\" in the arguement.");
                                    }
                                }
                            }
                            else
                            {
                                expression.Add(nextToken);
                                j++;
                            }
                            nextToken = internalTokens[i + j];
                        }
                        j = expression.Count + inputOffset;
                        i += j + 1;

                        intermediateList.AddRange(MapReturn(expression));
                        break;
                    case TokenType.SUBROUTINE_NAME:
                        // Function Call
                        // Current Syntax:
                        // FunctionName (arg1,arg2,...)
                        arguementsStack = new Stack<Variable>();
                        isLiteralArguementStack = new Stack<bool>();
                        nextToken = internalTokens[i + 1];
                        paramCounter = 0;

                        if (!Is(nextToken, TokenType.LEFT_BRACKET))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Missing \"(\" after {token.GetLiteral()}");
                        }
                        areParamsToRead = true;
                        readyForNextParam = true;
                        for (j = 1; areParamsToRead; j++)
                        {
                            nextToken = internalTokens[i + j + 1];
                            //MessageBox.Show($"Next token = {nextToken.GetTokenType()}");
                            if (Is(nextToken, TokenType.RIGHT_BRACKET))
                            {
                                areParamsToRead = false;
                            }
                            else if (!IsEndOfToken(nextToken) && IsLiteral(nextToken) && readyForNextParam)
                            {
                                arguementsStack.Push(new Variable($"localParameter{paramCounter++}", new List<object> { nextToken.GetLiteral() }, false));
                                isLiteralArguementStack.Push(true);
                                readyForNextParam = false;
                            }
                            else if (!IsEndOfToken(nextToken) && IsVariable(nextToken) && readyForNextParam)
                            {
                                // Get the variable name and parse it through as the literal of localParam
                                arguementsStack.Push(new Variable($"localParameter{paramCounter++}", new List<object> { nextToken.GetLiteral() }, false));
                                isLiteralArguementStack.Push(false);
                                readyForNextParam = false;
                            }
                            else if (!IsEndOfToken(nextToken) && Is(nextToken, TokenType.COMMA) && !readyForNextParam)
                            {
                                readyForNextParam = true;
                            }
                            else
                            {
                                throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Unknown keyword \"{nextToken}\" in the arguement.");
                            }
                        }

                        //Variable arguement = new Variable($"localParameter{paramCounter++}", nextToken.GetLiteral());
                        //arguements.Add(arguement);
                        localCounter = FindLocalVariables(token.GetLiteral()).Length;
                        //MessageBox.Show($"subroutine name = {token.GetLiteral()}");
                        //MessageBox.Show($"Subroutine Index: {subroutineDict[token.GetLiteral().ToUpper()]}");
                        //MessageBox.Show($"paramcounter = {paramCounter}");
                        //MessageBox.Show($"local counter = {localCounter}");
                        // Lock in bro
                        subroutineParametersCount[subroutineDict[token.GetLiteral().ToUpper()]] = paramCounter;
                        subroutineLocalVariableCounter[subroutineDict[token.GetLiteral().ToUpper()]] = localCounter;
                        // Return address found in the stack frame
                        // Which is referred to in the RETURN statement
                        arguements = new List<Variable>();
                        isLiteralList = new List<bool>();
                        while (arguementsStack.Count > 0)
                        {
                            arguements.Add(arguementsStack.Pop());
                            isLiteralList.Add(isLiteralArguementStack.Pop());
                        }

                        // Removed isLiteralList
                        intermediateList.AddRange(MapSubroutineCall(token.GetLiteral().ToUpper(), arguements, isLiteralList));
                        i += j + 1 + 1;
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

        // Fields for Execution
        private int PC;
        private Stack<object> stack;
        private Stack<StackFrame> callStack;
        private bool validProgram;
        private bool isRunning = false;

        private string consoleText;

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

        public string GetConsoleText()
        {
            return consoleText;
        }

        public void FetchExecute(string[] intermediateCode, ref TextBox console, bool inFunction)
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
                Execute(opcode, operand, intermediateCode, ref console, inFunction);
            }
            else
            {
                isRunning = false;
            }
        }

        private string Fetch(string[] intermediateCode)
        {
            string line = intermediateCode[PC];
            //MessageBox.Show($"Line: {line}\nPC = {PC}");
            PC++;
            return line;
        }

        private void StartSubroutineExecution(string[] intermediateCode, ref TextBox console)
        {
            // Does a call stack contain a pointer to the subroutine it was called from?
            StackFrame sf = callStack.Peek();
            while (isRunning && PC < intermediateCode.Length)
            {
                string line = intermediateCode[PC];
                FetchExecute(intermediateCode, ref console, true);
                
                if (line == "RETURN")
                {
                    return;
                }
            }
            // In that case return back to the previous subroutine from here
        }

        private void Execute(string opcode, string operand, string[] intermediateCode, ref TextBox console, bool inFunction)
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
                                throw new Exception($"LOGIC ERROR: Unknown data type when adding {object1.ToString()} and {object2.ToString()}.");
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
                                throw new Exception($"LOGIC ERROR: Cannot subtract {object2.ToString()} from {object1.ToString()} as characters.");
                            case DataType.STRING:
                                throw new Exception($"LOGIC ERROR: Cannot subtract {object2.ToString()} from {object1.ToString()} as strings.");
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot subtract {object2.ToString()} from {object1.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when subtracting {object2.ToString()} from {object1.ToString()}.");
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
                                throw new Exception($"LOGIC ERROR: Cannot multiply {object1.ToString()} with {object2.ToString()} as characters.");
                            case DataType.STRING:
                                throw new Exception($"LOGIC ERROR: Cannot multiply {object1.ToString()} with {object2.ToString()} as strings.");
                            case DataType.BOOLEAN:
                                // Logical multiplication - AND
                                result = Convert.ToBoolean(object1) & Convert.ToBoolean(object2);
                                break;
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when multiplying {object1.ToString()} with {object2.ToString()}.");
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
                                throw new Exception($"LOGIC ERROR: Cannot divide {object2.ToString()} by {object1.ToString()} as characters.");
                            case DataType.STRING:
                                throw new Exception($"LOGIC ERROR: Cannot divide {object2.ToString()} by {object1.ToString()} as strings.");
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot divide {object2.ToString()} by {object1.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when dividing {object2.ToString()} by {object1.ToString()}.");
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
                                throw new Exception($"LOGIC ERROR: Cannot apply modulo to {object1.ToString()} and {object2.ToString()} as characters.");
                            case DataType.STRING:
                                throw new Exception($"LOGIC ERROR: Cannot apply modulo to {object1.ToString()} and {object2.ToString()} as strings.");
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot apply modulo to {object1.ToString()} and {object2.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when applying modulo to {object1.ToString()} and {object2.ToString()}.");
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
                                throw new Exception($"LOGIC ERROR: Cannot apply exponents to {object1.ToString()} and {object2.ToString()} as characters.");
                            case DataType.STRING:
                                throw new Exception($"LOGIC ERROR: Cannot apply exponents to {object1.ToString()} and {object2.ToString()} as strings.");
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot apply exponents to {object1.ToString()} and {object2.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when applying exponents to {object1.ToString()} and {object2.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "OR":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        if (type != DataType.BOOLEAN)
                        {
                            throw new Exception($"LOGIC ERROR: Attempted to perform \"OR\" on non boolean values.");
                        }
                        stack.Push(Convert.ToBoolean(object1) | Convert.ToBoolean(object2));
                        break;
                    case "AND":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        if (type != DataType.BOOLEAN)
                        {
                            throw new Exception($"LOGIC ERROR: Attempted to perform \"AND\" on non boolean values.");
                        }
                        stack.Push(Convert.ToBoolean(object1) & Convert.ToBoolean(object2));
                        break;
                    case "NOT":
                        object1 = stack.Pop();
                        type = IdentifyDataType(object1.ToString());
                        switch (type)
                        {
                            case DataType.INTEGER:
                                throw new Exception($"LOGIC ERROR: Cannot apply NOT to {object1.ToString()} as an integers.");
                            case DataType.DECIMAL:
                                throw new Exception($"LOGIC ERROR: Cannot apply NOT to {object1.ToString()} as a decimal.");
                            case DataType.CHARACTER:
                                throw new Exception($"LOGIC ERROR: Cannot apply NOT to \"{object1.ToString()}\" as acharacter.");
                            case DataType.STRING:
                                throw new Exception($"LOGIC ERROR: Cannot apply NOT to \"{object1.ToString()}\" as a string.");
                            case DataType.BOOLEAN:
                                result = !Convert.ToBoolean(object1);
                                break;
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when applying NOT to {object1.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "GREATER":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) > Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) > Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                result = Convert.ToInt32(object1) > Convert.ToInt32(object2);
                                break;
                            case DataType.STRING:
                                result = object1.ToString().Length > object2.ToString().Length;
                                break;
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot compare {object1.ToString()} to {object2.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when comparing {object1.ToString()} and {object2.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "LESS":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) < Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) < Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                result = Convert.ToInt32(object1) < Convert.ToInt32(object2);
                                break;
                            case DataType.STRING:
                                result = object1.ToString().Length < object2.ToString().Length;
                                break;
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot compare {object1.ToString()} to {object2.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when comparing {object1.ToString()} to {object2.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "GREATER_EQUAL":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) >= Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) >= Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                result = Convert.ToInt32(object1) >= Convert.ToInt32(object2);
                                break;
                            case DataType.STRING:
                                result = object1.ToString().Length >= object2.ToString().Length;
                                break;
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot compare {object1.ToString()} to {object2.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when comparing {object1.ToString()} to {object2.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "LESS_EQUAL":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) <= Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) <= Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                result = Convert.ToInt32(object1) <= Convert.ToInt32(object2);
                                break;
                            case DataType.STRING:
                                result = object1.ToString().Length <= object2.ToString().Length;
                                break;
                            case DataType.BOOLEAN:
                                throw new Exception($"LOGIC ERROR: Cannot compare {object1.ToString()} to {object2.ToString()} as booleans.");
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when comparing {object1.ToString()} to {object2.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "EQUAL":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) == Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) == Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                result = Convert.ToChar(object1) == Convert.ToChar(object2);
                                break;
                            case DataType.STRING:
                                result = object1.ToString() == object2.ToString();
                                break;
                            case DataType.BOOLEAN:
                                result = Convert.ToBoolean(object1) == Convert.ToBoolean(object2);
                                break;
                            default:
                                throw new Exception($"ERROR: Unknown data type when comparing {object1.ToString()} to {object2.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "NOT_EQUAL":
                        object2 = stack.Pop();
                        object1 = stack.Pop();
                        type = GetDataTypeFrom(object1, object2);
                        switch (type)
                        {
                            case DataType.INTEGER:
                                result = Convert.ToInt32(object1) != Convert.ToInt32(object2);
                                break;
                            case DataType.DECIMAL:
                                result = Convert.ToDouble(object1) != Convert.ToDouble(object2);
                                break;
                            case DataType.CHARACTER:
                                result = Convert.ToChar(object1) != Convert.ToChar(object2);
                                break;
                            case DataType.STRING:
                                result = object1.ToString() != object2.ToString();
                                break;
                            case DataType.BOOLEAN:
                                result = Convert.ToBoolean(object1) != Convert.ToBoolean(object2);
                                break;
                            default:
                                throw new Exception($"LOGIC ERROR: Unknown data type when comparing {object1.ToString()} to {object2.ToString()}.");
                        }
                        stack.Push(result);
                        break;
                    case "RETURN":
                        StackFrame sf = callStack.Pop();
                        PC = Convert.ToInt32(sf.GetReturnAddress());
                        if (callStack.Count > 0)
                        {
                            FetchExecute(callStack.Peek().GetIntermediate(), ref console, true);
                        }
                        else
                        {
                            FetchExecute(intermediate, ref console, false);
                        }
                        break;
                    case "HALT":
                        isRunning = false;
                        break;
                }
            }
            // Opcodes involving an operand in the instruction
            else
            {
                int intOp;
                Variable var;
                Variable[] localVariables = variables;

                if (inFunction)
                {
                    localVariables = callStack.Peek().GetLocalVariables();
                }

                switch (opcode)
                {
                    case "CALL":
                        if (operand == "PRINT")
                        {
                            object object1 = stack.Pop();
                            consoleText += $"{object1}\r\n";
                        }
                        else if (operand == "INPUT")
                        {
                            Tuple<DialogResult, string> result = new Tuple<DialogResult, string>(DialogResult.None, "");
                            string prompt = stack.Pop().ToString();
                            result = ShowInputDialog(ref prompt);
                            if (result.Item2 == "")
                            {
                                consoleText += "[No Input Given]\r\n";
                            }
                            else
                            {
                                consoleText += $"INPUT: {result.Item2}\r\n";
                            }
                            stack.Push(result.Item2);
                        }
                        else
                        {
                            // Custom Subroutine's Written by the User
                            int index = subroutineDict[operand];
                            int parameterCount = subroutineParametersCount[subroutineDict[operand]];
                            // No values assigned to subroutineLocalVariableCounter ! ! !
                            int localVariablesCounter = subroutineLocalVariableCounter[subroutineDict[operand]];
                            // ! ! ! 
                            Variable[] parameters = new Variable[parameterCount];
                            // Find the number of local variables per subroutine
                            Variable[] local = new Variable[localVariablesCounter];
                            //MessageBox.Show($"ParamCounter = {parameterCount}");
                            for (int i = 0; i < parameterCount; i++)
                            {
                                object parameterValue = stack.Pop();

                                // DO NOT CREATE DUPLICATE VARIABLES THIS MAKES EXECUTION MUCH HARDER

                                parameters[i] = new Variable($"subroutine{subroutineDict[operand]}Param{i}", new List<object> { parameterValue }, false);
                                parameters[i].Declare();
                                // Parameters are also considered local variables, therefore add them to variables too.
                                local[i] = new Variable($"subroutine{subroutineDict[operand]}Param{i}", new List<object> { parameterValue }, false);
                                local[i].Declare();
                                //MessageBox.Show($"param = {parameters[i].GetName()}\nvalue = {parameters[i].GetValue()}");
                            }
                            for (int i = parameterCount; i < localVariablesCounter; i++)
                            {
                                local[i] = new Variable($"subroutine{subroutineDict[operand]}Local{i}", null, false);
                            }
                            StackFrame sf = new StackFrame(parameters, local, PC, true, intermediateSubroutines[index]);
                            callStack.Push(sf);

                            string output = "StackFrame:\nParameters:\n";

                            foreach (Variable v in parameters)
                            {
                                output += $"{v.GetName()} - {v.GetValue()}\n";
                            }
                            output += "Locals:\n";
                            foreach (Variable v in local)
                            {
                                output += $"{v.GetName()} - {v.GetValue()}\n";
                            }
                            output += $"Return Address = {PC}";

                            PC = 0;
                            StartSubroutineExecution(intermediateSubroutines[index], ref console);
                        }
                        break;
                    case "LOAD_CONST":
                        stack.Push(operand);
                        break;
                    case "LOAD_VAR":
                        intOp = Convert.ToInt32(operand);
                       
                        var = localVariables[intOp];
                        if (var.IsDeclared() && !var.IsNull())
                        {
                            stack.Push(localVariables[intOp].GetValue());
                        }
                        else if (!var.IsDeclared())
                        {
                            throw new Exception($"LOGIC ERROR in execution: Tried to use an undeclared local variable.");
                        }
                        else
                        {
                            throw new Exception($"DEV ERROR in execution: Attempted to use a variable {var.GetName()} with no assigned value.");
                        }
                        break;
                    case "STORE_VAR":
                        intOp = Convert.ToInt32(operand); 
                        var = localVariables[intOp];
                        if (var.IsDeclared())
                        {
                            var.SetValue(stack.Pop());
                        }
                        break;
                    case "CREATE_LIST":
                        intOp = Convert.ToInt32(operand);
                        var = localVariables[intOp];
                        if (var.IsDeclared())
                        {
                            var.CreateNewList();
                        }
                        break;
                    case "STORE_LIST_ITEM":
                        intOp = Convert.ToInt32(operand);
                        var = localVariables[intOp];
                        if (var.IsDeclared())
                        {
                            var.Add(stack.Pop());
                        }
                        break;
                    case "DECLARE_VAR":
                        intOp = Convert.ToInt32(operand);
                        localVariables[intOp].Declare();
                        localVariables[intOp].SetNull();
                        break;
                    case "JUMP":
                        intOp = Convert.ToInt32(operand);
                        PC = GetLabelCounter(intOp);
                        break;
                    case "JUMP_FALSE":
                        object value = stack.Pop();
                        try
                        {
                            bool toJump = Convert.ToBoolean(value);
                            if (!toJump)
                            {
                                intOp = Convert.ToInt32(operand);
                                PC = GetLabelCounter(intOp);
                            }
                        }
                        catch
                        {
                            throw new Exception($"DEV ERROR: When attempting \"JUMP_FALSE\" stack was not prepped. Top of stack was not a boolean value");
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
                        string lastLine = intermediateCode[PC - 2];
                        string lastOperand = "";
                        bool isOperator = true;
                        foreach (char c in lastLine)
                        {
                            if (!isOperator)
                            {
                                lastOperand += c;
                            }
                            else if (c == ' ')
                            {
                                isOperator = false;
                            }
                        }
                        int variableIndex = 0;
                        try
                        {
                            variableIndex = Convert.ToInt32(lastOperand);
                        }
                        catch
                        {
                            MessageBox.Show($"Pre-crash: {lastOperand}");
                        }
                        localVariables[variableIndex].SetDataType(type);
                        break;
                }
            }
        }

        private int GetLabelCounter(int labelNumber)
        {
            for (int i = 0; i < intermediate.Length; i++)
            {
                string line = intermediate[i];
                if (line.Split(' ')[0] == "LABEL")
                {
                    if (line.Split(' ')[1] == labelNumber.ToString())
                    {
                        return i;
                    }
                }
            }
            return -1;
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
                case "BOOLEAN":
                    return DataType.BOOLEAN;
                case "LIST":
                    return DataType.LIST;
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

        private static Tuple<DialogResult, string> ShowInputDialog(ref string prompt)
        {
            InputDialog inputDialog = new InputDialog(prompt);

            DialogResult dialogResult = inputDialog.ShowDialog();

            string input = inputDialog.GetUserInput();

            return new Tuple<DialogResult, string>(dialogResult, input);
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