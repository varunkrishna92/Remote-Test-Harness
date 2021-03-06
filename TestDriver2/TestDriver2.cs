﻿/////////////////////////////////////////////////////////////////////////////
//  Remote Test Harness - Project 4                                        //
//  TestDriver.cs                                                          //
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
 *   TestDriver2 testDriver = new TestDriver();
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
    public class TestDriver2 : ITest
    {
        public bool test()
        {
            TestCode2 tested = new TestCode2();
            string[] height = { "5#8", "5#7", "6#0", "5..7" };
            int size = height.Length;
            tested.MaxHeight(height, size);
            return true;
        }
        public string getLog()
        {
            return "Max Height test that always fails";
        }

#if (TEST_DRIVER2)
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
