using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace stract_lang
{
    class StractLang
    {
        private static void Main(string[] args)
        {
            string sourceFilePath = "";
            if (args.Length < 1)
            {
                Console.Write("Enter path to source file: ");
                sourceFilePath = Console.ReadLine();
            } else
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
            code.Replace("\r\n", "\n");
            Tokenize(code);
        }

        private static void Tokenize(string code)
        {
            string[] specialWords = { ",", "(", ")", "{", "}", "=", ";", ":" };
            char stringDelimiter = '"';

            string beginLineComment = "//";
            string beginMultilineComment = "/*";
            string endMultilineComment = "*/";

            List<Token> tokens = new List<Token>();
            List<string> words = new List<string>();

            int i = 0;
            while (i < code.Length)
            {
                string word = "";

                while (i < code.Length)
                {
                    if (i + beginLineComment.Length <= code.Length && code.Substring(i, beginLineComment.Length) == beginLineComment)
                    {
                        while (i < code.Length && code[i] != '\n')
                        {
                            i++;
                        }
                    }
                    if (i + beginMultilineComment.Length <= code.Length && code.Substring(i, beginMultilineComment.Length) == beginMultilineComment)
                    {
                        while (i < code.Length && code.Substring(i, endMultilineComment.Length) != endMultilineComment)
                        {
                            i++;
                        }
                    }

                    if (code[i] == stringDelimiter)
                    {
                        if (word != "")
                        {
                            // TODO: parse error
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

                        words.Add(word);
                        tokens.Add(new StringToken(word.Substring(1, word.Length - 2)));
                        break;
                    }

                    if (char.IsWhiteSpace(code[i]))
                    {
                        if (word != "")
                        {
                            words.Add(word);
                        }
                        break;
                    }

                    bool foundSpecialWord = false;
                    foreach (string specialWord in specialWords)
                    {
                        if (i + specialWord.Length <= code.Length && code.Substring(i, specialWord.Length) == specialWord)
                        {
                            if (word != "")
                            {
                                words.Add(word);
                            }

                            words.Add(specialWord);
                            foundSpecialWord = true;
                            i += specialWord.Length - 1;
                            break;
                        }
                    }

                    if (foundSpecialWord)
                    {
                        break;
                    }

                    word += code[i];
                    i++;
                }

                i++;
            }

            foreach (string word in words)
            {
                Console.WriteLine(word);
            }
        }
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

    class Token
    {
        public int CodeIndex;
    }

    class IdentifierToken : Token
    {
        public string Name;
    }

    class AssignToken : Token { };
    class ScopeStartToken : Token { };
    class ScopeEndToken : Token { };
    class ParenthesisStartToken : Token { };
    class ParenthesisEndToken : Token { };

    class StringToken : Token 
    {
        public string Content;

        public StringToken(string content)
        {
            this.Content = content;
        }
    };
}
