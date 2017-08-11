/////////////////////////////////////////////////////////////////////////////
//  Remote Test Harness - Project 4                                        //
//  TestCode4.cs                                                           //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Demonstration for CSE681 - Software Modeling & Analysis  //
//  Source:       Proffesor Jim Fawcett                                    // 
//  Author:       Varun Krishna Peruru Vanaparthy                          //
//                vperuruv@syr.edu                                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   TestCode package defines the code to be tested.
 *       
 *    Public Interface
 *   ----------------
 *   Divide(args[0], args[1]);
 * 
 *  */
/*
 *   Build Process
 *   -------------
 *   - Required files:   TestCode.cs
 *   
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 07 Oct 2016
 *     - first release
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote_TestHarness
{
    public class TestCode4
    {
        public double Divide(int a, int b)
        {
            return a / b;
        }

#if (TEST_CODE4)
        static void Main(string[] args)
        {
            TestCode _testCode = new TestCode();
            int a = 80, b = 20;
            double d = Divide(a / b);
        }
#endif
    }
}
