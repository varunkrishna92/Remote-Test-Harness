/////////////////////////////////////////////////////////////////////////////
//  Remote Test Harness - Project 4                                        //
//  TestCode2.cs                                                            //
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
 *   MaxHeight(args[0], args[1]);
 * 
 *  */
/*
 *   Build Process
 *   -------------
 *   - Required files:   TestCode2.cs
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
    public class TestCode2
    {
        public void MaxHeight(string[] height, int size)
        {
            int[] inches = new int[size];
            int max = 0;
            for (int i = 0; i < size; i++)
            {
                string[] temp = height[i].Split('#');
                inches[i] = (Convert.ToInt16(temp[0]) * 12) + Convert.ToInt16(temp[1]);
                //Console.WriteLine(inches[i] + "" + i);

            }
            max = inches[0];
            for (int j = 1; j < size; j++)
            {
                if (inches[j] > max)
                    max = inches[j];
            }
            Console.WriteLine("\n Maximum Height : " + max + " inches");
        }
#if (TEST_CODE2)
        static void Main(string[] args)
        {
            TestCode2 _testCode = new TestCode2();
            string[] height = { "5#4", "6#2", "5#1", "6#6" };
            _testCode.MaxHeight(height, height.Length);
        }
#endif
    }
}
