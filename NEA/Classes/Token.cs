using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA.Classes
{
    class Token
    {
        private TokenType type;
        private string literal;
        private int line;
        
        // Creates a new token
        public Token(TokenType type, string literal, int line)
        {
            this.type = type;
            this.literal = literal;
            this.line = line;
        }

        // Returns the token type
        public TokenType GetTokenType()
        {
            return type;
        }

        // Returns the literal
        public string GetLiteral()
        {
            return literal;
        }

        // Returns the line
        public int GetLine()
        {
            return line;
        }
    }
}
