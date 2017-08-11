/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  LoadAndTest.cs                                                         //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Demonstration for CSE681 - Software Modeling & Analysis  //
//  Source:       Proffesor Jim Fawcett                                    // 
//  Author:       Varun Krishna Peruru Vanaparthy                          //
//                vperuruv@syr.edu                                         //
//                                                                         //  
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   LoadAndTest package operates in child AppDomain.  It loads and
 *   executes test code defined by a TestRequest message.
 *    
 *   Public Interface
 *   ----------------
 *   IRequestInfo info = args[0];
 *   ITestResults tr = test();
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   LoadAndTest.cs, ITest.cs, Messages
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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Remote_TestHarness
{
    public class LoadAndTest : MarshalByRefObject, ILoadAndTest
    {
        private ICallback cb_ = null;
        private string loadPath_ = "";
        object sync_ = new object();
        ///////////////////////////////////////////////////////
        // Data Structures used to store test information
        //
        [Serializable]
        private class TestResult : ITestResult
        {
            public string testName { get; set; }
            public string testResult { get; set; }
            public string testLog { get; set; }
        }
        [Serializable]
        private class TestResults : ITestResults
        {
            public string testKey { get; set; }
            public DateTime dateTime { get; set; }
            public List<ITestResult> testResults { get; set; } = new List<ITestResult>();
        }
        TestResults testResults_ = new TestResults();

        //----< initialize loggers >-------------------------------------

        public LoadAndTest()
        {
            
        }
        public void loadPath(string path)
        {
            loadPath_ = path;
            Console.Write("\n  loadpath = {0}", loadPath_);
        }
        //----< load libraries into child AppDomain and test >-----------
       
        public ITestResults test(IRequestInfo testRequest)
        {
            TestResults testResults = new TestResults();
            foreach (ITestInfo test in testRequest.requestInfo)
            {
                TestResult testResult = new TestResult();
                testResult.testName = test.testName;
                try
                {
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": -- \"" + test.testName + "\" --");

                    ITest tdr = null;
                    string testDriverName = "";
                    string fileName = "";

                    foreach (string file in test.files)
                    {
                        fileName = file;
                        Assembly assem = null;
                        try
                        {
                            if (loadPath_.Count() > 0)
                            {
                                assem = Assembly.LoadFrom(loadPath_ + "/" + file);
                            }
                            else
                                assem = Assembly.Load(file);
                        }
                        catch
                        {
                            testResult.testResult = "failed";
                            testResult.testLog = "file not loaded";
                            Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": can't load\"" + file + "\"");
                            continue;
                        }
                        Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": loaded \"" + file + "\"");
                        Type[] types = assem.GetExportedTypes();

                        foreach (Type t in types)
                        {
                            if (t.IsClass && typeof(ITest).IsAssignableFrom(t))  // does this type derive from ITest ?
                            {
                                try
                                {
                                    testDriverName = file;
                                    tdr = (ITest)Activator.CreateInstance(t);    // create instance of test driver
                                    Console.WriteLine("------------------------Requirement 5 - Line 131 LoadAndTest.cs---------------");
                                    Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": " + testDriverName + " implements ITest interface");
                                }
                                catch
                                {
                                    //Console.Write("\n----" + file + " - exception thrown when created");
                                    continue;
                                }
                            }
                        }
                    }
                    Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": testing " + testDriverName);
                    bool testReturn;
                    try
                    {
                        testReturn = tdr.test();
                    }
                    catch
                    {
                        //Console.Write("\n----exception thrown in " + fileName);
                        testReturn = false;
                    }
                    if (tdr != null && testReturn == true)
                    {
                        testResult.testResult = "passed";
                        testResult.testLog = tdr.getLog();
                        Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": test passed");
                        if (cb_ != null)
                        {
                            cb_.sendMessage(new Messages(testDriverName + " passed"));
                        }
                    }
                    else
                    {
                        testResult.testResult = "failed";
                        if (tdr != null)
                            testResult.testLog = tdr.getLog();
                        else
                            testResult.testLog = "file not loaded";
                        Console.Write("\n    TID" + Thread.CurrentThread.ManagedThreadId + ": test failed");
                        if (cb_ != null)
                        {
                            cb_.sendMessage(new Messages(testDriverName + ": failed"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    testResult.testResult = "failed";
                    testResult.testLog = "exception thrown";
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + ex.Message);
                }
                testResults_.testResults.Add(testResult);
            }

            testResults_.dateTime = DateTime.Now;
            testResults_.testKey = System.IO.Path.GetFileName(loadPath_);
            return testResults_;
        }
        //----< TestHarness calls to pass ref to Callback function >-----

        public void setCallback(ICallback cb)
        {
            cb_ = cb;
        }

#if (TEST_LOADANDTEST)
static void Main(string[] args)
    {
        IRequestInfo info = args[0];
        ITestResults tr = test();
    }
#endif
    }
}
