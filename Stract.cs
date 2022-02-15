using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace stract_lang
{
    class StractLang
    {
        private static void Main(string[] args)
        {
            string sourceFilePath = "";
            if (args.Length == 0)
            {
                /*
                 * Console.Write("Enter path to source file: ");
                 * sourceFilePath = Console.ReadLine(); */
                sourceFilePath = "../../../test.stract";
                if (sourceFilePath == "")
                {
                    Console.WriteLine("No path to source-file was given.");
                    return;
                }
            }
            else
            {
                sourceFilePath = args[0];
            }

            sourceFilePath = Path.GetFullPath(sourceFilePath);

            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine("Could not find source-file: " + sourceFilePath);
                return;
            }

            string code = File.ReadAllText(sourceFilePath);
            code = code.Replace("\r\n", "\n");
            code = code.Replace("\t", "    ");
            List<Token> tokens = Tokenize(code);

            if (tokens == null)
            {
                return;
            }

            int codeIndex = 0;
            Assimilate(0, ref codeIndex, tokens, code);
        }

        private static StractScope Assimilate(int openingBracketIndex, ref int i, List<Token> tokens, string code)
        {
            StractScope scope = new StractScope();
            scope.tokenIndexStart = openingBracketIndex;

            // TODO: I think this should be two functions, one that takes the next object and one that assimilates them, maybe parse for the next object
            // The parse function could supply to where it jumped, so that it can be recursive in a nice way

            while (i < tokens.Count && tokens[i].tokenType != TokenType.BracketEnd)
            {
                scope.codeObjects.Add(Parse(ref i, tokens, code));
                i++;
            }

            scope.tokenIndexEnd = i;

            return scope;
        }

        private static StractCodeObject Parse(ref int i, List<Token> tokens, string code)
        {
            if (i + 2 < tokens.Count &&
                tokens[i].tokenType == TokenType.ParenthesisStart &&
                tokens[i + 1].tokenType == TokenType.Identifier &&
                tokens[i + 2].tokenType == TokenType.Colon)
            {
                StractStructTemplate structureTemplate = new StractStructTemplate();
                structureTemplate.tokenIndexStart = i;

                while (tokens[i].tokenType != TokenType.ParenthesisEnd)
                {
                    i++;
                    ExpectTokenOfType(i, TokenType.Identifier, tokens, code);
                    string name = tokens[i].content;
                    StractType type = new StractType();
                    type.tokenIndexStart = i;
                    type.tokenIndexEnd = i;

                    i++;
                    ExpectTokenOfType(i, TokenType.Colon, tokens, code);

                    i++;
                    ExpectTokenOfType(i, TokenType.Identifier, tokens, code);

                    if (tokens[i].tokenType == TokenType.ParenthesisStart)
                    {
                        i++;
                        StractCodeObject codeObject = Parse(ref i, tokens, code);

                        // if codeObject is type => cast to type
                        type = (StractType)codeObject;
                    }
                    else
                    {
                        ExpectTokenOfType(i, TokenType.Identifier, tokens, code);
                        type = new StractPrimitiveType(tokens[i].content);
                    }

                    structureTemplate.namedValueTypes.Add(name, type);

                    i++;
                    ExpectOneOfTokenTypes(i, new TokenType[] { TokenType.Comma, TokenType.ParenthesisEnd }, tokens, code);
                }

                structureTemplate.tokenIndexEnd = i;
                i++;

                return structureTemplate;
            }
            else if (i + 1 < tokens.Count &&
                tokens[i].tokenType == TokenType.Identifier &&
                tokens[i + 1].tokenType == TokenType.Assign)
            {
                string name = tokens[i].content;
                i += 2;
                StractCodeObject toAssign = Parse(ref i, tokens, code);
                return new StractAssignment(name, toAssign);
            }
            else if (i < tokens.Count &&
                tokens[i].tokenType == TokenType.BracketStart)
            {
                i++;
                StractScope scope = Assimilate(i - 1, ref i, tokens, code);

                return scope;
            }

            Console.WriteLine("Could not parse any code objects from this token.");
            PrintCodeLine(tokens[i].codeIndexStart, tokens[i].codeIndexEnd, -1, code, true, true);
            throw new Exception("Unable to parse code.");
        }

        private static List<Token> Tokenize(string code)
        {
            Dictionary<string, TokenType> specialWords = new Dictionary<string, TokenType>(){
                { ",", TokenType.Comma },
                { "(", TokenType.ParenthesisStart },
                { ")", TokenType.ParenthesisEnd },
                { "{", TokenType.BracketStart },
                { "}", TokenType.BracketEnd },
                { "=", TokenType.Assign},
                { ";", TokenType.Semicolon },
                { ":", TokenType.Colon },
            };

            char stringDelimiter = '"';

            string beginLineComment = "//";
            string beginMultilineComment = "/*";
            string endMultilineComment = "*/";

            List<Token> tokens = new List<Token>();

            int i = 0;
            while (i < code.Length)
            {
                string word = "";
                int wordStart = i;

                while (i < code.Length)
                {
                    if (i + beginLineComment.Length <= code.Length && code.Substring(i, beginLineComment.Length) == beginLineComment)
                    {
                        while (i < code.Length && code[i] != '\n')
                        {
                            i++;
                        }
                        break;
                    }
                    if (i + beginMultilineComment.Length <= code.Length && code.Substring(i, beginMultilineComment.Length) == beginMultilineComment)
                    {
                        i += beginLineComment.Length;
                        while (i < code.Length && code.Substring(i, endMultilineComment.Length) != endMultilineComment)
                        {
                            i++;
                        }
                        i += endMultilineComment.Length;
                        break;
                    }

                    if (code[i] == stringDelimiter)
                    {
                        if (word != "")
                        {
                            Console.WriteLine("Syntax error: ");
                            Console.WriteLine("  - String cannot start directly after an identifier.");
                            PrintCodeLine(wordStart, i + 1, i, code, true, true);
                            return null;
                        }

                        word += code[i];
                        i++;

                        while (i < code.Length)
                        {
                            word += code[i];

                            if (code[i] == stringDelimiter)
                            {
                                break;
                            }

                            i++;
                        }

                        tokens.Add(new Token(wordStart, i + 1, TokenType.String, word.Substring(1, word.Length - 2)));
                        break;
                    }

                    bool foundSpecialWord = false;
                    foreach (KeyValuePair<string, TokenType> specialWord in specialWords)
                    {
                        if (i + specialWord.Key.Length <= code.Length && code.Substring(i, specialWord.Key.Length) == specialWord.Key)
                        {
                            if (word != "")
                            {
                                tokens.Add(new Token(wordStart, i, TokenType.Identifier, word));
                                wordStart = i;
                            }

                            i += specialWord.Key.Length - 1;

                            tokens.Add(new Token(wordStart, i + 1, specialWord.Value, specialWord.Key));

                            foundSpecialWord = true;
                            break;
                        }
                    }

                    if (foundSpecialWord)
                    {
                        break;
                    }

                    if (char.IsWhiteSpace(code[i]))
                    {
                        if (word != "")
                        {
                            tokens.Add(new Token(wordStart, i, TokenType.Identifier, word));
                        }
                        break;
                    }

                    word += code[i];
                    i++;
                }

                i++;
            }

            foreach (Token token in tokens)
            {
                PrintCodeLine(token.codeIndexStart, token.codeIndexEnd, -1, code, true, true);
            }

            return tokens;
        }

        private static void ExpectTokenOfType(int tokenIndex, TokenType expectedType, List<Token> tokens, string code)
        {
            if (tokens[tokenIndex].tokenType != expectedType)
            {
                Console.WriteLine("Unexpected token. Expected: " + expectedType.ToString() + ", but got: " + tokens[tokenIndex].tokenType.ToString());
                PrintCodeLine(tokens[tokenIndex].codeIndexStart, tokens[tokenIndex].codeIndexEnd, -1, code, true, true);
                throw new Exception("Unexpected token encountered.");
            }
        }

        private static void ExpectOneOfTokenTypes(int tokenIndex, TokenType[] expectedTypes, List<Token> tokens, string code)
        {
            if (!expectedTypes.Contains(tokens[tokenIndex].tokenType))
            {
                string tokensString = "";
                for (int i = 0; i < expectedTypes.Length; i++)
                {
                    tokensString += expectedTypes[i].ToString();
                    if (i != expectedTypes.Length - 1)
                    {
                        tokensString += ", ";
                    }
                }
                Console.WriteLine("Unexpected token. Expected one of the following tokens: [ " + tokensString + " ], but got: " + tokens[tokenIndex].tokenType.ToString());
                PrintCodeLine(tokens[tokenIndex].codeIndexStart, tokens[tokenIndex].codeIndexEnd, -1, code, true, true);
                throw new Exception("Unexpected token encountered.");
            }
        }

        private static void PrintCodeLine(int highlightBegin, int highlightEnd, int markPoint, string code, bool underline, bool showLineNumber)
        {
            highlightBegin = Math.Clamp(highlightBegin, 0, code.Length);
            highlightEnd = Math.Clamp(highlightEnd, 0, code.Length);

            int begin = highlightBegin;
            int end = highlightEnd - 1;

            while (begin > 0 && code[begin - 1] != '\n')
            {
                begin--;
            }

            while (end < code.Length - 1 && code[end + 1] != '\n')
            {
                end++;
            }

            end++;

            int firstLine = 1;
            int lastLine = 1;
            string largestLineNumber = "";
            if (showLineNumber)
            {
                for (int i = 0; i < highlightBegin; i++)
                {
                    if (code[i] == '\n')
                    {
                        firstLine++;
                        lastLine++;
                    }
                }
                for (int i = highlightBegin; i < highlightEnd; i++)
                {
                    if (code[i] == '\n')
                    {
                        lastLine++;
                    }
                }

                largestLineNumber = "Line " + lastLine.ToString() + ": ";
            }

            string[] lines = code.Substring(begin, end - begin).Split('\n', StringSplitOptions.None);

            int prog = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string lineNumber = "Line " + (firstLine + i).ToString() + ": ";
                lineNumber += RepeatString(" ", largestLineNumber.Length - lineNumber.Length);
                Console.WriteLine(lineNumber + lines[i]);

                if (underline)
                {
                    string highlight = RepeatString(" ", lineNumber.Length);
                    if (i == 0)
                    {
                        highlight += RepeatString(" ", highlightBegin - begin);
                    }
                    highlight += RepeatString("~", 
                        lines[i].Length - 
                        (i == 0 ? (highlightBegin - begin) : 0) -
                        (i == lines.Length - 1 ? (end - highlightEnd) : 0));

                    if (i == lines.Length - 1)
                    {
                        highlight += RepeatString(" ", end - highlightEnd);
                    }

                    if (markPoint < highlightEnd && markPoint >= highlightBegin)
                    {
                        if ((markPoint - begin) - prog >= 0 && (markPoint - begin) - prog < lines[i].Length + 1)
                        {
                            char[] highlightChars = (highlight + " ").ToCharArray();
                            highlightChars[lineNumber.Length + markPoint - begin - prog] = '^';
                            highlight = new string(highlightChars);
                        }
                    }

                    prog += lines[i].Length + 1;

                    Console.WriteLine(highlight);
                }
            }

        }

        public static string RepeatString(string toRepeat, int count)
        {
            return string.Concat(Enumerable.Repeat(toRepeat, count));
        }
    }

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

    public class StractCodeObject
    {
        public int tokenIndexStart;
        public int tokenIndexEnd;
    }

    public class StractAssignment : StractCodeObject
    {
        public string identifier;
        public StractCodeObject codeObject;

        public StractAssignment(string identifier, StractCodeObject codeObject)
        {
            this.identifier = identifier;
            this.codeObject = codeObject;
        }
    }

    public class StractType : StractCodeObject
    {
    }

    public class StractStructTemplate : StractType
    {
        public Dictionary<string, StractType> namedValueTypes;

        public StractStructTemplate()
        {
            namedValueTypes = new Dictionary<string, StractType>();
        }
    }


    public class StractPrimitiveType : StractType
    {
        public string name;

        public StractPrimitiveType(string name)
        {
            this.name = name;
        }
    }

    public class StractScope : StractCodeObject
    {
        public List<StractCodeObject> codeObjects;

        public StractScope()
        {
            codeObjects = new List<StractCodeObject>();
        }
    }

    public class StractFunction : StractCodeObject
    {

    }
}
