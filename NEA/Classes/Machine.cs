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
                                     "LEFT_BRACKET", "RIGHT_BRACKET", "ADD", "SUB", "MUL", "DIV", "MOD", "EXP", "THEN", "NEWLINE", "TABSPACE", "TIMES", "DIVIDED", "RAISE", "POWER",
                                     "INPUT", "PROMPT", "PRINT", "AND", "OR", "NOT", "BEGIN", "END", "RETURN", "EOF", "EON" /*/ End of nest /*/ };
        private int current, start, line, counter;

        // Fields for Translation into Intermediate Code
        private string[] intermediate;
        private List<string[]> intermediateSubroutines;
        private Dictionary<string, int> subroutineDict;
        private Variable[] variables;
        private Dictionary<string, int> variablesDict = new Dictionary<string, int>();
        private int counterVar;
        private int fixedLoopCounter;

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

            fixedLoopCounter = 0;
        }

        public string[] GetIntermediateCode()
        {
            return intermediate;
        }

        public void Interpret()
        {
            // Tokenization
            tokens = Tokenize();

            variables = new Variable[GetNoVariables() + GetNoUnnamedVariables()];

            OrganiseVariables();

            // Translation

            intermediate = TokensToIntermediate(tokens);

            // Execution is done from the form
            // To acommodate for outputs (& later inputs)
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

        private int GetNoUnnamedVariables()
        {
            int counter = 0;
            for (int i = 0; i < tokens.Length; i++) 
            {
                Token token = tokens[i];
                if (token.GetTokenType() == TokenType.TIMES && tokens[i + 1].GetTokenType() == TokenType.BEGIN)
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
                case "TIMES":
                    return TokenType.TIMES;
                case "DIVIDED":
                    return TokenType.DIVIDED;
                case "RAISE":
                    return TokenType.RAISE;
                case "POWER":
                    return TokenType.POWER;
                default:
                    validProgram = false;
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
                variables[i] = new Variable(KeyByValue(i), null);
            }
            for (int i = noNormalVariables; i < noNormalVariables + GetNoUnnamedVariables(); i++)
            {
                variablesDict.Add($"CounterVariable{i - noNormalVariables}", counter++);
                variables[i] = new Variable($"CounterVariable{i - noNormalVariables}", null);
            }
        }
        #endregion

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
            string[] multiCharKeywords = { "=", /*/ Temp /*/ "<>", ">", "<", ">=", "<=" };
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

            tokensList.Add(new Token(TokenType.EOF, null, line + 1));

            return tokensList.ToArray();
        }
        #endregion

        #region Translation into Intermdiate

        #region Utility for Translation
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
            TokenType[] binaryBitwiseOperations = { TokenType.AND, TokenType.OR };

            for (int i = 0; i < tokens.Count; i++)
            {
                Token e = tokens[i];
                if (literals.Contains(e.GetTokenType()))
                {
                    output.Add("LOAD_CONST " + e.GetLiteral());
                    if (stack.Count > 0 && stack.Peek().GetTokenType() == TokenType.NOT)
                    {
                        stack.Pop();
                        output.Add("NOT");
                    }
                }
                else if (e.GetTokenType() == TokenType.VARIABLE)
                {
                    output.Add("LOAD_VAR " + variablesDict[e.GetLiteral()]);
                    if (stack.Count > 0 && stack.Peek().GetTokenType() == TokenType.NOT)
                    {
                        stack.Pop();
                        output.Add("NOT");
                    }
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
                        // Removed this if statement wrapper
                        // No idea why it was there
                        //if (comparisonOperators.Contains(topToken.GetTokenType()) || mathematicalOperations.Contains(topToken.GetTokenType()))
                        output.Add(topToken.GetTokenType().ToString());
                    }
                    stack.Pop();
                }
                else if (IsBitwise(e))
                {
                    stack.Push(e);
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
                        else if (binaryBitwiseOperations.Contains(topToken.GetTokenType()))
                        {
                            if (topToken.GetTokenType() == TokenType.AND)
                            {
                                output.Add("MUL");
                            }
                            if (topToken.GetTokenType() == TokenType.OR)
                            {
                                output.Add("ADD");
                            }
                        }
                        else if (IsUnaryBitwise(topToken))
                        {
                            output.Add("NOT");
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
                else if (binaryBitwiseOperations.Contains(topToken.GetTokenType()))
                {
                    if (topToken.GetTokenType() == TokenType.AND)
                    {
                        output.Add("MUL");
                    }
                    if (topToken.GetTokenType() == TokenType.OR)
                    {
                        output.Add("ADD");
                    }
                }
                else if (topToken.GetTokenType() == TokenType.NOT)
                {
                    output.Add("NOT");
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
                case TokenType.NOT:
                    return 5;
            }
            return -1;
        }

        private bool ContainsExpressions(List<Token> expression)
        {
            TokenType[] mathematicalOperations = { TokenType.ADD, TokenType.SUB, TokenType.MUL,
                                                   TokenType.DIV, TokenType.MOD, TokenType.EXP };
            TokenType[] comparisonOperation = { TokenType.GREATER, TokenType.LESS, TokenType.EQUAL,
                                                TokenType.GREATER_EQUAL, TokenType.LESS_EQUAL, TokenType.NOT_EQUAL };
            TokenType[] bitwiseOperations = { TokenType.AND, TokenType.OR, TokenType.NOT };

            List<TokenType> tokenTypeExpression = new List<TokenType>();

            foreach (Token e in expression)
            {
                tokenTypeExpression.Add(e.GetTokenType());
            }

            // Does the print statement have an expression, mathematical or bitwise
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
                                i--;
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

                //string String = "";

                //foreach (Token t in expression)
                //{
                //    String += $"{t.GetLiteral()}\n";
                //}

                //MessageBox.Show($"{String}");

                // Does not register last token in expression?
                for (int counter = 0; counter < begins.Count; counter++)
                {
                    expressionForRPN = new List<Token>();
                    for (int i = 0; i < begins[counter] && counter == 0; i++)
                    {
                        instrLine = new List<string>();
                        Token e = expression[i];

                        //MessageBox.Show($"BEGIN\ne = {e.GetTokenType()}");
                        instrLine.AddRange(GetInstructions(e, ref i, expression));
                        instructions.AddRange(instrLine);
                    }
                    // Changed to <=
                    // ???
                    for (int i = begins[counter]; i <= ends[counter]; i++)
                    {
                        Token e = expression[i];

                        //MessageBox.Show($"END\ne = {e.GetTokenType()}");

                        expressionForRPN.Add(e);
                    }
                    instructions.AddRange(ConvertToPostfix(expressionForRPN));

                    //String = "";

                    //foreach (string t in ConvertToPostfix(expressionForRPN))
                    //{
                    //    String += $"{t}\n";
                    //}

                    //MessageBox.Show($"Postfix:\n{String}");
                    // i < expression.Count 
                    // Ensures that the last token is added in the case that it is by itself.
                    // counter < begins.Count - 1
                    // Ensures i < begins[counter + 1] does not crash for being out of bound.
                    // i < begins[counter + 1]
                    // Ensures the tokens after the expression go up until the beginning of the next expression.
                    for (int i = ends[counter]; i < expression.Count || counter < begins.Count - 1 && i < begins[counter + 1]; i++)
                    {
                        instrLine = new List<string>();
                        Token e = expression[i];

                        MessageBox.Show($"BOTH\ne = {e.GetTokenType()}");

                        instrLine.AddRange(GetInstructions(e, ref i, expression));
                        instructions.AddRange(instrLine);
                    }
                }

                instrLine = new List<string>();
                for (int i = 0; i < nonExpressionTotalMembers + numberOfExpressions - 1; i++)
                {
                    instrLine = new List<string>();
                    instrLine.Add("ADD");

                    //MessageBox.Show($"ADD");
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

            return instructions.ToArray();
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

                inputPrompt.Add(expression[i]);

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
                //MessageBox.Show($"Pre-crash: {e.GetTokenType()}");
                throw new Exception($"SYNTAX ERROR on Line {e.GetLine() + 1}: Invalid keyword in the expression.");
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
                    throw new Exception($"SYNTAX ERROR on Line {e.GetLine() + 1}: Invalid token in string.");
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

        #region Assignment
        private string[] MapAssignment(string variable, List<Token> expression, string type)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            instrLine = "DECLARE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            instructions.AddRange(GetIntermediateFromExpression(expression));

            instrLine = "STORE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "ADJUST_TYPE " + type;
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] MapReassignment(string variable, List<Token> expression, string type)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            instructions.AddRange(GetIntermediateFromExpression(expression));

            //string String = "Expression: ";

            //foreach (Token t in expression)
            //{
            //    String += $"{t.GetLiteral()}";
            //}

            //String += "\n\n";

            //foreach (string s in GetIntermediateFromExpression(expression))
            //{
            //    String += s + "\n";
            //}

            //MessageBox.Show($"{String}");

            instrLine = "STORE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            if (type != null)
            {
                instrLine = "ADJUST_TYPE " + type;
                instructions.Add(instrLine);
            }

            return instructions.ToArray();
        }

        private string[] MapDeclaration(string variable, string type)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            instrLine = "DECLARE_VAR " + counterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "ADJUST_TYPE " + type;
            instructions.Add(instrLine);

            return instructions.ToArray();
        }
        #endregion

        #region If Statement
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
        #endregion

        #region Loops
        private string[] MapForLoop(string variable, Token[] startExpression, Token[] endExpression, Token[] stepExpression, Token[] body)
        {
            List<string> instructions = new List<string>();
            string instrLine;
            counterVar = variablesDict[variable];

            counter += 2;

            int localCounter = counter;
            int localCounterVar = counterVar;

            instructions.AddRange(ConvertToPostfix(startExpression.ToList()));

            instrLine = "DECLARE_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "STORE_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instrLine = "LABEL " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instrLine = "LOAD_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instructions.AddRange(ConvertToPostfix(endExpression.ToList()));

            instrLine = "LESS_EQUAL";
            instructions.Add(instrLine);

            instrLine = "JUMP_FALSE " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            string[] statement = TokensToIntermediate(body);

            instructions.AddRange(statement);

            instrLine = "LOAD_VAR " + localCounterVar.ToString();
            instructions.Add(instrLine);

            instructions.AddRange(ConvertToPostfix(stepExpression.ToList()));

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

        private string[] MapWhileLoop(Token[] expression, Token[] body)
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

            instructions.AddRange(TokensToIntermediate(body));

            instrLine = "JUMP " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instrLine = "LABEL " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] MapDoWhileLoop(Token[] expression, Token[] body)
        {
            List<string> instructions = new List<string>();
            string instrLine;

            counter += 2;

            int localCounter = counter;

            instrLine = "LABEL " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instructions.AddRange(TokensToIntermediate(body));

            instructions.AddRange(ConvertToPostfix(expression.ToList()));

            instrLine = "JUMP_FALSE " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            instrLine = "JUMP " + (localCounter - 2).ToString();
            instructions.Add(instrLine);

            instrLine = "LABEL " + (localCounter - 1).ToString();
            instructions.Add(instrLine);

            return instructions.ToArray();
        }

        private string[] MapFixedLengthLoop(string variable, Token[] expression,Token[] body)
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

            instructions.AddRange(TokensToIntermediate(body));

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
        private int FindRelevantEndIndex(int index, Token[] tokens)
        {
            int nestCounter = 1;

            while (nestCounter > 0 && index < tokens.Length - 1)
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

        private bool IsEndOfToken(Token token)
        {
            return token.GetTokenType() == TokenType.EOF || token.GetTokenType() == TokenType.EON;
        }

        private bool IsVariable(Token token)
        {
            return token.GetTokenType() == TokenType.VARIABLE;
        }

        private bool IsInput(Token token)
        {
            return token.GetTokenType() == TokenType.INPUT;
        }

        private bool IsLiteral(Token token)
        {
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
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

        private bool IsUnaryBitwise(Token token)
        {
            return token.GetTokenType() == TokenType.NOT;
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
        // PRINT
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
        // EON (End of Nest)
        // EOF (End of File)
        private string[] TokensToIntermediate(Token[] internalTokens)
        {
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

            string type, variableName;
            bool noType;
            List<Token> expression;
            int j, k, l;
            int inputOffset;
            Token nextToken;
            List<Token> body = new List<Token>();
            Token bodyStartToken, bodyEndToken;
            int currentLine;
            Token finalTokenOfLine;


            while (i < internalTokens.Length)
            {
                Token token = internalTokens[i];
                switch (token.GetTokenType())
                {
                    case TokenType.PRINT:
                        // Reject:
                        // - End of file
                        // Accept:
                        // - Variable
                        // - Any literal
                        // - An input
                        // - Left Bracket
                        nextToken = internalTokens[i + 1];
                        if (!IsEndOfToken(nextToken) && IsVariable(nextToken) || IsLiteral(nextToken) || IsInput(nextToken) || IsLeftBracket(nextToken) || IsUnaryBitwise(nextToken))
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
                                // Limitation: in an if statement the prompt cannot contain multiple strings or variable
                                else if (IsInput(nextToken))
                                {
                                    expression.Add(nextToken);
                                    // Increment by 2 more to skip filler "WITH PROMPT"
                                    // Continue onto the following string prompt
                                    j += 3;
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
                        type = "STRING";
                        noType = true;

                        // Verify Valid Syntax
                        nextToken = internalTokens[i + 1];
                        if (!IsVariable(nextToken))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: No variable found after \"SET\".");
                        }
                        nextToken = internalTokens[i + 2];
                        if (nextToken.GetTokenType() != TokenType.TO)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: \"TO\" keyword not found after variable.");
                        }
                        // Get Variable Name & Expression
                        variableName = internalTokens[i + 1].GetLiteral();

                        if (PreviouslyDeclared(variableName, i, internalTokens))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: Variable {variableName} already created or set, use \"CHANGE\" to modify the value after.");
                        }

                        expression = new List<Token>();
                        j = 1;
                        inputOffset = 0;
                        nextToken = internalTokens[i + j + 2];
                        if (!IsSameLine(nextToken, token))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {token.GetLine() + 1}: Variable {variableName} not set to anything.");
                        }

                        while (!IsEndOfToken(nextToken) && internalTokens[i + j + 2].GetLine() == token.GetLine() && nextToken.GetTokenType() != TokenType.AS)
                        {
                            // Limitation: in an if statement the prompt cannot contain multiple strings or variables
                            // Format without punctuation does not support this
                            if (IsInput(nextToken))
                            {
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
                        // Check for type declaration
                        nextToken = internalTokens[i + j + 2];
                        if (!IsEndOfToken(nextToken) && Is(internalTokens[i + j + 3],TokenType.AS) && Is(internalTokens[i + j + 4],TokenType.DATA_TYPE))
                        {
                            type = internalTokens[i + j + 4].GetLiteral();
                            noType = false;
                        }
                        else if (!IsEndOfToken(internalTokens[i + j + 3]) && IsSameLine(internalTokens[i + j + 3],token))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: No data type mentioned after \"AS\" keyword.");
                        }

                        intermediateList.AddRange(MapAssignment(variableName, expression, type));
                        i += j + 3;
                        if (!noType)
                        {
                            i += 2;
                        }
                        break;
                    case TokenType.REASSIGNMENT:
                        type = null;
                        noType = true;

                        // Verify Valid Syntax
                        nextToken = internalTokens[i + 1];
                        if (!IsVariable(nextToken))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: When assigning no variable was found.");
                        }
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken,TokenType.TO))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: No value was mentioned for assignment.");
                        }
                        // Get Variable Name & Expression
                        variableName = internalTokens[i + 1].GetLiteral();


                        if (!PreviouslyDeclared(variableName, i, internalTokens) && DeclaredAfter(variableName, i, internalTokens))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: Variable {variableName} was not created before changing. Try using \"SET\" in the first mention of the variable.");
                        }
                        if (!PreviouslyDeclared(variableName, i, internalTokens))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: Variable {variableName} was not created, therefore cannot be changed. Try using \"SET\" instead.");
                        }

                        expression = new List<Token>();
                        j = 1;
                        inputOffset = 0;
                        nextToken = internalTokens[i + j + 2];
                        while (!IsEndOfToken(nextToken) && nextToken.GetLine() == token.GetLine() && nextToken.GetTokenType() != TokenType.AS)
                        {
                            // Limitation: in an if statement the prompt cannot contain multiple strings or variables
                            // Format without punctuation does not support this
                            if (IsInput(nextToken))
                            {
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
                        // Check for type declaration
                        nextToken = internalTokens[i + j + 2];
                        if (!IsEndOfToken(nextToken) && Is(internalTokens[i + j + 3],TokenType.AS) && Is(internalTokens[i + j + 4],TokenType.DATA_TYPE))
                        {
                            type = internalTokens[i + j + 4].GetLiteral();
                            noType = false;
                        }
                        else if (!IsEndOfToken(internalTokens[i + j + 3]) && IsSameLine(internalTokens[i + j + 3],token))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: No data type mentioned.");
                        }
                        if (noType)
                        {
                            intermediateList.AddRange(MapReassignment(variableName, expression, null));
                        }
                        else
                        {
                            intermediateList.AddRange(MapReassignment(variableName, expression, type));
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
                        if (!Is(finalTokenOfLine,TokenType.THEN))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {finalTokenOfLine.GetLine() + 1}: Missing \"THEN\".");
                        }
                        // Get Main If Expression
                        nextToken = internalTokens[i + j + 1];
                        while (IsSameLine(nextToken,token) && !Is(nextToken,TokenType.THEN))
                        {
                            mainExpression.Add(internalTokens[i + j + 1]);
                            j++;
                            nextToken = internalTokens[i + j + 1];
                        }
                        // Capture Main If Body
                        int bodyStart = i + j + 2;
                        int bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);

                        // Verify Valid Syntax - BEGIN & END
                        bodyStartToken = internalTokens[bodyStart];
                        bodyEndToken = internalTokens[bodyEnd];
                        if (!Is(bodyStartToken, TokenType.BEGIN) || !Is(bodyEndToken, TokenType.END))
                        {
                            if (!Is(bodyStartToken, TokenType.BEGIN) && !Is(bodyEndToken, TokenType.END))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {bodyStartToken.GetLine() + 1}: Missing \"BEGIN\"\r\nERROR following Line {bodyStartToken.GetLine() + 1}: Missing \"END\".");
                            }
                            if (!Is(bodyStartToken, TokenType.BEGIN))
                            {
                                throw new Exception($"SYNTAX ERROR on Line {bodyStartToken.GetLine() + 1}: Missing \"BEGIN\".");
                            }
                            if (!Is(bodyEndToken, TokenType.END))
                            {
                                throw new Exception($"SYNTAX ERROR following Line {bodyStartToken.GetLine() + 1}: Missing \"END\".");
                            }
                        }

                        mainBody = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                        // Set i to next section
                        i = bodyEnd + 1;

                        Token startToken = internalTokens[i];
                        // Identify if Else If statement(s)
                        while (!IsEndOfToken(startToken) && Is(startToken,TokenType.ELSE) && Is(internalTokens[i + 1],TokenType.IF))
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
                            while (IsSameLine(nextToken,startToken) && !Is(nextToken, TokenType.THEN))
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
                            bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                            // Verify Valid Syntax - BEGIN & END
                            bodyStartToken = internalTokens[bodyStart];
                            bodyEndToken = internalTokens[bodyEnd];
                            if (!Is(bodyStartToken, TokenType.BEGIN) || !Is(bodyEndToken, TokenType.END))
                            {
                                if (!Is(bodyStartToken, TokenType.BEGIN) && !Is(bodyEndToken, TokenType.END))
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {bodyStartToken.GetLine() + 1}: Missing \"BEGIN\"\r\nERROR following Line {bodyStartToken.GetLine() + 1}: Missing \"END\".");
                                }
                                if (!Is(bodyStartToken, TokenType.BEGIN))
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {bodyStartToken.GetLine() + 1}: Missing \"BEGIN\".");
                                }
                                if (!Is(bodyEndToken, TokenType.END))
                                {
                                    throw new Exception($"SYNTAX ERROR following Line {bodyStartToken.GetLine() + 1}: Missing \"END\".");
                                }
                            }

                            body = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                            // Add body & expresison to lists
                            elseIfBodies.Add(body.ToArray());
                            elseIfExpressions.Add(expression.ToArray());
                            // Set i to next section
                            i = bodyEnd + 1;
                            // Needed for preparing the next iteration of ELSE IF
                            startToken = internalTokens[i];
                        }
                        // Set up Else statement
                        bool isElse = false;
                        // Identify if Else statement is present
                        if (!IsEndOfToken(internalTokens[i]) && Is(internalTokens[i],TokenType.ELSE))
                        {
                            isElse = true;
                            bodyStart = i + 1;
                            bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);

                            // Verify Valid Syntax - BEGIN & END
                            bodyStartToken = internalTokens[bodyStart];
                            bodyEndToken = internalTokens[bodyEnd];
                            if (!Is(bodyStartToken, TokenType.BEGIN) || !Is(bodyEndToken, TokenType.END))
                            {
                                if (!Is(bodyStartToken, TokenType.BEGIN) && !Is(bodyEndToken, TokenType.END))
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {bodyStartToken.GetLine() + 1}: Missing \"BEGIN\"\r\nERROR following Line {bodyStartToken.GetLine() + 1}: Missing \"END\".");
                                }
                                if (!Is(bodyStartToken, TokenType.BEGIN))
                                {
                                    throw new Exception($"SYNTAX ERROR on Line {bodyStartToken.GetLine() + 1}: Missing \"BEGIN\".");
                                }
                                if (!Is(bodyEndToken, TokenType.END))
                                {
                                    throw new Exception($"SYNTAX ERROR following Line {bodyStartToken.GetLine() + 1}: Missing \"END\".");
                                }
                            }

                            elseBody = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                            i = bodyEnd + 1;
                        }
                        intermediateList.AddRange(MapIfStatement(mainExpression.ToArray(), mainBody.ToArray(), elseIfExpressions, elseIfBodies, isElse, elseBody.ToArray()));
                        break;
                    case TokenType.COUNT:
                        nextToken = internalTokens[i + 1];
                        if (!Is(nextToken,TokenType.WITH))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"WITH\" keyword.");
                        }
                        nextToken = internalTokens[i + 2];
                        if (!Is(nextToken,TokenType.VARIABLE))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing variable from \"COUNT WITH\".");
                        }
                        nextToken = internalTokens[i + 3];
                        if (!Is(nextToken,TokenType.FROM))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {nextToken.GetLine() + 1}: Missing \"FROM\" keyword.");
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
                        while (IsSameLine(internalTokens[i + j + k + 3],token) && !Is(internalTokens[i + j + k + 3],TokenType.BY))
                        {
                            expression2.Add(internalTokens[i + j + k + 3]);
                            k++;
                        }
                        l = 0;
                        List<Token> expression3 = new List<Token>();
                        if (!IsSameLine(internalTokens[i + j + k + 4],token))
                        {
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
                        bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                        body = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);

                        intermediateList.AddRange(MapForLoop(variableName, expression1.ToArray(), expression2.ToArray(), expression3.ToArray(), body.ToArray()));
                        i = bodyEnd + 1;
                        break;
                    case TokenType.WHILE:
                        // WHILE condition THEN
                        // BEGIN
                        // statements
                        // END
                        expression = new List<Token>();
                        j = 1;
                        nextToken = internalTokens[i + j];
                        while (IsSameLine(nextToken,token) && !Is(nextToken,TokenType.THEN))
                        {
                            expression.Add(internalTokens[i + j]);
                            j++;
                            nextToken = internalTokens[i + j];
                        }
                        if (!Is(nextToken,TokenType.THEN))
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + j - 1].GetLine() + 1}: Missing \"THEN\" in while loop.");
                        }
                        bodyStart = i + j + 1;
                        bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                        body = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                        intermediateList.AddRange(MapWhileLoop(expression.ToArray(), body.ToArray()));
                        i = bodyEnd + 1;
                        break;
                    case TokenType.LOOP:
                        // NOT FINAL SYNTAX

                        // LOOP
                        // BEGIN
                        // statement
                        // END
                        // REPEAT IF condition
                        if (internalTokens[i + 1].GetTokenType() != TokenType.BEGIN)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: No \"BEGIN\" keyword found after \"LOOP\".");
                        }
                        bodyStart = i + 1;
                        bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                        body = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                        i = bodyEnd + 1;
                        if (internalTokens[i - 1].GetTokenType() != TokenType.END)
                        {
                            // Throw error for no END 1 line ahead of the final token in the loop
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i - 2].GetLine() + 2}: No \"END\" keyword after the body.");
                        }
                        if (internalTokens[i].GetTokenType() != TokenType.REPEAT)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: No \"REPEAT\" keyword found after the body.");
                        }
                        if (internalTokens[i + 1].GetTokenType() != TokenType.IF)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: No \"IF\" keyword found after \"REPEAT\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        while (internalTokens[i + j + 1].GetTokenType() != TokenType.EOF && internalTokens[i + j + 1].GetLine() == internalTokens[i].GetLine())
                        {
                            expression.Add(internalTokens[i + j + 1]);
                            j++;
                        }
                        intermediateList.AddRange(MapDoWhileLoop(expression.ToArray(), body.ToArray()));
                        i = i + j + 1;
                        break;
                    case TokenType.REPEAT:
                        expression = new List<Token>();
                        j = 1;
                        while (internalTokens[i + j].GetLine() == token.GetLine() && internalTokens[i + j].GetTokenType() != TokenType.TIMES)
                        {
                            expression.Add(internalTokens[i + j]);
                            j++;
                        }
                        if (internalTokens[i + j].GetTokenType() != TokenType.TIMES)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + j].GetLine() + 1}: No \"TIMES\" keyword found after expression.");
                        }
                        if (internalTokens[i + j + 1].GetTokenType() != TokenType.BEGIN)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + j + 1].GetLine() + 1}: No \"BEGIN\" keyword found after \"TIMES\".");
                        }
                        bodyStart = i + j + 1;
                        bodyEnd = FindRelevantEndIndex(bodyStart, internalTokens);
                        body = internalTokensList.GetRange(bodyStart + 1, bodyEnd - bodyStart - 1);
                        if (internalTokens[bodyEnd].GetTokenType() != TokenType.END)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[bodyEnd].GetLine() + 1}: No \"END\" keyword found after body.");
                        }
                        variableName = $"CounterVariable{fixedLoopCounter++}";
                        intermediateList.AddRange(MapFixedLengthLoop(variableName, expression.ToArray(), body.ToArray()));
                        i = bodyEnd + 1;
                        break;
                    case TokenType.ADDITION:
                        expression = new List<Token>();
                        j = 1;
                        while (internalTokens[i + j].GetLine() == token.GetLine() && internalTokens[i + j].GetTokenType() != TokenType.TO)
                        {
                            expression.Add(internalTokens[i + j]);
                            j++;
                        }
                        variableName = internalTokens[i + j + 1].GetLiteral();
                        intermediateList.AddRange(MapAddition(variableName, expression.ToArray()));
                        i += j + 2;
                        break;
                    case TokenType.TAKE:
                        if (internalTokens[i + 1].GetTokenType() != TokenType.AWAY)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"AWAY\" keyword found after \"TAKE\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        while (internalTokens[i + j + 1].GetLine() == token.GetLine() && internalTokens[i + j + 1].GetTokenType() != TokenType.FROM)
                        {
                            expression.Add(internalTokens[i + j + 1]);
                            j++;
                        }
                        if (internalTokens[i + j + 1].GetTokenType() != TokenType.FROM)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + j].GetLine() + 1}: No \"FROM\" keyword after {expression[expression.Count - 1].GetLiteral()}.");
                        }
                        variableName = internalTokens[i + j + 2].GetLiteral();
                        intermediateList.AddRange(MapSubtraction(variableName, expression.ToArray()));
                        i += j + 3;
                        break;
                    case TokenType.MULTIPLICATION:
                        variableName = internalTokens[i + 1].GetLiteral();
                        if (internalTokens[i + 2].GetTokenType() != TokenType.BY)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"BY\" keyword after {variableName}.");
                        }
                        expression = new List<Token>();
                        j = 1;
                        while ((internalTokens[i + j + 2].GetTokenType() != TokenType.EOF || internalTokens[i + j + 2].GetTokenType() != TokenType.EON) && internalTokens[i + j + 2].GetLine() == token.GetLine())
                        {
                            expression.Add(internalTokens[i + j + 2]);
                            j++;
                        }
                        intermediateList.AddRange(MapMultiplication(variableName, expression.ToArray()));
                        i += j + 2;
                        break;
                    case TokenType.DIVISION:
                        variableName = internalTokens[i + 1].GetLiteral();
                        if (internalTokens[i + 2].GetTokenType() != TokenType.BY)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"BY\" keyword after {variableName}.");
                        }
                        expression = new List<Token>();
                        j = 1;
                        while ((internalTokens[i + j + 2].GetTokenType() != TokenType.EOF || internalTokens[i + j + 2].GetTokenType() != TokenType.EON) && internalTokens[i + j + 2].GetLine() == token.GetLine())
                        {
                            expression.Add(internalTokens[i + j + 2]);
                            j++;
                        }
                        intermediateList.AddRange(MapDivision(variableName, expression.ToArray()));
                        i += j + 2;
                        break;
                    case TokenType.GET:
                        // Allows for both of these syntaxes:
                        // GET THE REMAINDER OF ...
                        // GET REMAINDER OF...
                        int theOffset = 0;
                        if (internalTokens[i + 1].GetTokenType() != TokenType.THE)
                        {
                            theOffset = -1;
                        }
                        if (internalTokens[i + 2 + theOffset].GetTokenType() != TokenType.REMAINDER)
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
                        if (internalTokens[i + 3 + theOffset].GetTokenType() != TokenType.FROM)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 2 + theOffset].GetLine() + 1}: No \"OF\" keyword after \"REMAINDER\".");
                        }
                        variableName = internalTokens[i + 4 + theOffset].GetLiteral();
                        // Current syntax
                        // GET (THE) REMAINDER OF variable DIVDED BY expression
                        // GET (THE) REMAINDER FROM x DIVIDED BY expression
                        if (internalTokens[i + 5 + theOffset].GetTokenType() != TokenType.DIVIDED)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 4 + theOffset].GetLine() + 1}: No \"DIVIDED\" keyword after \"{variableName}\".");
                        }
                        if (internalTokens[i + 6 + theOffset].GetTokenType() != TokenType.BY)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 5 + theOffset].GetLine() + 1}: No \"BY\" keyword after \"DIVIDED\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        while ((internalTokens[i + j + 6 + theOffset].GetTokenType() != TokenType.EOF || internalTokens[i + j + 6 + theOffset].GetTokenType() != TokenType.EON) && internalTokens[i + j + 6 + theOffset].GetLine() == token.GetLine())
                        {
                            expression.Add(internalTokens[i + j + 6 + theOffset]);
                            j++;
                        }
                        intermediateList.AddRange(MapModulo(variableName, expression.ToArray()));
                        i += j + 6 + theOffset;
                        break;
                    case TokenType.RAISE:
                        if (internalTokens[i + 1].GetTokenType() != TokenType.VARIABLE)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i].GetLine() + 1}: No variable found after \"RAISE\".");
                        }
                        variableName = internalTokens[i + 1].GetLiteral();
                        if (internalTokens[i + 2].GetTokenType() != TokenType.TO)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 1].GetLine() + 1}: No \"TO\" after \"{variableName}\".");
                        }
                        if (internalTokens[i + 3].GetTokenType() != TokenType.THE)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 2].GetLine() + 1}: No \"THE\" keyword after \"TO\".");
                        }
                        if (internalTokens[i + 4].GetTokenType() != TokenType.POWER)
                        {
                            throw new Exception($"SYNTAX ERROR on Line {internalTokens[i + 3].GetLine() + 1}: No \"POWER\" keyword after \"THE\".");
                        }
                        expression = new List<Token>();
                        j = 1;
                        while ((internalTokens[i + j + 4].GetTokenType() != TokenType.EOF || internalTokens[i + j + 4].GetTokenType() != TokenType.EON) && internalTokens[i + j + 4].GetLine() == token.GetLine())
                        {
                            expression.Add(internalTokens[i + j + 4]);
                            j++;
                        }
                        intermediateList.AddRange(MapExponentiation(variableName, expression.ToArray()));
                        i += j + 4;
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
            //MessageBox.Show($"Line: {line}\nPC = {PC}");
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
                                throw new Exception($"LOGIC ERROR: Cannot apply NOT to {object1.ToString()} as acharacter.");
                            case DataType.STRING:
                                throw new Exception($"LOGIC ERROR: Cannot apply NOT to {object1.ToString()} as a string.");
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
                    case "HALT":
                        isRunning = false;
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
                        if (variables[intOp].IsDeclared() && !variables[intOp].IsNull())
                        {
                            stack.Push(variables[intOp].GetValue());
                        }
                        if (variables[intOp].IsDeclared() && variables[intOp].IsNull())
                        {
                            throw new Exception($"DEV ERROR in execution: Attempted to use a variable {variables[intOp].GetName()} with no assigned value.");
                        }
                        //MessageBox.Show($"Var {variables[intOp].GetName()} - {variables[intOp].GetDataType().ToString()}");
                        break;
                    case "STORE_VAR":
                        intOp = Convert.ToInt32(operand);
                        if (variables[intOp].IsDeclared())
                        {
                            variables[intOp].SetValue(stack.Pop());
                        }
                        break;
                    case "DECLARE_VAR":
                        intOp = Convert.ToInt32(operand);
                        variables[intOp].Declare();
                        variables[intOp].SetNull();
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
                        variables[variableIndex].SetDataType(type);
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