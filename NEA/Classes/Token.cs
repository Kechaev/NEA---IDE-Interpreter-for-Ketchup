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

        public Token(TokenType type, string literal)
        {
            this.type = type;
            this.literal = literal;
        }

        public TokenType GetTokenType()
        {
            return type;
        }

        public string GetLiteral()
        {
            return literal;
        }
    }
}
