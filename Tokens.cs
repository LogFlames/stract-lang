using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stract_lang
{
    public class Token
    {
        public int codeIndexStart;
        public int codeIndexEnd;
        public TokenType tokenType;

        public string content;

        public Token(int codeIndexStart, int codeIndexEnd, TokenType tokenType, string content = "")
        {
            this.tokenType = tokenType;
            this.codeIndexStart = codeIndexStart;
            this.codeIndexEnd = codeIndexEnd;
            this.content = content;
        }
    }

    public enum TokenType
    {
        NotDefined,
        Identifier,
        String,
        ParenthesisStart,
        ParenthesisEnd,
        BracketStart,
        BracketEnd,
        Colon,
        Comma,
        Semicolon,
        Assign,
    }
}
