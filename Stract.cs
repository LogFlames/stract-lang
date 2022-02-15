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
            Tokenize(code);
        }

        private static void Tokenize(string code)
        {
            Dictionary<string, TokenType> specialWords = new Dictionary<string, TokenType>(){
                { ",", TokenType.NotDefined },
                { "(", TokenType.ParenthesisStart },
                { ")", TokenType.ParenthesisEnd },
                { "{", TokenType.NotDefined },
                { "}", TokenType.NotDefined },
                { "=", TokenType.Assign},
                { ";", TokenType.NotDefined },
                { ":", TokenType.NotDefined },
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
                        while (i < code.Length && code.Substring(i, endMultilineComment.Length) != endMultilineComment)
                        {
                            i++;
                        }
                        break;
                    }

                    if (code[i] == stringDelimiter)
                    {
                        if (word != "")
                        {
                            Console.WriteLine("Syntax error: ");
                            Console.WriteLine("  - String cannot start directly after an identifier.");
                            PrintCodeLine(wordStart, i + 1, i, code, true, true);
                            return;
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

    class Token
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

    enum TokenType
    {
        NotDefined,
        Identifier,
        String,
        ParenthesisStart,
        ParenthesisEnd,
        Assign,
    }

    enum Operation {

    }

    struct Struct
    {
        // sturct template
        // hashmap: name -> values
    }

    struct StructTemplate
    {
        // hashmap with names and types
    }

    struct Scope
    {
        // list of "operations"
    }

    struct Function
    {
        // input parameters
        // scope
    }
}
