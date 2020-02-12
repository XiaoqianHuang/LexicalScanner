/////////////////////////////////////////////////////////////////////////
// SemiExpression.cs   -  Builds semiExpressions                                                                                          
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
 * Semi provides, via class SemiExpression, facilities to extract semiExpressions.
 * A semiExpression is a sequence of tokens that is just the right amount
 * of information to parse for code analysis.  SemiExpressions are token
 * sequences that end in "{" or "}" or ";"
 * 
 * SemiExpression works with a private Toker object attached to a specified file.
 * It provides a get() function that extracts semiExpressions from the file
 * while filtering out comments and merging quotes into single tokens.
 * 
 * SemiExpression implements the interface ITokenCollection.
 * 
 * Public Interface
 * ================
 * SemiExpression semi = new SemiExpression;();    // constructs SemiExpression object
 * if(semi.open(fileName)) ...        // attaches semi to specified file
 * semi.close();                      // closes file stream
 * setSpecialSingleChars(char);// set special single char
 * setSpecialCharPairs(string)// set special char pairs
 * printSpecialSingleChars // print the list of special single char
 * printSpecialCharPairs // print the list of special char pairs
 * if(semi.Equals(se)) ...            // do these semiExps have same tokens?
 * int hc = semi.GetHashCode()        // returns hashcode
 * if(getSemi()) ...                  // extracts and stores next semiExp
 * int len = semi.count;              // length property
 * semi.verbose = true;               // verbose property - shows tokens
 * string tok = semi[2];              // access a semi token
 * string tok = semi[1];              // extract token
 * semi.flush();                      // removes all tokens
 * semi.initialize();                 // adds ";" to empty semi-expression
 * semi.insert(2,tok);                // inserts token as third element
 * semi.Add(tok);                     // appends token
 * semi.Add(tokArray);                // appends array of tokens
 * semi.display();                    // sends tokens to Console
 * string show = semi.displayStr();   // returns tokens as single string
 * semi.returnNewLines = false;       // property defines newline handling
 *                                    //   default is true
 */
//
/*
 * Build Process
 * =============
 * Required Files:
 *   SemiExpression.cs Tokenizer.cs ITokCollection.cs
 * 
 * Compiler Command:
 *   initiated by ScannerExec
 * 
 * Maintenance History
 * ===================
 * ver 3.0 : 03 Oct 18
 * - modifying codes to fit the requirements in project 2.
 * ver 2.3 : 07 Aug 18
 * - added functions isWhiteSpace and trim
 *   That eliminates whitespace tokens in SemiExp, making them easier to use.
 * ver 2.2 : 14 Aug 14
 * - added folding rule for "for(int i=0; i<count; ++i)" type statements
 * ver 2.1 : 24 Sep 11
 * - collect line starting with # and ending with \n as semiExpression.
 * ver 2.0 : 05 Sep 11
 * - Converted to new C# property syntax
 * - Converted from untyped ArrayList to generic List<string>
 * - Simplified display() and displayStr()
 * - Added new tests in test stub
 * ver 1.9 : 27 Sep 08
 * - Changed comments on manual page to say that semi.ReturnNewLines is true by default
 * ver 1.8 : 10 Jun 08
 * - Aniruddha Gore added Contains function and set returnNewLines as the default
 * ver 1.7 : 17 Jun 06
 * - added displayNewLines property
 * ver 1.6 : 16 Jun 06
 * - added CSemi member functions copy(), remove(int i), and remove(string tok).
 * ver 1.5 : 12 Jun 05
 * - added returnNewLines property
 * - modified way get() behaves so that it will not hang on files that
 *   end with text that have no semiExp terminator.
 * ver 1.4 : 30 May 05
 * - removed CppCommentFilter, CCommentFilter, SQuoteFilter, DQuoteFilter
 *   since Toker now returns comments and quotes as tokens.
 * - added isComment(string tok) member function
 * ver 1.3 : 16 Sep 03
 * - removed insert(tokenArray), added Add(tokenArray)
 *   Since this is a change to public interface it may break some code.
 *   It simply changes the name of the function to more directly 
 *   describe what it does - append a token array.
 * - added overrides of Equals(object) and GetHashCode()
 * - completed Manual Page description of public interface
 * ver 1.2 : 14 Sep 03
 * - cosmetic changes to comments
 * - Added formatting of extracted comments (see notes in code below)
 * ver 1.1 : 13 Sep 03
 * - fixed bug in CppCommentFilter() that caused collection to terminate
 *   if a C++ comment was on same line as a semiExpression.
 * - added calls to semiExp.Add(currTok) in SQuoteFilter() and DQuoteFilter()
 *   which simplified getSemi().
 * - added some functions to create and manipulate semi-expressions.
 * ver 1.0 : 31 Aug 03
 * - first release
 * 
 * Planned Modifications:
 * ----------------------
 * - return, or don't return, comments based on discardComments property
 *   which is now present but inactive.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexical_Scanner
{
    namespace SemiExpression
    {

        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Text;
        using Tokenizer;

        ///////////////////////////////////////////////////////////////////////
        // class CSemiExp - filters token stream and collects semiExpressions
        public class SemiExpression : ITokCollection
        {
            Toker toker = null;
            List<string> semiExp = null; // this is a trailing comment
            string currTok = "";
            string prevTok = "";
            int linecount = 0;

            //----< methods to handle special character set >----------------------------------------
            public bool setSpecialSingleChars(char ssc)
            {
                toker.setSpecialSingleChars(ssc);
                return true;
            }

            public bool setSpecialCharPairs(string scp)
            {
                toker.setSpecialCharPairs(scp);
                return true;
            }

            public bool printSpecialSingleChars()
            {
                toker.printSpecialSingleChars();
                return true;
            }
            public bool printSpecialCharPairs()
            {
                toker.printSpecialCharPairs();
                return true;
            }

            //----< line count property >----------------------------------------
            public int lineCount
            {
                get { return toker.lineCount(); }
            }

            //----< constructor >------------------------------------------------

            public SemiExpression()
            {
                toker = new Toker();
                semiExp = new List<string>();
                discardComments = true;  // not implemented yet
                returnNewLines = true;
                displayNewLines = false;
            }

            //----< test for equality >------------------------------------------

            override public bool Equals(Object semi)
            {
                SemiExpression temp = (SemiExpression)semi;
                if (temp.count != this.count)
                    return false;
                for (int i = 0; i < temp.count && i < this.count; ++i)
                    if (this[i] != temp[i])
                        return false;
                return true;
            }

            //---< pos of first str in semi-expression if found, -1 otherwise >--

            public int FindFirst(string str)
            {
                for (int i = 0; i < count - 1; ++i)
                    if (this[i] == str)
                        return i;
                return -1;
            }

            //---< pos of last str in semi-expression if found, -1 otherwise >--- 

            public int FindLast(string str)
            {
                for (int i = this.count - 1; i >= 0; --i)
                    if (this[i] == str)
                        return i;
                return -1;
            }

            //----< deprecated: here to avoid breakage with old code >----------- 

            public int Contains(string str)
            {
                return FindLast(str);
            }

            //----< have to override GetHashCode() >-----------------------------

            override public System.Int32 GetHashCode()
            {
                return base.GetHashCode();
            }

            //----< opens member tokenizer with specified file >-----------------

            public bool open(string fileName)
            {
                return toker.open(fileName);//Tokenizer.Toker.open()
            }

            //----< close file stream >------------------------------------------

            public void close()
            {
                toker.close();
            }

            //----< is this is the last token? >------

            bool isTerminated(string tok)
            {
                if (tok == null || tok == "")
                {
                    return true;
                }
                else
                    return false;
            }
            //----< get next token, saving previous token >----------------------

            public string get()
            {
                try
                {
                    prevTok = currTok;
                    if (!toker.isDone())
                    {
                        currTok = toker.getTok().ToString();
                        linecount = toker.lineCount();
                    }
                    else
                    {
                        currTok = null;
                        return null;//no token
                    }
                    return null;
                }
                catch
                {
                    Console.WriteLine("fail to get Token");
                }
                return null;
            }

            //----< is this character a punctuator> >----------------------------

            bool IsPunc(char ch)
            {
                return (Char.IsPunctuation(ch) || Char.IsSymbol(ch));
            }

            //----< are these characters an operator? >--------------------------
            // Performance issue - C# would not let me make opers static, so
            // it is being constructed on every call.  This is not desireable,
            // but neither is using a static data member that is initialized
            // remotely.  I will think more about this later.

            bool IsOperatorPair(char first, char second)
            {
                string[] opers = new string[]
                {
                    "/*", "*/", "//", "!=", "==", ">=", "<=", "&&", "||", "--", "++",
                     "+=", "-=", "*=", "/=", "%=", "&=", "^=", "|=", "<<", ">>",
                    "\\n", "\\t", "\\r", "\\f"
                };

                StringBuilder test = new StringBuilder();//text？
                test.Append(first).Append(second);
                foreach (string oper in opers)
                    if (oper.Equals(test.ToString()))
                        return true;
                return false;
            }

            //----< collect semiExpression from filtered token stream >----------

            public bool isWhiteSpace(string tok)
            {
                Char ch = tok[0];
                return Char.IsWhiteSpace(tok[0]);
            }

            public void trim()
            {
                SemiExpression temp = new SemiExpression();
                foreach (string tok in semiExp)
                {
                    if (isWhiteSpace(tok))
                        continue;
                    temp.Add(tok);
                }
                semiExp = temp.semiExp;
            }

            public bool getSemi()
            {
                semiExp.RemoveRange(0, semiExp.Count);  // empty container
                do
                {
                    {
                        get();
                        if (currTok == null)
                            return false;  // end of file
                        if (currTok.Equals("using") || currTok.Equals("#")) //line starting with "using" or #
                        {
                            string newline = null;
                            newline = "\n";
                            semiExp.Add(newline);
                        }
                        semiExp.Add(currTok);
                        while (currTok.Equals("for"))//determine whether is rule : for(;;)
                        {
                            int countopen = 0;
                            int countclose = 0;
                            do
                            {
                                get();
                                if (currTok == null)
                                    return false;
                                semiExp.Add(currTok);
                                if (currTok == "(")
                                    countopen++;
                                if (currTok == ")")
                                    countclose++;
                            } while (countopen != countclose);
                            get();
                            if (currTok == null)
                                return false;
                            semiExp.Add(currTok);
                        }

                        if (currTok.Length >= 1)
                        {
                            if (currTok[currTok.Length - 1] == ';' || currTok[currTok.Length - 1] == '{' || currTok[currTok.Length - 1] == '}')
                                return (semiExp.Count > 0);//terminated by ";", "{", "}"
                        }
                    }
                } while ((!isTerminated(currTok)) || (semiExp.Count == 0) || (currTok != null));
                return (semiExp.Count > 0);//true or false
            }

            //----< get length property >----------------------------------------

            public int count
            {
                get { return semiExp.Count; }
            }

            //----< indexer for semiExpression >---------------------------------

            public string this[int i]
            {
                get { return semiExp[i]; }
                set { semiExp[i] = value; }
            }

            //----< insert token - fails if out of range and returns false>------

            public bool insert(int loc, string tok)
            {
                if (0 <= loc && loc < semiExp.Count)
                {
                    semiExp.Insert(loc, tok);
                    return true;
                }
                return false;
            }

            //----< append token to end of semiExp >-----------------------------

            public SemiExpression Add(string token)
            {
                semiExp.Add(token);
                return this;
            }

            //----< load semiExp from array of strings >-------------------------

            public void Add(string[] source)
            {
                foreach (string tok in source)
                    semiExp.Add(tok);
            }

            //--< initialize semiExp with single ";" token - used for testing >--

            public bool initialize()
            {
                if (semiExp.Count > 0)
                    return false;
                semiExp.Add(";");
                return true;
            }

            //----< remove all contents of semiExp >-----------------------------

            public void flush()
            {
                semiExp.RemoveRange(0, semiExp.Count);
            }

            //----< is this token a comment? >-----------------------------------

            public bool isComment(string tok)
            {
                if (tok.Length > 1)
                    if (tok[0] == '/')
                        if (tok[1] == '/' || tok[1] == '*')// //，/*
                            return true;
                return false;
            }

            //----< display semiExpression on Console >--------------------------

            public void display()
            {
                Console.Write("\n -- ");
                Console.Write(displayStr());
            }

            //----< return display string >--------------------------------------

            public string displayStr()
            {
                StringBuilder disp = new StringBuilder("");
                foreach (string tok in semiExp)
                {
                    disp.Append(tok);
                    if (tok.IndexOf('\n') != tok.Length - 1)
                        disp.Append(" ");
                }
                return disp.ToString();
            }

            //----< announce tokens when verbose is true >-----------------------

            public bool verbose
            {
                get;
                set;
            }
            //----< determines whether new lines are returned with semi >--------

            public bool returnNewLines
            {
                get;
                set;
            }

            //----< determines whether new lines are displayed >-----------------

            public bool displayNewLines
            {
                get;
                set;
            }

            //----< determines whether comments are discarded >------------------

            public bool discardComments
            {
                get;
                set;
            }

            //----< make a copy of semiEpression >-------------------------------

            public SemiExpression clone()
            {
                SemiExpression copy = new SemiExpression();
                for (int i = 0; i < count; ++i)
                {
                    copy.Add(this[i]);
                }
                return copy;
            }

            //----< remove a token from semiExpression >-------------------------

            public bool remove(int i)
            {
                if (0 <= i && i < semiExp.Count)
                {
                    semiExp.RemoveAt(i);
                    return true;
                }
                return false;
            }

            //----< remove a token from semiExpression >-------------------------

            public bool remove(string token)
            {
                if (semiExp.Contains(token))
                {
                    semiExp.Remove(token);
                    return true;
                }
                return false;
            }


            //----< test stub >--------------------------------------------------

#if (TEST_SEMI)
            [STAThread]
            static bool testfile(string path)
            {
                SemiExpression test = new SemiExpression();
                if (!test.open(path))
                {
                    Console.Write("\n  Can't open file {0}", path);
                    return false;
                }
                int setcount = 0;
                while (test.getSemi())
                {
                    setcount++;
                    Console.Write("\n**Set {0}", setcount);
                    test.display();
                }
                test.close();
                return true;
            }
        
            static void Main(string[] args)
            {
              Console.Write("\n  Testing semiExp Operations");
              Console.Write("\n ============================\n");

              testfile("../../TestSemi.txt");
              testfile("../../Tokenizer.cs");
              Console.ReadLine();
            }
#endif
        }
    }
}

