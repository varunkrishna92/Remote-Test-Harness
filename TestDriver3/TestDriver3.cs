/////////////////////////////////////////////////////////////////////////////
//  Remote Test Harness - Project 4                                        //
//  TestDriver3.cs                                                          //
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
 *   Test Driver package implements the ITest interface and defines a test to run 
 *   on the test code.
 *       
 *   Public Interface
 *   ----------------
 *   TestDriver3 testDriver = new TestDriver();
 *   bool result = testDriver.test();
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   TestDriver.cs
 *    
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 20 Nov 2016
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
    public class TestDriver3 : ITest
    {
        public bool test()
        {
            TestCode3 tested = new TestCode3();
            int a = 90, b = 0;
            double d = tested.Divide(a, b);
            return true;
        }
        public string getLog()
        {
            return "Divide test that always passes";
        }

#if (TEST_DRIVER3)
        static void Main(string[] args)
        {
            TestDriver _testDriver = new TestDriver();
            bool testCode = _testDriver.test();
            if (testCode)
                Console.WriteLine("Test Passed");
            else
                Console.WriteLine("Test Failed");
        }
#endif
    }
}
