/////////////////////////////////////////////////////////////////////////
// Tokenizer.cs   -  Builds tokenizer                                                                                          
// Language: C#, Visual Studio 2018, .Net Framework 4.6.1           
// Platform: Macbook Pro , Win 10                
// Application: Pr#2 LexicalScanner                                     
// Author: Jim Fawcett, CST 2-187, Syracuse University            
//                Xiaoqian Huang (SU ID: 878174727) 
//Date: October 3rd, 2018    
/////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * Toker provides, via the class Tokenizer, the facilities to tokenize ASCII
 * text files.  That is, it composes the file's stream of characters into
 * words and punctuation symbols.
 * 
 * Tokenizer works with a private buffer of characters from an attached file.
 * When the buffer is emptied Tokenizer silently fills it again, so tokens
 * are always available until the end of file is reached.  End of file is
 * reported by tok = getTok() returning an empty token, e.g., tok == "".  
 * 
 * Public Interface
 * ================
 * Tokenizer toker = new Tokenizer();       // constructs Tokenizer object
 * if(toker.openFile(fileName)) ...   // attaches toker to specified file
 * if(toker.openString(str)) ...      // attaches toker to specified string
 * toker.close();                     // closes stream
 * setSpecialSingleChars(char);// set special single char
 * setSpecialCharPairs(string)// set special char pairs
 * printSpecialSingleChars // print the list of special single char
 * printSpecialCharPairs // print the list of special char pairs
 * string tok = toker.getTok();       // extracts next token from stream
 * string tok = toker.peekNextTok();  // peeks but does not extract
 * toker.pushBack(tok);               // puts token back on stream
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   Tokenizer.cs
 * 
 * Compiler Command:
 *    initiated by ScannerExec
 * 
 * Maintenance History
 * ===================
 * ver 3.0 : 03 Oct 18
 * - modifying codes to fit the requirements in project 2.
 * ver 2.9 : 12 Feb 18
 * - fixed bug in extractComment that caused failure to detect tokens
 *   after processing a line ending with a C++ style comment
 * ver 2.8 : 14 Oct 14
 * - fixed bug in extract that caused tokenizing of multiline string
 *   to loop endlessly
 * - reset lineCount in Attach function
 * ver 2.7 : 21 Sep 14
 * - made returning comments optional
 * - fixed handling of @"..." strings
 * ver 2.6 : 19 Sep 14
 * - stopped returning comments in getTok function
 * ver 2.5 : 14 Aug 14
 * - added patch to handle @"..." string format
 * ver 2.4 : 24 Sep 11
 * - added a thrown exception if extract encounters a string with the 
 *   substring "@.  This should be handled but raises two many changes
 *   to fix immediately.
 * ver 2.3 : 05 Sep 11
 * - fixed bug collecting C Comments in eatCComment()
 * - fixed bug where token contained embedded newline, now broken
 *   into seperate tokens
 * - fixed ackward display formatting
 * - replaced untyped ArrayList with generic List<string> 
 * - added lineCount property
 * ver 2.2 : 10 Jun 08
 * - added IsGrammarPunctuation to make tokenizer treat underscore
 *   as an ASCII char rather than a punctuator and used that in
 *   fillBuffer and eatASCII
 * ver 2.1 : 14 Jun 05
 * - fixed newline handling bug in buffer filling routines:
 *   readLine, getLine, fillbuffer
 * - fixed newline handling bug in extractComment
 * ver 2.0 : 30 May 05
 * - added extraction of comments and quotes as tokens
 * - added openString(...) to attach tokenizer to string
 * ver 1.1 : 21 Sep 04
 * - added toker.close() in test stub
 * - added processing for all command line args
 * ver 1.0 : 31 Aug 03
 * - first release
 * 
 * Planned Changes:
 * ----------------
 * - Handle quoted strings that use the @"\X" construct to allow omitting 
 *   double \\ when \ should be treated like a character, not the beginning
 *   of an escape sequence. 
 * - Improve performance by change lineRemainder from string to StringBuilder
 *   to avoid a lot of copies.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexical_Scanner
{
    namespace Tokenizer
    {
        using Token = StringBuilder;

        ///////////////////////////////////////////////////////////////////
        // ITokenSource interface
        // - Declares operations expected of any source of tokens
        // - Typically we would use either files or strings.  This demo
        //   provides a source only for Files, e.g., TokenFileSource, below.

        public interface ITokenSource
        {
            bool open(string path);
            void close();
            int next();
            int peek(int n = 0);
            bool end();
            int lineCount { get; set; }
        }

        ///////////////////////////////////////////////////////////////////
        // ITokenState interface
        // - Declares operations expected of any token gathering state

        public interface ITokenState
        {
            Token getTok();
            bool isDone();
        }

        ///////////////////////////////////////////////////////////////////
        // Toker class
        // - applications need to use only this class to collect tokens

        public class Toker
        {
            private TokenContext context_;       // holds single instance of all states and token source

            //----< initialize state machine >-------------------------------

            public Toker()
            {
                context_ = new TokenContext();      // context is the glue that holds all of the state machine parts 
            }

            //----< attempt to open source of tokens >-----------------------
            /*
             * If src is successfully opened, it uses TokenState.nextState(context_)
             * to set the initial state, based on the source content.
             */
            public bool open(string path)
            {
                TokenSourceFile src = new TokenSourceFile(context_);
                context_.src = src;
                return src.open(path);
            }

            //----< close source of tokens >---------------------------------

            public void close()
            {
                context_.src.close();
            }

            //----< provide methods to set special rules >---------------------------------
            public bool setSpecialCharPairs(string scp)
            {
                context_.setSpecialCharPairs(scp);
                return true;
            }
            public bool setSpecialSingleChars(char ssc)
            {
                context_.setSpecialSingleChars(ssc);
                return true;
            }

            //----< print the rule of special character tokens >---------------------------------

            public bool printSpecialCharPairs()
            {
                context_.printSpecialCharPairs();
                return true;
            }
            public bool printSpecialSingleChars()
            {
                context_.printSpecialSingleChars();
                return true;
            }

            //----< extract a token from source >----------------------------

            private bool isWhiteSpaceToken(Token tok)
            {
                return (tok.Length > 0 && Char.IsWhiteSpace(tok[0]));
            }

            public Token getTok()
            {
                Token tok = null;
                while (!isDone())
                {
                    tok = context_.currentState_.getTok();
                    context_.currentState_ = TokenState.nextState(context_);
                    if (!isWhiteSpaceToken(tok))
                        break;
                }
                return tok;
            }

            //----< has Toker reached end of its source? >-------------------

            public bool isDone()
            {
                if (context_.currentState_ == null)
                    return true;
                return context_.currentState_.isDone();
            }
            public int lineCount() { return (int)context_.src.lineCount; }
        }

        ///////////////////////////////////////////////////////////////////
        // TokenContext class
        // - holds all the tokenizer states
        // - holds source of tokens
        // - internal qualification limits access to this assembly

        public class TokenContext
        {
            internal TokenContext()
            {
                ws_ = new WhiteSpaceState(this);
                ps_ = new PunctState(this);
                as_ = new AlphaState(this);
                s1_s_ = new SpecialSingleCharState(this);
                s2_s_ = new SpecialCharPairState(this);
                cs_s_ = new CommentSingleState(this);
                cm_s_ = new CommentMultiState(this);
                str_s_ = new QuotedStrState(this);
                currentState_ = ws_;
            }
            internal WhiteSpaceState ws_ { get; set; }
            internal PunctState ps_ { get; set; }
            internal AlphaState as_ { get; set; }
            internal SpecialSingleCharState s1_s_ { get; set; }
            internal SpecialCharPairState s2_s_ { get; set; }
            internal CommentSingleState cs_s_ { get; set; }
            internal CommentMultiState cm_s_ { get; set; }
            internal QuotedStrState str_s_ { get; set; }
            internal TokenState currentState_ { get; set; }
            internal ITokenSource src { get; set; }  // can hold any derived class

            //----< all the special character tokens are defined here >-------------------------------

            public string[] SpecialCharPairs = new string[]
            {
                "<<", ">>", "::", "++","--", "==", "+=", "-=", "*=", "/=", "&&", "||"
            };

            public char[] SpecialSingleChars = new char[]
            {
                '<', '>', '[', ']', '(', ')', '{', '}', ':', '=', '+', '-', '*'
             };

            //----< provide methods to change special token sets >-------------------------------

            public bool setSpecialCharPairs(string scp)
            {
                List<string> strList = new List<string>(SpecialCharPairs);
                strList.Add(scp);
                SpecialCharPairs = strList.ToArray();
                return true;
            }
            public bool setSpecialSingleChars(char ssc)
            {
                List<char> strList = new List<char>(SpecialSingleChars);
                strList.Add(ssc);
                SpecialSingleChars = strList.ToArray();
                return true;
            }

            //----< methods to print special token sets >-------------------------------

            public bool printSpecialCharPairs()
            {
                Console.Write("\nThe special char pairs are:\n{");
                for (int i = 0; i < SpecialCharPairs.Length; i++)
                {
                    Console.Write("\"{0}\" ", SpecialCharPairs[i]);
                }

                Console.Write("}");
                return true;
            }
            public bool printSpecialSingleChars()
            {
                Console.Write("\nThe special char pairs are:\n{");
                for (int i = 0; i < SpecialSingleChars.Length; i++)
                {
                    Console.Write("\'{0}\' ", SpecialSingleChars[i]);
                }
                Console.Write("}");
                return true;
            }
        }

        ///////////////////////////////////////////////////////////////////
        // TokenState class
        // - base for all the tokenizer states

        public abstract class TokenState : ITokenState
        {

            internal TokenContext context_ { get; set; }  // derived classes store context ref here

            //----< delegate source opening to context's src >---------------

            public bool open(string path)
            {
                return context_.src.open(path);
            }

            //----< pass interface's requirement onto derived states >-------

            public abstract Token getTok();

            //----< derived states don't have to know about other states >---

            static public TokenState nextState(TokenContext context)
            {
                int nextItem = context.src.peek();//peek next bit
                if (nextItem < 0)
                    return null;
                char ch = (char)nextItem;
                int seconditem = context.src.peek(1);//peek the following second bit
                if (seconditem < 0)
                    return null;
                char ch2 = (char)seconditem;

                //Classify different states
                if (Char.IsWhiteSpace(ch))
                    return context.ws_;
                if (Char.IsLetterOrDigit(ch) || ch == '_' || ch == '@')//Some variables are named as "_abc" or "@abc"
                    return context.as_;
                if (Char.IsPunctuation(ch) || Char.IsSymbol(ch))
                {
                    if (ch == '\"' || ch == '\'')//quoted strings?
                        return context.str_s_;
                    if (Char.IsPunctuation(ch2) || Char.IsSymbol(ch2))
                    {
                        StringBuilder pairs = new StringBuilder();
                        pairs.Append(ch);
                        pairs.Append(ch2);
                        if (IsSpecialCharPair(pairs, context)) //special two character?
                            return context.s2_s_;
                        else if (pairs.ToString().Equals("//"))//single-line comment?
                            return context.cs_s_;
                        else if (pairs.ToString().Equals("/*"))//multi-line comment?
                            return context.cm_s_;
                        else
                        {
                            if (IsSpecialSingleChar(ch, context))
                                return context.s1_s_;//<>
                            else
                                return context.ps_;//the rest is punctuator
                        }
                    }
                    else
                    {
                        if (IsSpecialSingleChar(ch, context))
                            return context.s1_s_;
                        else
                            return context.ps_;//the rest is punctuator
                    }
                }
                return context.ws_;
            }

            //----< has tokenizer reached the end of its source? >-----------

            static public bool IsSpecialCharPair(StringBuilder pairs, TokenContext context)//if the string is special char pairs
            {
                foreach (string pair in context.SpecialCharPairs)
                {
                    if (pair.Equals(pairs.ToString()))
                        return true;
                }
                return false;
            }

            static public bool IsSpecialSingleChar(char ch, TokenContext context)//if the string is special char pairs
            {
                foreach (char c in context.SpecialSingleChars)
                {
                    if (c == ch)
                        return true;
                }
                return false;
            }
            public bool isDone()
            {
                if (context_.src == null)
                    return true;
                return context_.src.end();
            }
        }

        ///////////////////////////////////////////////////////////////////
        // Derived State Classes
        /* - WhiteSpaceState          Token with space, tab, and newline chars
         * - AlphaNumState            Token with letters and digits
         * - PunctuationState         Token holding anything not included above
         * ----------------------------------------------------------------
         * - Each state class accepts a reference to the context in its
         *   constructor and saves in its inherited context_ property.
         * - It is only required to provide a getTok() method which
         *   returns a token conforming to its state, e.g., whitespace, ...
         * - getTok() assumes that the TokenSource's first character 
         *   matches its type e.g., whitespace char, ...
         * - The nextState() method ensures that the condition, above, is
         *   satisfied.
         * - The getTok() method promises not to extract characters from
         *   the TokenSource that belong to another state.
         * - These requirements lead us to depend heavily on peeking into
         *   the TokenSource's content.
         */

        ///////////////////////////////////////////////////////////////////
        // WhiteSpaceState class
        // - extracts contiguous whitespace chars as a token
        // - will be thrown away by tokenizer

        public class WhiteSpaceState : TokenState //if char is white space
        {
            public WhiteSpaceState(TokenContext context)
            {
                context_ = context;
            }
            //----< manage converting extracted ints to chars >--------------

            bool isWhiteSpace(int i)
            {
                int nextItem = context_.src.peek();
                if (nextItem < 0)
                    return false;
                char ch = (char)nextItem;
                return Char.IsWhiteSpace(ch);
            }
            //----< keep extracting until get none-whitespace >--------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());     // first is WhiteSpace
                while (isWhiteSpace(context_.src.peek()))  // stop when non-WhiteSpace
                {
                    tok.Append((char)context_.src.next());
                }
                return tok;
            }
        }

        ///////////////////////////////////////////////////////////////////
        // PunctState class
        // - extracts contiguous punctuation chars as a token

        public class PunctState : TokenState // if char is normal punctuation
        {
            public PunctState(TokenContext context)
            {
                context_ = context;
            }

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next()); // return single char
                return tok;
            }
        }

        ///////////////////////////////////////////////////////////////////
        // AlphaState class
        // - extracts contiguous letter and digit chars as a token

        public class AlphaState : TokenState
        {
            public AlphaState(TokenContext context)
            {
                context_ = context;
            }
            //----< manage converting extracted ints to chars >--------------

            bool isLetterOrDigit(int i)
            {
                int nextItem = context_.src.peek();
                if (nextItem < 0)
                    return false;
                char ch = (char)nextItem;
                return Char.IsLetterOrDigit(ch);
            }
            //----< keep extracting until get none-alpha >-------------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());          // first is alpha
                while (isLetterOrDigit(context_.src.peek()) || (char)context_.src.peek() == '_')    // stop when non-alpha 
                {
                    tok.Append((char)context_.src.next());
                }
                return tok;
            }
        }

        public class SpecialSingleCharState : TokenState
        {
            public SpecialSingleCharState(TokenContext context)
            {
                context_ = context;
            }

            //----< keep extracting until get none-alpha >-------------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next()); // return single char
                return tok;
            }
        }

        public class SpecialCharPairState : TokenState
        {
            public SpecialCharPairState(TokenContext context)
            {
                context_ = context;
            }

            //----< keep extracting until get none-alpha >-------------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());
                tok.Append((char)context_.src.next());// return two char
                return tok;
            }
        }

        public class CommentSingleState : TokenState
        {
            public CommentSingleState(TokenContext context)
            {
                context_ = context;
            }

            //----< keep extracting until get none-alpha >-------------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());          // first is "/"
                bool endline = false;
                while (!endline)    // stop when the line end
                {
                    char ch = (char)context_.src.next();
                    tok.Append(ch);
                    if (ch == '\n')//return the entire line
                    {
                        endline = true;
                    }
                }
                return tok;
            }
        }

        public class QuotedStrState : TokenState
        {
            public QuotedStrState(TokenContext context)
            {
                context_ = context;
            }

            //----< keep extracting until get none-alpha >-------------------

            override public Token getTok()
            {
                Token tok = new Token();
                char quote = (char)context_.src.next();
                tok.Append(quote);          // first is \" or \'
                bool endquote = false;
                if (quote == '\'')
                {
                    int count = 0;
                    char pre_ch = '\'';
                    while (!endquote)    // stop when the quote complete
                    {
                        char cur_ch = (char)context_.src.next();
                        tok.Append(cur_ch);
                        if (cur_ch == '\\')
                            count++;
                        if (cur_ch == '\'')
                        {
                            if (pre_ch != '\\')
                                endquote = true;
                            if (pre_ch == '\\' && (count % 2 == 0)) //odd: '\'', even: '\\'
                                endquote = true;
                        }
                        pre_ch = cur_ch;
                    }
                    return tok;
                }
                else
                {
                    int count = 0;
                    char pre_ch = '\"';
                    while (!endquote)    // stop when the quote complete
                    {
                        char cur_ch = (char)context_.src.next();
                        tok.Append(cur_ch);
                        if (cur_ch == '\\')
                            count++;
                        if (cur_ch == '\"')
                        {
                            if (pre_ch != '\\')
                                endquote = true;
                            if (pre_ch == '\\' && (count % 2 == 0)) //odd: "\"", even: "\\"
                                endquote = true;
                        }
                        pre_ch = cur_ch;
                    }
                    return tok;
                }
            }
        }

        public class CommentMultiState : TokenState
        {
            public CommentMultiState(TokenContext context)
            {
                context_ = context;
            }
            //----< keep extracting until get none-alpha >-------------------

            override public Token getTok()
            {
                Token tok = new Token();
                tok.Append((char)context_.src.next());          // first is '/'
                bool endcomment = false;
                char pre_ch = '/';
                while (!endcomment)    // stop when comment is ended
                {
                    char cur_ch = (char)context_.src.next();
                    tok.Append(cur_ch);
                    if (cur_ch == '/' && pre_ch == '*')//end when "*/“ occurs
                    {
                        endcomment = true;
                    }
                    pre_ch = cur_ch;
                }
                return tok;
            }
        }

        ///////////////////////////////////////////////////////////////////
        // TokenSourceFile class
        // - extracts integers from token source
        // - Streams often use terminators that can't be represented by
        //   a character, so we collect all elements as ints
        // - keeps track of the line number where a token is found
        // - uses StreamReader which correctly handles byte order mark
        //   characters and alternate text encodings.

        public class TokenSourceFile : ITokenSource
        {
            public int lineCount { get; set; } = 1;
            private System.IO.StreamReader fs_;           // physical source of text
            private List<int> charQ_ = new List<int>();   // enqueing ints but using as chars
            private TokenContext context_;

            public TokenSourceFile(TokenContext context)//constructor
            {
                context_ = context;
            }

            //----< attempt to open file with a System.IO.StreamReader >-----

            public bool open(string path)
            {
                try
                {
                    fs_ = new System.IO.StreamReader(path, true);
                    context_.currentState_ = TokenState.nextState(context_);
                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}\n", ex.Message);
                    return false;
                }
                return true;
            }

            //----< close file >---------------------------------------------

            public void close()
            {
                fs_.Close();
            }
            //----< extract the next available integer >---------------------
            /*
             *  - checks to see if previously enqueued peeked ints are available
             *  - if not, reads from stream
             */
            public int next()
            {
                int ch;
                if (charQ_.Count == 0)  // no saved peeked ints
                {
                    if (end())
                        return -1;
                    ch = fs_.Read();
                }
                else                    // has saved peeked ints, so use the first 
                {
                    ch = charQ_[0];
                    charQ_.Remove(ch);//remove the last bit
                }
                if ((char)ch == '\n')   // track the number of newlines seen so far
                    ++lineCount;
                return ch;
            }

            //----< peek n ints into source without extracting them >--------
            /*
             *  - This is an organizing prinicple that makes tokenizing easier
             *  - We enqueue because file streams only allow peeking at the first int
             *    and even that isn't always reliable if an error occurred.
             *  - When we look for two punctuator tokens, like ==, !=, etc. we want
             *    to detect their presence without removing them from the stream.
             *    Doing that is a small part of your work on this project.
             */
            public int peek(int n = 0)//peek the n+1 bit of the context
            {
                if (n < charQ_.Count)
                {
                    return charQ_[n];
                }
                else                  // nth int not yet peeked
                {
                    for (int i = charQ_.Count; i <= n; ++i)
                    {
                        if (end())
                            return -1;
                        charQ_.Add(fs_.Read());  // read and enqueue
                    }
                    return charQ_[n];   // now return the last peeked
                }
            }
            //----< reached the end of the file stream? >--------------------

            public bool end()
            {
                return fs_.EndOfStream;
            }
        }

        //----< test stub >----------------------------------------------------------------------------------

#if (TEST_TOKER)
        class DemoToker
        {
            static bool testToker(string path)
            {
                Toker toker = new Toker();
                string fqf = System.IO.Path.GetFullPath(path);
                if (!toker.open(fqf))
                {
                    Console.Write("\n can't open {0}\n", fqf);
                    return false;
                }
                else
                {
                    Console.Write("\n  processing file: {0}", fqf);
                }
                while (!toker.isDone())
                {
                    Token tok = toker.getTok();
                    Console.Write("\n -- line#{0, 4} : {1}", toker.lineCount(), tok);
                }
                toker.close();
                return true;
            }

            private static void Main(string[] args)
            {

                Console.Write("\n  Testing Token State");
                Console.Write("\n ============================\n");

                //testToker("../../Test.txt");
                testToker("../../Tokenizer.cs");

                Console.ReadLine();
            }
        }
#endif
    }
}