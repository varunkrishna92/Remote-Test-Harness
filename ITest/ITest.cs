/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  ITest.cs                                                               //
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
 *   ITest.cs provides interfaces:
 * - ITestHarness   used by TestExec and Client
 * - ICallback      used by child AppDomain to send messages to TestHarness
 * - IRequestInfo   used by TestHarness
 * - ITestInfo      used by TestHarness
 * - ILoadAndTest   used by TestHarness
 * - ITest          used by LoadAndTest
 * - IRepository    used by Client and TestHarness
 * - IClient        used by TestExec and TestHarness
 *
 * Required files:
 * ---------------
 * - ITest.cs
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
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to send messages to TestHarness

    public interface ICallback
    {
        void sendMessage(Messages msg);
    }
    public interface ITestHarness
    {
        void setClient(IClient client);
        void sendTestRequest(Messages testRequest);
        Messages sendMessage(Messages msg);
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to invoke test driver's test()

    public interface ITest
    {
        bool test();
        string getLog();
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to communicate with Repository
    // via TestHarness Comm

    public interface IRepository
    {
        bool getFiles(string path, string fileList);  // fileList is comma separated list of files
        void sendLog(Messages log);
        List<string> queryLogs(string queryText);
    }
    /////////////////////////////////////////////////////////////
    // used by child AppDomain to send results to client
    // via TestHarness Comm

    public interface IClient
    {
        void sendResults(Messages result);
        void makeQuery(string queryText);
    }
    /////////////////////////////////////////////////////////////
    // used by TestHarness to communicate with child AppDomain

    public interface ILoadAndTest
    {
        ITestResults test(IRequestInfo requestInfo);
        void setCallback(ICallback cb);
        void loadPath(string path);
    }
    public interface ITestInfo
    {
        string testName { get; set; }
        List<string> files { get; set; }
    }
    public interface IRequestInfo
    {
        List<ITestInfo> requestInfo { get; set; }
    }
    public interface ITestResult
    {
        string testName { get; set; }
        string testResult { get; set; }
        string testLog { get; set; }
    }
    public interface ITestResults
    {
        string testKey { get; set; }
        DateTime dateTime { get; set; }
        List<ITestResult> testResults { get; set; }
    }
}
