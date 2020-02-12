/////////////////////////////////////////////////////////////////////////
// ScannerExec.cs   -  Executing the program                                                                                          
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
 * ScannerExec provides, via the class ScannerExec, the facilities to 
 * initiate the Lexical Scanner.
 * 
 * An Automated Testing Unit is built to test the scanner to meet 
 * all the requirements in project 2.
 * 
 * Public Interface
 * ================
 * ScannerExec ScannerTest = new ScannerExec();       // constructs ScannerExec object
 * testToken(string) // intiate tester to test the tokenizer
 * testSemi(string)   // intiate tester to test the SemiExpression
 * setSpecialSingleChars(char);// set special single char
 * setSpecialCharPairs(string)// set special char pairs
 * printSpecialSingleChars // print the list of special single char
 * printSpecialCharPairs // print the list of special char pairs
 */
/*
 * Build Process
 * =============
 * Required Files:
 *  ScannerExec.cs SemiExpression.cs Tokenizer.cs ITokCollection.cs
 * 
 * Compiler Command:
 *  ScannerExec.cs
 * 
 * Maintenance History
 * ===================
 * ver 1.0 : 03 Oct 18
 * - first release
 * 
 * Planned Changes:
 * ----------------
 * - add more functionalities.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexical_Scanner
{
    namespace ScannerExec
    {
        using SemiExpression;
        using Tokenizer;
        using Token = StringBuilder;

        ///////////////////////////////////////////////////////////////////
        // ScannerExec.class
        // - class to initate the sacnner
        // - here we just provided methods to complete the test

        class ScannerExec
        {
            Toker toker = new Toker();
            SemiExpression test = new SemiExpression();
            public ScannerExec()
            { }

            //----< methods that used to initiate the tokenizer tester>-----------
            public bool testToken(string path)
            {
                string fqf = System.IO.Path.GetFullPath(path);
                if (!toker.open(fqf))
                {
                    Console.Write("\n can't open {0}\n", fqf);
                    return false;
                }
                while (!toker.isDone())
                {
                    Token tok = toker.getTok();
                    Console.Write("\n -- line#{0, 4} : {1}", toker.lineCount(), tok);
                }
                toker.close();
                return true;
            }


            //----< methods that used to initiate the semiexpression tester>-----------
            public bool testSemi(string path)
            {
                if (!test.open(path))
                {
                    Console.Write("\n  Can't open file {0}", path);
                    return false;
                }
                int setcount = 0;
                while (test.getSemi())
                {
                    setcount++;
                    Console.Write("\n ++ Set {0}", setcount);
                    test.display();
                }
                test.close();
                return true;
            }

            //----< methods that used to handle the special characer sets>-----------
            public bool setSpecialSingleChars(char ssc)
            {
                test.setSpecialSingleChars(ssc);
                return true;
            }
            public bool setSpecialCharPairs(string scp)
            {
                test.setSpecialCharPairs(scp);
                return true;
            }

            public bool printSpecialSingleChars()
            {
                test.printSpecialSingleChars();
                return true;
            }
            public bool printSpecialCharPairs()
            {
                test.printSpecialCharPairs();
                return true;
            }
        }

        ///////////////////////////////////////////////////////////////////
        // AutomatedTestUnit.class
        // - class to test the scanner with necessary information:
        // - 1. Test the extracted tokens and semiexpression set with all the special cases (stored in("../../TestSpecialCases.cs")) including:
        /* Tokenizer special cases:
        // 1.Special alphanumeric token: @505, _Test;
        // 2.Special token: "\"", "\\";if (ch == '\"' || ch == '\'')
        // 3.tokens to change = .( &=|&&-+=;
        // SemiExpression special cases:
        // 1.#Demo of all special case
        // {
        //      StringBuilder temp = new StringBuilder();
        //      temp.Append("{");
        //      temp.Append(String.Format("{0,-10}", type)).Append(" : ");
        //      temp.Append(String.Format("{0,-5}", endLine.ToString()));    // line of scope end
        //      temp.Append("}");
        //      return temp.ToString();
        // }
        // int p;
        // 2.using namespace Lexical_Scanner;
        // 3.for(int j=0; j<5; ++j)
        //    for(int k=0; k<2; ++k)
        //    p = i* j* k; */
        // - 2. Test the methods to handle new rules of special character sets
        // - 3. T the scanner with a .cs file ("../../SemiExpression.cs")
    }
    class AutomatedTestUnit
    {
        static void Main(string[] args)
        {
            string filename1 = "../../TestSpecialCases.txt";
            string filename2 = "../../SemiExpression.cs";
            ScannerExec.ScannerExec ScannerTest = new ScannerExec.ScannerExec();

            Console.Write("\n ===================================================================");
            Console.Write("\n Lexical Scanner");
            Console.Write("\n Xiaoqian Huang (SUID: 878174727)");
            Console.Write("\n CSE 681");
            Console.Write("\n Software Modeling and Analysis");
            Console.Write("\n October 3rd, 2018");
            Console.Write("\n ===================================================================");
            Console.Write("\n ===================================================================");
            Console.Write("\n 1.Testing Special cases...");
            Console.Write(" Reading file:{0}", filename1);
            Console.Write("\n 1.1.Displaying Tokens:", filename1);
            ScannerTest.testToken(filename1);
            Console.Write("\n 1.2.Displaying SemiExpressions:", filename1);
            ScannerTest.testSemi(filename1);
            Console.Write("\n ===================================================================");
            Console.Write("\n 2.Managing Rules");
            Console.Write("\n 2.1.Special single char");
            ScannerTest.printSpecialSingleChars();
            Console.Write("\n Adding special single token \'|\':");
            ScannerTest.setSpecialSingleChars('|');
            ScannerTest.printSpecialSingleChars();
            Console.Write("\n 2.1.Special char pairs");
            ScannerTest.printSpecialCharPairs();
            Console.Write("\n Adding special char pair token \"&=\":");
            ScannerTest.setSpecialCharPairs("&=");
            ScannerTest.printSpecialCharPairs();
            ScannerTest.testSemi(filename1);
            Console.Write("\n ===================================================================");
            Console.Write("\n 3.Scannering C# file...");
            Console.Write(" Reading file:{0}", filename2);
            ScannerTest.testSemi(filename2);
            Console.ReadLine();
        }
    }
}