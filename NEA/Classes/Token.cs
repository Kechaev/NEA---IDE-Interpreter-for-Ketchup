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
        public Token(TokenType type, string literal, int line)
        {
            this.type = type;
            this.literal = literal;
            this.line = line;
        }

        public TokenType GetTokenType()
        {
            return type;
        }

        public string GetLiteral()
        {
            return literal;
        }

        public int GetLine()
        {
            return line;
        }
    }
}
