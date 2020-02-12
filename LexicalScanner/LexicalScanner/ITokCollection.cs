/////////////////////////////////////////////////////////////////////////
// ITokCollection.cs   -  Interface to declair get()                                                                                          
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
 * ITokCollection is the interface to declare get();
/*
 * Build Process
 * =============
 * Required Files:
 *   ITokCollection.cs
 * 
 * Maintenance History
 * ===================
 * ver 1.0 : 03 Oct 18
 * - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexical_Scanner
{
    public interface ITokCollection
    {
        string get();
    }
}

