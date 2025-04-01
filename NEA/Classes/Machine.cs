﻿using NEA.Classes;
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
using System.Text.RegularExpressions;

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
                                     "LEFT_BRACKET", "RIGHT_BRACKET", "ADD", "SUB", "MUL", "DIV", "MOD", "EXP",  "SQUARE_LEFT_BRACKET", "SQUARE_RIGHT_BRACKET",
                                     "MULTIPLE", "THEN", "NEWLINE", "TABSPACE", "TIMES", "DIVIDED", "RAISE", "POWER", "REMOVE", "SORT", "SWAP", "LENGTH",
                                     "INPUT", "MESSAGE", "PRINT", "AND", "OR", "NOT", "END", "RETURN", "EOF" };
        private int current;
        private int start;
        private int line;
        private int counter;

        // Instantiation for the Machine class
        public Machine(string sourceCode, string console)
        {
            // Resets variables for repeat use of the same Machine object
            Variable.ResetVariables();
            // Setting the source code in the class
            this.sourceCode = sourceCode;
            // Set up for variables
            callStack = new Stack<StackFrame>();
            subroutineDict = new Dictionary<string, int>();
            intermediateSubroutines = new List<string[]>();
            listVariableNames = new List<string>();
            counterSubroutine = 0;
            counter = 0;
            PC = 0;
            validProgram = true;
            stack = new Stack<object>();
            consoleText = console;
            fixedLoopCounter = 0;
        }

        // Calls all the methods required for interpreting in the correct order
        public void Interpret()
        {
            RearrangeSourceCode();
            // Tokenization
            tokens = Tokenize();

            variables = new Variable[GetNoVariables() + GetNoUnnamedVariables()];

            OrganiseVariables();

            // Translation

            intermediate = TokensToIntermediate(tokens, false);

            // Execution is done from the form
            // To acommodate for outputs
        }

        #region Machine Getters
        // Returns the string array of all intermediate code
        public string[] GetIntermediateCode()
        {
            return intermediate;
        }

        // Returns a list of string arrays for each set of intermediate code for each subroutine in the code
        public List<string[]> GetSubroutinesIntermediateCode()
        {
            return intermediateSubroutines;
        }

        // Returns the dictionary of subroutine names and their ID
        public Dictionary<string, int> GetSubroutineDictionary()
        {
            return subroutineDict;
        }
        #endregion

        #region Tokenization - Credit to Robert Nystrom (Crafting Interpreters)

        // Tokenization heavily inspired by Nystrom's tokenization algorithms
        // General concept of tokenization is inspired by the book
        // However it is adapted to fit the Ketchup programming language

        #region Utility
        // Rearranges the source code to move the function and procedure definitions to the start
        // This allows the sequential reading and translation of the subroutines
        private void RearrangeSourceCode()
        {
            Regex beginningFunction = new Regex(@"\bfunction\s+[a-zA-Z][a-zA-Z1-9_]*\(\s*([a-zA-Z][a-zA-Z1-9_]*\s*(,\s*[a-zA-Z][a-zA-Z1-9_]*)*)*\s*\)", RegexOptions.IgnoreCase);
            Regex beginningProcedure = new Regex(@"\bprocedure\s+[a-zA-Z][a-zA-Z1-9_]*\(\s*([a-zA-Z][a-zA-Z1-9_]*\s*(,\s*[a-zA-Z][a-zA-Z1-9_]*)*)*\s*\)", RegexOptions.IgnoreCase);
            Regex endFunction = new Regex(@"\bend\s*function", RegexOptions.IgnoreCase);
            Regex endProcedure = new Regex(@"\bend\s*procedure", RegexOptions.IgnoreCase);

            string allText = sourceCode;
            string[] lines = allText.Split('\n');

            bool inFunction = false;

            string modifiedText = "";
            string mainText = "";

            foreach (string line in lines)
            {
                if (!inFunction && (beginningFunction.IsMatch(line) ||  beginningProcedure.IsMatch(line)))
                {
                    inFunction = true;
                    modifiedText += line + "\n";
                }
                else if (inFunction)
                {
                    modifiedText += line + "\n";
                    if (inFunction && (endFunction.IsMatch(line) || endProcedure.IsMatch(line)))
                    {
                        inFunction = false;
                    }
                }
                else
                {
                    mainText += line + "\n";
                }
            }

            sourceCode = modifiedText + mainText;
        }

        // Finds all the local variables inside a certain subroutine
        private string[] FindLocalVariables(string subroutineName)
        {
            bool inFunction = false;
            bool loop = true;
            List<string> variablesFound = new List<string>();
            for (int i = 0; i < tokens.Length - 1 && loop; i++)
            {
                if ((tokens[i].GetTokenType() == TokenType.FUNCTION || tokens[i].GetTokenType() == TokenType.PROCEDURE) && tokens[i + 1].GetLiteral().ToUpper() == subroutineName.ToUpper())
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

        // Returns the number of variables there are in the program
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

        // Returns the number of counting variables in Fixed-Length loops
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

        // Returns the next character in the source code
        private char Peek()
        {
            if (current >= sourceCode.Length)
            {
                return '\0';
            }
            return sourceCode[current];
        }

        // Returns the second next character in the source code
        private char PeekNext()
        {
            if (current + 1 >= sourceCode.Length)
            {
                return '\0';
            }
            return sourceCode[current + 1];
        }

        // Verifies that the character is a digit or a letter
        private bool IsAlphaNumeric(char c)
        {
            return IsDigit(c) || IsAlpha(c);
        }

        // Verifies that the character is a letter
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        // Verifies that the character is a digit
        private bool IsDigit(char c)
        {
            return char.IsDigit(c);
        }

        // Token definitions
        // Returns the Token Type of the token given
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
                case "REMOVE":
                    return TokenType.REMOVE;
                case "SORT":
                    return TokenType.SORT;
                case "SWAP":
                    return TokenType.SWAP;
                case "LENGTH":
                    return TokenType.LENGTH;
                default:
                    throw new Exception($"SYNTAX ERROR: Unkown keyword: {token}.");
            }
        }

        // Adds each variable in the variableDictionary with a unique ID
        // Including counting variables
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

        // Returns the entire comparison operator ahead
        private string GetComparisonOperator()
        {
            while (IsComparisonOperatorChar(Peek()))
            {
                current++;
            }

            return sourceCode.Substring(start, current - start);
        }

        // Returns the entire word ahead
        private string GetWord()
        {
            while (IsAlphaNumeric(Peek()))
            {
                current++;
            }

            return sourceCode.Substring(start, current - start);
        }

        // Returns the entire number ahead (integer or decimal)
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

        // Returns the entire string ahead (string must begin and end with ")
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

        // Goes to the beginning of the next line, ignoring the rest of the current line
        private void SkipToEndOfLine()
        {
            while (Peek() != '\n' && current < sourceCode.Length)
            {
                current++;
            }
        }

        // Returns if a character is part of a comparison operator
        private bool IsComparisonOperatorChar(char c)
        {
            return c == '=' || c == '>' || c == '<';
        }

        // Finds all subroutine names and separates them from variables
        public string[] FindSubroutineNames()
        {
            List<string> subroutineNames = new List<string>();
            char[] seperators = { ' ', '\n', '(', ')' };
            string[] words = sourceCode.Split(seperators);
            for (int i = 0; i < words.Length - 1; i++)
            {
                if ((words[i] == "FUNCTION" || words[i] == "PROCEDURE") && !keyword.Contains(words[i + 1]))
                {
                    subroutineNames.Add(words[i + 1]);
                }
            }
            return subroutineNames.ToArray();
        }
        #endregion

        // Performs the entire tokenization of the source code
        // Turning all the text into an array of tokens
        public Token[] Tokenize()
        {
            List<Token> tokensList = new List<Token>();
            char[] singleCharKeyword = { ')', '(', '+', '-', '*', '/', '%', '^', ',', '[', ']' };
            string[] multiCharKeywords = { "=", "<>", ">", "<", ">=", "<=" };

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
        private int counterVar;
        private int counterSubroutine;
        private int fixedLoopCounter;
        private List<string> listVariableNames;

        #region Translation into Intermdiate

        #region Utility for Translation
        // Shunting Yard Algorithm
        // Converts list of tokens
        // To intermediate code in postfix (RPN)
        // https://en.wikipedia.org/wiki/Shunting_yard_algorithm
        // Adapted to accommodate for Tokens and boolean operations
        private string[] ConvertToPostfix(List<Token> tokens)
        {
            List<string> output = new List<string>();
            Stack<Token> stack = new Stack<Token>();

            // Dealing with an expression beginning with a negative number
            // -1 expressed as 0 - 1
            if (ValidLengthForIndexing(0, tokens.Count) && Is(tokens[0], TokenType.SUB))
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
                else if (ValidLengthForIndexing(i + 1, tokens.Count) && IsVariable(token) && Is(tokens[i + 1], TokenType.SQUARE_LEFT_BRACKET))
                {
                    output.AddRange(GetInstructions(token, ref i, tokens));
                }
                else if (IsVariable(token))
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

        // Returns the precedence of an operator
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

        // Returns if the operator is left associative
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

        #region Expressions
        // Returns if an expression with operators is contained within the tokens
        private bool ContainsExpressions(List<Token> expression)
        {
            #region Operations Array Declarations
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            TokenType[] comparisonOperation = { TokenType.GREATER, TokenType.LESS, TokenType.EQUAL,
                                                TokenType.GREATER_EQUAL, TokenType.LESS_EQUAL, TokenType.NOT_EQUAL };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };
            TokenType[] brackets = { TokenType.LEFT_BRACKET, TokenType.RIGHT_BRACKET, TokenType.SQUARE_LEFT_BRACKET, TokenType.SQUARE_RIGHT_BRACKET };
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

        // Returns the intermediate code for an entire expression
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

        // Processes the general expression with operators and text
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

        // Processes a simpler expression without operators and only text
        private void ProcessSimpleExpression(List<Token> expression, ref List<string> instructions)
        {
            int inputOffset = 0;
            int indexOffset = 0;
            int lengthOffset = 0;

            for (int i = 0; i < expression.Count; i++)
            {
                Token token = expression[i];
                if (Is(token, TokenType.INPUT))
                {
                    inputOffset++;
                }

                if (Is(token, TokenType.LENGTH))
                {
                    lengthOffset++;
                }

                if (ValidLengthForIndexing(i + 3, expression.Count) && IsVariable(token) && Is(expression[i + 1], TokenType.SQUARE_LEFT_BRACKET))
                {
                    do
                    {
                        i++;
                        indexOffset++;
                    }
                    while (i < expression.Count && !Is(expression[i], TokenType.SQUARE_RIGHT_BRACKET));
                    i -= indexOffset;
                }

                instructions.AddRange(GetInstructions(token, ref i, expression));
            }

            for (int i = 0; i < expression.Count - inputOffset - indexOffset - lengthOffset - 1; i++)
            {
                instructions.Add("ADD");
            }
        }

        // Finds all the expressions within the tokens which contain operators
        private void IdentifyExpressions(List<Token> expression, ref List<int> begins, ref List<int> ends, ref int NoOfNonSimpleExpressionTokens)
        {
            bool inExpresison = false;

            for (int i = 0; i < expression.Count; i++)
            {
                Token token = expression[i];
                if (!Is(token, TokenType.STR_LITERAL))
                {
                    begins.Add(i);
                    inExpresison = true;

                    while (i < expression.Count && inExpresison)
                    {
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

        // Returns a list of tokens between two points
        private List<Token> SubexpressionFrom(List<Token> expression, int start, int end)
        {
            return expression.GetRange(start, end - start);
        }

        // Processes the tokens which do not contain operators in them
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

            for (int i = start; i < end; i++)
            {
                instructions.AddRange(GetInstructions(expression[i], ref i, expression));
            }
        }

        // Puts on "ADD" instructions onto the stack to accommodate for concatenation of strings
        private void AppendConcatenationOperators(ref List<string> instructions, int NoOfNonSimpleExpressionTokens, int totalTokens, int NoOfExpressions)
        {
            for (int i = 0; i < totalTokens - NoOfNonSimpleExpressionTokens + NoOfExpressions - 1; i++)
            {
                instructions.Add("ADD");
            }
        }
        #endregion

        // Returns the intermediate code instruction
        private List<string> GetInstructions(Token e, ref int i, List<Token> expression)
        {
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };

            List<string> instrLine = new List<string>();
            Token nextToken;

            // Indexing
            if (ValidLengthForIndexing(i + 3, expression.Count) && IsVariable(e) && Is(expression[i + 1],TokenType.SQUARE_LEFT_BRACKET))
            {
                string variableName = expression[i].GetLiteral();
                i += 2;
                nextToken = expression[i];
                List<Token> indexingExpression = new List<Token>();
                while (!Is(nextToken, TokenType.SQUARE_RIGHT_BRACKET) && i < expression.Count)
                {
                    indexingExpression.Add(expression[i]);
                    i++;
                    if (i < expression.Count)
                    {
                        nextToken = expression[i];
                    }
                } 

                string[] indexingStatement = MapIndexing(variableName, indexingExpression);
                instrLine.AddRange(indexingStatement);
            }
            // Variable
            else if (IsVariable(e))
            {
                instrLine.Add("LOAD_VAR " + variablesDict[e.GetLiteral()]);
            }
            // Literal
            else if (IsLiteral(e))
            {
                instrLine.Add("LOAD_CONST " + e.GetLiteral());
            }
            // Input
            else if (Is(e, TokenType.INPUT))
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

                instrLine.AddRange(inputStatement);
            }
            // Length
            else if (Is(e, TokenType.LENGTH))
            {
                i++;

                string variableName = expression[i].GetLiteral().ToUpper();

                string[] lengthStatement = MapLengthStatement(variableName);

                instrLine.AddRange(lengthStatement);
            }
            // Bitwise Operator
            else if (IsBitwise(e))
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
        // Maps the input statement using a low level function call (CALL INPUT)
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

        // Maps the print statement using a low level function call (CALL PRINT)
        private string[] MapPrintStatement(List<Token> expression)
        {
            List<string> instructions = new List<string>();

            instructions.AddRange(GetIntermediateFromExpression(expression));

            instructions.Add("CALL PRINT");

            return instructions.ToArray();
        }

        // Maps sorting using a low level function (SORT)
        private string[] MapSorting(string variable)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("SORT " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        // Maps indexing setting up a variable and an index
        private string[] MapIndexing(string variable, List<Token> indexingExpression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_CONST " + localCounterVar.ToString());

            instructions.AddRange(ConvertToPostfix(indexingExpression.ToList()));

            instructions.Add("INDEX");

            return instructions.ToArray();
        }

        // Maps swapping two indexes in a variable
        // Setting up the variable and the two indexes
        // Swapping them in low level
        private string[] MapSwapping(string variable, List<Token> expression1, List<Token> expression2)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LOAD_CONST " + localCounterVar.ToString());

            instructions.AddRange(ConvertToPostfix(expression1.ToList()));

            instructions.AddRange(ConvertToPostfix(expression2.ToList()));

            instructions.Add("SWAP");

            return instructions.ToArray();
        }

        // Maps the get length statement using a low level function (LENGTH)
        private string[] MapLengthStatement(string variable)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("LENGTH " + localCounterVar.ToString());

            return instructions.ToArray();
        }
        #endregion

        #region Functions
        // Maps a function call (CALL subroutine)
        // Feeds the parameters onto the stack for the subroutine to use
        private string[] MapSubroutineCall(string subroutineName, List<Variable> parameters, List<bool> isLiteral)
        {
            List<string> instructions = new List<string>();

            // Loading all the parameters necessary for the subroutine
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

        // Declares all parameters for the subroutine
        private string[] MapParameterDeclaration(List<Variable> parameters)
        {
            List<string> instructions = new List<string>();

            foreach (Variable p in parameters)
            {
                instructions.Add($"DECLARE_VAR {p.GetID()}");
            }

            return instructions.ToArray();
        }

        // Maps the return function
        // Pushing the expression to return onto the stack
        // Using it after the returning to the previous stack frame
        private string[] MapReturn(List<Token> expression)
        {
            List<string> instructions = new List<string>();

            instructions.AddRange(GetIntermediateFromExpression(expression));

            instructions.Add("RETURN");

            return instructions.ToArray();
        }
        #endregion

        #region Assignment
        // Maps normal variable assignment (single element)
        private string[] MapAssignment(string variable, List<Token> expression, string type)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("DECLARE_VAR " + localCounterVar.ToString());

            instructions.AddRange(GetIntermediateFromExpression(expression));

            instructions.Add("STORE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        // Maps list assignment
        private string[] MapListAssignment(string variable, List<List<Token>> listOfExpressions)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("DECLARE_VAR " + localCounterVar.ToString());

            instructions.Add("CREATE_LIST " + localCounterVar.ToString());

            foreach (List<Token> expression in listOfExpressions)
            {
                instructions.AddRange(GetIntermediateFromExpression(expression));

                instructions.Add("STORE_LIST_ITEM " + localCounterVar.ToString());
            }

            return instructions.ToArray();
        }

        // Maps declaration with assignment
        private string[] MapDeclaration(string variable)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.Add("DECLARE_VAR " + localCounterVar.ToString());

            return instructions.ToArray();
        }
        #endregion

        #region If Statement
        // Maps an if statement
        // Connecting the boolean expressions with their bodies of code upon which they are executed on
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
        // Maps for loop
        // Considering if the step is negative or positive, adapting the intermediate code for this
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

        // Maps a while loop
        // Creating a boolean expression dependent label loop
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

        // Maps a do while loop
        // Following the same logic as a while loop
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

        // Maps a fixed length loop
        // Functioning similarly to a for loop
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
        // Maps list addition
        // Using the low level STORE_LIST_ITEM instruction
        private string[] MapListAddition(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("STORE_LIST_ITEM " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        // Maps addition assignment
        // Adding a value to a variable and assigning their sum as the new variable
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

        // Maps removal from a list
        // Removes an item with that value from the list
        private string[] MapListSubtraction(string variable, Token[] expression)
        {
            List<string> instructions = new List<string>();
            counterVar = variablesDict[variable];

            int localCounterVar = counterVar;

            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instructions.Add("REMOVE_LIST_ITEM " + localCounterVar.ToString());

            return instructions.ToArray();
        }

        // Maps subtraction assignment
        // Subtracting a value from a variable and assiging the result to the variable
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

        // More niche and use cases

        // Maps multiplication assignment
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

        // Maps division assignment
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

        // Maps modulo assignment
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

        // Maps exponential assignment
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
        // Finds the index of the last token related to the structure in question
        private int FindEndIndex(int index, string structure, Token[] tokens)
        {
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

        // Ensures that the if statement does not crash due to "index out of bound"
        private bool ValidLengthForIndexing(int index, int arrayLength)
        {
            return index < arrayLength;
        }

        // Verifies that the tokens have not ended
        private bool IsEndOfToken(Token token)
        {
            return Is(token, TokenType.EOF) || Is(token, TokenType.EON);
        }

        // Verifies that the token is an input
        private bool IsInput(Token token)
        {
            return Is(token, TokenType.INPUT);
        }

        // Verifies that the token is a length getter
        private bool IsLength(Token token)
        {
            return Is(token, TokenType.LENGTH);
        }

        // Verifies that the token is a subroutine name (call)
        private bool IsSubroutineCall(Token token)
        {
            return Is(token, TokenType.SUBROUTINE_NAME);
        }

        // Verifies that the token is a variable
        private bool IsVariable(Token token)
        {
            return Is(token, TokenType.VARIABLE);
        }

        // Verifies that the token is a literal
        private bool IsLiteral(Token token)
        {
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            return literals.Contains(token.GetTokenType());
        }

        // Verifies that the token opens a bracketed expression
        private bool IsLeftBracket(Token token)
        {
            return Is(token, TokenType.LEFT_BRACKET);
        }
        
        // Verifies that the token is any "round" bracket (for BIDMAS)
        private bool IsBracket(Token token)
        {
            return IsLeftBracket(token) || Is(token, TokenType.RIGHT_BRACKET);
        }

        // Verifies that the token is a mathematical operator
        private bool IsMathsOperator(Token token)
        {
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            return mathematicalOperations.Contains(token.GetTokenType());
        }

        // Verifies that the token is ANY bitwise operator
        private bool IsBitwise(Token token)
        {
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };
            return bitwiseOperations.Contains(token.GetTokenType());
        }

        // Verifies that the token is a BINARY bitwise operator
        private bool IsBinaryBitwise(Token token)
        {
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR };
            return bitwiseOperations.Contains(token.GetTokenType());
        }

        // Verifies that the token operates with one operand
        private bool IsUnary(Token token)
        {
            return token.GetTokenType() == TokenType.NOT || token.GetTokenType() == TokenType.SUB;
        }

        // Verifies that the token operates with two operands
        private bool IsBinary(Token token)
        {
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            return IsBinaryBitwise(token) || mathematicalOperations.Contains(token.GetTokenType()) || IsComparison(token);
        }

        // Verifies that the token is a comparison operator (returns bool)
        private bool IsComparison(Token token)
        {
            TokenType[] comparisonOperations = { TokenType.GREATER, TokenType.LESS, TokenType.EQUAL,
                                                 TokenType.GREATER_EQUAL, TokenType.LESS_EQUAL, TokenType.NOT_EQUAL };
            return comparisonOperations.Contains(token.GetTokenType());
        }

        // Verifies that the two tokens are on the same line
        private bool IsSameLine(Token token1, Token token2)
        {
            return token1.GetLine() == token2.GetLine();
        }

        // GOD FUNCTION: Verifies if a token is a certain TokenType (99+ references) 👍
        private bool Is(Token token, TokenType type)
        {
            return token.GetTokenType() == type;
        }

        // Retrieves the last token in that line
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

        // Verifies that the variable has been declared previously
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

            string type;
            string variableName;
            string subroutineName;
            bool noType;
            List<Token> expression;
            List<List<Token>> expressions;
            int j, k, l;
            int inputOffset;
            int lengthOffset;
            Token nextToken;
            Token prevToken = new Token(TokenType.EOF, "", -1);
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
            List<string> variableNames;
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
                        // - Variable (Lists - prints them accordingly)
                        // - Any literal
                        // - An input
                        // - Left Bracket
                        // - Function call
                        nextToken = internalTokens[i + 1];
                        if (!IsEndOfToken(nextToken) && IsVariable(nextToken) || IsLiteral(nextToken) || IsInput(nextToken) || IsLeftAssociative(nextToken) || IsLeftBracket(nextToken) || IsUnary(nextToken) || IsSubroutineCall(nextToken))
                        {
                            expression = new List<Token>();
                            j = 1;
                            nextToken = internalTokens[i + j];
                            while (!IsEndOfToken(nextToken) && nextToken.GetLine() == token.GetLine())
                            {
                                if (prevToken == nextToken)
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {prevToken.GetLine() + 1}: Incorrectly formatted PRINT statement");
                                }
                                prevToken = nextToken;

                                // List indexing
                                if (ValidLengthForIndexing(i + j + 1, internalTokens.Length) && IsVariable(nextToken) && Is(internalTokens[i + j + 1], TokenType.SQUARE_LEFT_BRACKET))
                                {
                                    expression = new List<Token>();
                                    expression.Add(nextToken);
                                    nextToken = internalTokens[i + j + 2];
                                    if (!IsLiteral(nextToken) && !IsVariable(nextToken))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine()}: {nextToken.GetLiteral()} must be a literal or a variable to index.");
                                    }
                                    expression.Add(new Token(TokenType.SQUARE_LEFT_BRACKET, "[", nextToken.GetLine()));
                                    while (!IsEndOfToken(nextToken) && !Is(nextToken, TokenType.SQUARE_RIGHT_BRACKET))
                                    {
                                        expression.Add(nextToken);
                                        j++;
                                        nextToken = internalTokens[i + j + 2];
                                    }

                                    if (!Is(nextToken, TokenType.SQUARE_RIGHT_BRACKET))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"]\" after indexing expression.");
                                    }
                                    expression.Add(new Token(TokenType.SQUARE_RIGHT_BRACKET, "]", nextToken.GetLine()));

                                    // Index Offset
                                    j += 3;
                                }
                                // Other elements
                                else if (IsVariable(nextToken) || IsLiteral(nextToken) || IsMathsOperator(nextToken) || IsBracket(nextToken) || IsBitwise(nextToken) || IsComparison(nextToken))
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
                                else if (IsLength(nextToken))
                                {
                                    // Correct Syntax:
                                    // LENGTH OF var
                                    if (!Is(internalTokens[i + j + 1], TokenType.OF))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"OF\" after \"LENGTH\".");
                                    }
                                    expression.Add(nextToken);
                                    j += 2;
                                }
                                else if (IsSubroutineCall(nextToken))
                                {
                                    subroutineName = nextToken.GetLiteral().ToUpper();

                                    arguementsStack = new Stack<Variable>();
                                    isLiteralArguementStack = new Stack<bool>();
                                    paramCounter = 0;
                                    nextToken = internalTokens[i + j + 1];

                                    if (!Is(nextToken, TokenType.LEFT_BRACKET))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Missing \"(\" after {token.GetLiteral()}");
                                    }
                                    areParamsToRead = true;
                                    readyForNextParam = true;
                                    for (; areParamsToRead; j++)
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

                                    // Sets up parameters and local variable for a subroutine call
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

                                    j += 2;
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
                        lengthOffset = 0;
                        nextToken = internalTokens[i + j + 2];
                        expression = new List<Token>();
                        expressions = new List<List<Token>>();
                        // List
                        if (Is(nextToken, TokenType.SQUARE_LEFT_BRACKET))
                        {
                            type = "LIST";
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
                                    while (!IsEndOfToken(nextToken) && !Is(nextToken, TokenType.COMMA) && !Is(nextToken, TokenType.SQUARE_RIGHT_BRACKET))
                                    {
                                        expression.Add(nextToken);
                                        j++;
                                        nextToken = internalTokens[i + j + 3];
                                    }
                                    expressions.Add(new List<Token>(expression));
                                    expression.Clear();
                                    j--;
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
                        // Single element data type
                        else
                        {
                            if (!IsSameLine(nextToken, token))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Variable {variableName} not set to anything.");
                            }
                            while (ValidLengthForIndexing(i + j + 2, internalTokens.Length) && !IsEndOfToken(nextToken) && IsSameLine(internalTokens[i + j + 2], token) && !IsEndOfToken(nextToken))
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
                                else if (IsLength(nextToken))
                                {
                                    // Correct Syntax:
                                    // LENGTH OF var
                                    if (!Is(internalTokens[i + j + 3], TokenType.OF))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"OF\" after \"LENGTH\".");
                                    }
                                    expression.Add(nextToken);
                                    j += 2;
                                    lengthOffset += 1;
                                }
                                else if (IsSubroutineCall(nextToken))
                                {
                                    subroutineName = nextToken.GetLiteral().ToUpper();

                                    arguementsStack = new Stack<Variable>();
                                    isLiteralArguementStack = new Stack<bool>();
                                    paramCounter = 0;
                                    nextToken = internalTokens[i + j + 3];

                                    if (!Is(nextToken, TokenType.LEFT_BRACKET))
                                    {
                                        throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Missing \"(\" after {token.GetLiteral()}");
                                    }
                                    areParamsToRead = true;
                                    readyForNextParam = true;
                                    for (; areParamsToRead; j++)
                                    {
                                        // Read as an expression until a comma then convert to instructions with Shunting Yard
                                        nextToken = internalTokens[i + j + 4];
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

                                    // Sets up parameters and local variables
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

                                    j += 2;
                                }
                                else
                                {
                                    expression.Add(nextToken);
                                    j++;
                                }
                                nextToken = internalTokens[i + j + 2];
                            }
                            j = expression.Count + inputOffset + lengthOffset;
                        }

                        if (type == "LIST")
                        {
                            intermediateList.AddRange(MapListAssignment(variableName, expressions));
                            listVariableNames.Add(variableName.ToUpper());
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

                        intermediateList.AddRange(MapDeclaration(variableName));
                        i += 2;
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
                            // Assigns the negativeStep variable to false if counting UP and already set to true if DOWN
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
                        if (listVariableNames.Contains(variableName.ToUpper()))
                        {
                            intermediateList.AddRange(MapListAddition(variableName, expression.ToArray()));
                        }
                        else
                        {
                            intermediateList.AddRange(MapAddition(variableName, expression.ToArray()));
                        }
                        i += j + 2;
                        break;
                    case TokenType.REMOVE:
                        // Current Syntax:
                        // REMOVE x FROM variable
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j];
                        while (IsSameLine(nextToken, token) && !Is(nextToken, TokenType.FROM))
                        {
                            expression.Add(internalTokens[i + j]);
                            j++;
                            nextToken = internalTokens[i + j];
                        }
                        variableName = internalTokens[i + j + 1].GetLiteral();
                        if (listVariableNames.Contains(variableName.ToUpper()))
                        {
                            intermediateList.AddRange(MapListSubtraction(variableName, expression.ToArray()));
                        }
                        else
                        {
                            throw new Exception($"LOGIC ERROR on Line {nextToken.GetLine() + 1}: Remove operation can only be done on a list variable. {variableName} is not a list.");
                        }
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
                        if (listVariableNames.Contains(variableName.ToUpper()))
                        {
                            intermediateList.AddRange(MapListSubtraction(variableName, expression.ToArray()));
                        }
                        else
                        {
                            intermediateList.AddRange(MapSubtraction(variableName, expression.ToArray()));
                        }
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
                        // FUNCTION subroutine (a,...)
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
                        variableNames = new List<string>();
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
                        nextToken = internalTokens[i + j + 2];
                        bodyStart = i + j + 2;
                        bodyEnd = FindEndIndex(bodyStart, "FUNCTION", internalTokens);
                        subroutineDict.Add(subroutineName.ToUpper(), counterSubroutine);

                        List<Token> functionsTokens = new List<Token>();

                        for (int x = bodyStart; x < bodyEnd; x++)
                        {
                            functionsTokens.Add(internalTokens[x]);
                        }

                        intermediateSubroutines.Add(TokensToIntermediate(functionsTokens.ToArray(), true));

                        counterSubroutine++;

                        // Incorrect calculation here
                        i = bodyEnd + 2;
                        break;
                    case TokenType.PROCEDURE:
                        // Current Function Syntax
                        // PROCEDURE subroutine (a,...)
                        //   statements
                        // END FUNCTION
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken, TokenType.SUBROUTINE_NAME))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: No valid procedure name found after \"PROCEDURE\" keyword.");
                        }
                        subroutineName = nextToken.GetLiteral();
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken, TokenType.LEFT_BRACKET))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Missing \"(\" after {token.GetLiteral()}");
                        }
                        variableNames = new List<string>();
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
                        nextToken = internalTokens[i + j + 2];
                        bodyStart = i + j + 2;
                        bodyEnd = FindEndIndex(bodyStart, "PROCEDURE", internalTokens);
                        subroutineDict.Add(subroutineName.ToUpper(), counterSubroutine);

                        List<Token> procedureTokens = new List<Token>();

                        for (int x = bodyStart; x < bodyEnd; x++)
                        {
                            procedureTokens.Add(internalTokens[x]);
                        }

                        List<string> subroutineIntermediate = TokensToIntermediate(procedureTokens.ToArray(), true).ToList();
                        subroutineIntermediate.Add("RETURN");
                        intermediateSubroutines.Add(subroutineIntermediate.ToArray());

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
                        i += j + 1; // + 1
                        break;
                    case TokenType.SORT:
                        // Current Syntax:
                        // SORT list
                        nextToken = internalTokens[i + 1];
                        if (!IsVariable(nextToken))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: \"SORT\" not followed by a variable.");
                        }
                        variableName = nextToken.GetLiteral().ToUpper();
                        if (!listVariableNames.Contains(variableName))
                        {
                            throw new Exception($"LOGIC ERROR on Line {nextToken.GetLine() + 1}: {variableName} is not a list variable. Cannot sort a non-list variable.");
                        }

                        intermediateList.AddRange(MapSorting(variableName));
                        
                        i += 2;
                        break;
                    case TokenType.SWAP:
                        // Current Syntax:
                        // SWAP index1 WITH index2 IN var
                        j = 0;
                        nextToken = internalTokens[i + j + 1];
                        expression1 = new List<Token>();
                        do
                        {
                            expression1.Add(nextToken);
                            j++;
                            nextToken = internalTokens[i + j + 1];
                        }
                        while (!Is(nextToken, TokenType.WITH) && ValidLengthForIndexing(i + j + 1, internalTokens.Length));

                        nextToken = internalTokens[i + j + 1];
                        if (!Is(nextToken, TokenType.WITH))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: \"WITH\" not found after the first index.");
                        }

                        k = 0;
                        nextToken = internalTokens[i + j + k + 2];
                        expression2 = new List<Token>();
                        do
                        {
                            expression2.Add(nextToken);
                            k++;
                            nextToken = internalTokens[i + j + k + 2];
                        }
                        while (!Is(nextToken, TokenType.IN) && ValidLengthForIndexing(i + j + k + 2, internalTokens.Length));

                        nextToken = internalTokens[i + j + k + 2];
                        if (!Is(nextToken, TokenType.IN))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: \"IN\" not found after the second index.");
                        }

                        nextToken = internalTokens[i + j + k + 3];
                        if (!IsVariable(nextToken))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Second index not followed by a variable.");
                        }
                        variableName = nextToken.GetLiteral().ToUpper();
                        if (!listVariableNames.Contains(variableName))
                        {
                            throw new Exception($"LOGIC ERROR on Line {nextToken.GetLine() + 1}: {variableName} is not a list variable. Cannot sort a non-list variable.");
                        }

                        intermediateList.AddRange(MapSwapping(variableName, expression1, expression2));

                        i += j + k + 4;
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
            Variable var;
            Variable[] localVariables = variables;
            // Opcodes not involving an operand in the instruction
            if (operand == null)
            {
                object object2, object1, result;
                int variableIndex;
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
                    case "INDEX":
                        int index;
                        try
                        {
                            index = Convert.ToInt32(stack.Pop());
                        }
                        catch
                        {
                            throw new Exception("LOGIC ERROR: Index was not a number.");
                        }
                        variableIndex = Convert.ToInt32(stack.Pop());

                        var = localVariables[variableIndex];

                        stack.Push(var.GetValueFromIndex(index));
                        break;
                    case "SWAP":
                        int index1;
                        try
                        {
                            index1 = Convert.ToInt32(stack.Pop());
                        }
                        catch
                        {
                            throw new Exception("LOGIC ERROR: Index was not a number.");
                        }
                        int index2;
                        try
                        {
                            index2 = Convert.ToInt32(stack.Pop());
                        }
                        catch
                        {
                            throw new Exception("LOGIC ERROR: Index was not a number.");
                        }
                        variableIndex = Convert.ToInt32(stack.Pop());
                        var = localVariables[variableIndex];
                        object temp = var.GetValueFromIndex(index1);
                        var.SetValueFromIndex(index1, var.GetValueFromIndex(index2));
                        var.SetValueFromIndex(index2, temp);
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
                int index;
                string listStr;

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
                            index = subroutineDict[operand];
                            int parameterCount = subroutineParametersCount[subroutineDict[operand]];
                            // No values assigned to subroutineLocalVariableCounter ! ! !
                            int localVariablesCounter = subroutineLocalVariableCounter[subroutineDict[operand]];
                            // ! ! ! 
                            Variable[] parameters = new Variable[parameterCount];
                            // Find the number of local variables per subroutine
                            Variable[] local = new Variable[localVariablesCounter];
                            for (int i = 0; i < parameterCount; i++)
                            {
                                object parameterValue = stack.Pop();

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
                            if (localVariables[intOp].GetDataType() != DataType.LIST)
                            {
                                stack.Push(localVariables[intOp].GetValue());
                            }
                            // Adding a string formatted in the standard list output
                            // When encountering a var that is a list
                            else
                            {
                                List<object> listOfValues = localVariables[intOp].GetValuesList();
                                listStr = "[ ";
                                foreach (object v in listOfValues)
                                {
                                    listStr += v + ", ";
                                }
                                if (listOfValues.Count != 0)
                                {
                                    listStr = listStr.Substring(0, listStr.Length - 2);
                                }
                                listStr += " ]";
                                stack.Push(listStr);
                            }
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
                            var.MakeList();
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
                    case "REMOVE_LIST_ITEM":
                        intOp = Convert.ToInt32(operand);
                        var = localVariables[intOp];
                        if (var.IsDeclared())
                        {
                            var.Remove(stack.Pop());
                        }
                        break;
                    case "LENGTH":
                        intOp = Convert.ToInt32(operand);
                        var = localVariables[intOp];
                        if (var.IsDeclared())
                        {
                            int length = var.GetLength();
                            stack.Push(length);
                        }
                        break;
                    case "DECLARE_VAR":
                        intOp = Convert.ToInt32(operand);
                        localVariables[intOp].Declare();
                        localVariables[intOp].SetNull();
                        break;
                    case "SORT":
                        intOp = Convert.ToInt32(operand);
                        var = localVariables[intOp];
                        List<object> list = var.GetValuesList();

                        QuickSort(ref list, 0, list.Count - 1);

                        var.SetListValues(list);
                        break;
                    case "JUMP":
                        intOp = Convert.ToInt32(operand);
                        PC = GetLabelCounter(intOp, intermediateCode);
                        break;
                    case "JUMP_FALSE":
                        object value = stack.Pop();
                        try
                        {
                            bool toJump = Convert.ToBoolean(value);
                            if (!toJump)
                            {
                                intOp = Convert.ToInt32(operand);
                                PC = GetLabelCounter(intOp, intermediateCode);
                            }
                        }
                        catch
                        {
                            throw new Exception($"DEV ERROR: When attempting \"JUMP_FALSE\" stack was not prepped. Top of stack was not a boolean value.");
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

        #region Quick Sort Algorithm
        // Sort the items by value or ASCII value
        private void QuickSort(ref List<object> list, int low, int high)
        {
            if (low >= high || low < 0)
            {
                return;
            }

            int partition = Partition(ref list, low, high);

            QuickSort(ref list, low, partition - 1);
            QuickSort(ref list, partition + 1, high);
        }

        private int Partition(ref List<object> list, int low, int high)
        {
            object pivot = list[high];

            int i = low;

            for (int j = low; j < high; j++)
            {
                if (GetSortingValue(list[j]) <= GetSortingValue(pivot))
                {
                    Swap(ref list, i, j);
                    i++;
                }
            }

            Swap(ref list, i, high);
            return i;
        }

        private void Swap(ref List<object> list, int index1, int index2)
        {
            object temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        private double GetSortingValue(object obj)
        {
            DataType type = IdentifyDataType(obj);

            switch (type)
            {
                case DataType.INTEGER:
                    return (int)obj;
                case DataType.DECIMAL:
                    return (double)obj;
                case DataType.STRING:
                    return Convert.ToInt32(obj.ToString()[0]);
                case DataType.CHARACTER:
                    return Convert.ToInt32(obj.ToString()[0]);
                case DataType.BOOLEAN:
                    if ((bool)obj)
                    {
                        return 1;
                    }
                    return 0;
                default:
                    throw new Exception($"MATHS ERROR: Unknown data type for {obj}");
            }
        }

        #endregion

        private int GetLabelCounter(int labelNumber, string[] localIntermediateCode)
        {
            for (int i = 0; i < localIntermediateCode.Length; i++)
            {
                string line = localIntermediateCode[i];
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