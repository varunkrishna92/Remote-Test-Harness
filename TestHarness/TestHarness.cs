/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  TestHarness.cs                                                         //
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
 *   TestHarness package provides integration testing services.  It:
 * - receives structured test requests
 * - retrieves cited files from a repository
 * - executes tests on all code that implements an ITest interface,
 *   e.g., test drivers.
 * - reports pass or fail status for each test in a test request
 * - stores test logs in the repository
 * It contains classes:
 * - TestHarness that runs all tests in child AppDomains
 * - Callback to support sending messages from a child AppDomain to
 *   the TestHarness primary AppDomain.
 * - Test and RequestInfo to support transferring test information
 *   from TestHarness to child AppDomain
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:  TestHarness.cs, BlockingQueue.cs
 * - ITest.cs
 * - LoadAndTest, Logger, Messages
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Policy;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Remote_TestHarness
{
    // Callback class
    
    public class Callback : MarshalByRefObject, ICallback
    {
        public void sendMessage(Messages message)
        {
            Console.Write("\n  received msg from childDomain: \"" + message.body + "\"");
        }
    }
    
    // Test and RequestInfo are used to pass test request information
    // to child AppDomain
    //
    [Serializable]
    public class Test : ITestInfo
    {
        public string testName { get; set; }
        public List<string> files { get; set; } = new List<string>();
    }
    [Serializable]
    class RequestInfo : IRequestInfo
    {
        public string tempDirName { get; set; }
        public List<ITestInfo> requestInfo { get; set; } = new List<ITestInfo>();
    }
    
    public class TestHarness : ITestHarness
    {
        public BlockingQueue<Messages> inQ_ { get; set; } = new BlockingQueue<Messages>();
        private ICallback cb_;
        private IRepository repo_;
        private IClient client_;
        public TestHarness testHarness { get; set; }
        //public static Client client { get; set; }
        public Repository repository { get; set; }
        //private string localDir_;
        private string repoPath_ = "../../../Repository/RepositoryStorage/";
        private string filePath_;
        object sync_ = new object();
        //private string loaderPath_;
        List<Thread> threads_ = new List<Thread>();
        Dictionary<int, string> TLS = new Dictionary<int, string>();
        public Comm<TestHarness> comm { get; set; } = new Comm<TestHarness>();
        public string endPoint { get; } = Comm<TestHarness>.makeEndPoint("http://localhost", 8080);
        private Thread rcvThread = null;
        IStreamService channel;
        HiResTimer hrt = null;

        public TestHarness(IRepository repo)
        {
            //Console.Write("\n  creating instance of TestHarness");
            repo_ = repo;
            repoPath_ = System.IO.Path.GetFullPath(repoPath_);
            cb_ = new Callback();
            //client = new Client(testHarness as ITestHarness);
            //repository = new Repository();
            //testHarness.setClient(client);
            //client.setRepository(repository);
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
        }
        
        public void setClient(IClient client)
        {
            client_ = client;
        }
        
        public void sendTestRequest(Messages testRequest)
        {
            Console.WriteLine("\n------------------------Requirement 2 - Line 130 TestHarness.cs--------------------");
            Console.Write("\n  TestHarness received a testRequest");
            inQ_.enQ(testRequest);
        }

        public Messages sendMessage(Messages msg)
        {
            return msg;
        }
        //Create path name from author and time >--------------------

        string makeKey(string author)
        {
            DateTime now = DateTime.Now;
            string nowDateStr = now.Date.ToString("d");
            string[] dateParts = nowDateStr.Split('/');
            string key = "";
            foreach (string part in dateParts)
                key += part.Trim() + '_';
            string nowTimeStr = now.TimeOfDay.ToString();
            string[] timeParts = nowTimeStr.Split(':');
            for (int i = 0; i < timeParts.Count() - 1; ++i)
                key += timeParts[i].Trim() + '_';
            key += timeParts[timeParts.Count() - 1];
            key = author + "_" + key + "_" + "ThreadID" + Thread.CurrentThread.ManagedThreadId; 
            return key;
        }
        //Retrieve test information from testRequest

        List<ITestInfo> extractTests(Messages testRequest)
        {
            Console.Write("\n  parsing test request");
            List<ITestInfo> tests = new List<ITestInfo>();
            XDocument doc = XDocument.Parse(testRequest.body);
            foreach (XElement testElem in doc.Descendants("test"))
            {
                Test test = new Test();
                string testDriverName = testElem.Element("testDriver").Value;
                test.testName = testElem.Attribute("name").Value;
                test.files.Add(testDriverName);
                foreach (XElement lib in testElem.Elements("testCode"))
                {
                    test.files.Add(lib.Value);
                }
                tests.Add(test);
            }
            return tests;
        }
        //Retrieve test code from testRequest 

        List<string> extractCode(List<ITestInfo> testInfos)
        {
            List<string> codes = new List<string>();
            foreach (ITestInfo testInfo in testInfos)
                codes.AddRange(testInfo.files);
            return codes;
        }
        //Create local directory and load from Repository

        static IStreamService CreateServiceChannel(string url)
        {
            BasicHttpSecurityMode securityMode = BasicHttpSecurityMode.None;

            BasicHttpBinding binding = new BasicHttpBinding(securityMode);
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 500000000;
            EndpointAddress address = new EndpointAddress(url);

            ChannelFactory<IStreamService> factory
              = new ChannelFactory<IStreamService>(binding, address);
            return factory.CreateChannel();
        }

        RequestInfo processRequestAndLoadFiles(Messages testRequest)
        {
            channel = CreateServiceChannel("http://localhost:8000/StreamService");
            string localDir_ = "";
            RequestInfo rqi = new RequestInfo();
            rqi.requestInfo = extractTests(testRequest);
            List<string> files = extractCode(rqi.requestInfo);
            Console.WriteLine("\n------------------------Requirement 8 - Line 210 TestHarness.cs--------------------");
            Console.WriteLine("Creating a unique key to store the results");
            localDir_ = makeKey(testRequest.author);            // name of temporary dir to hold test files
            rqi.tempDirName = localDir_;
            lock (sync_)
            {
                filePath_ = System.IO.Path.GetFullPath(localDir_);  // LoadAndTest will use this path
                TLS[Thread.CurrentThread.ManagedThreadId] = filePath_;
            }
            Console.Write("\n  creating local test directory \"" + localDir_ + "\"");
            System.IO.Directory.CreateDirectory(localDir_);

            Console.Write("\n  loading code from Repository");
            
            foreach (string file in files)
            {
                try
                {
                    download(file, localDir_);
                }
                catch
                {
                    //Console.WriteLine("File not avalailable in the repository--Req 3\n");
                    lock (sync_)
                    {
                        comm.sndr.PostMessage(MakeErrorMessage(testRequest));                        
                    }
                    
                }
                
            }
            Console.WriteLine();
            return rqi;
        }

        void download(string filename, string savePath)
        {
            //string savePath = "..\\..\\..\\Repository\\RepositoryStorage"
            int BlockSize = 1024;
            byte[] block;
            HiResTimer hrt2 = new HiResTimer();
            block = new byte[BlockSize];
            int totalBytes = 0;
            
                hrt2.Start();
                Stream strm = channel.downLoadFile(filename);
                string rfilename = Path.Combine(savePath, filename);
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    while (true)
                    {
                        int bytesRead = strm.Read(block, 0, BlockSize);
                        totalBytes += bytesRead;
                        if (bytesRead > 0)
                            outputStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                }
                hrt2.Stop();
                ulong time = hrt2.ElapsedMicroseconds;
                Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
            
            
        }
        //Save results and logs in Repository

        bool saveResultsAndLogs(ITestResults testResults)
        {
            Console.WriteLine("\n------------------------Requirement 7 - Line 277 TestHarness.cs--------------------");
            Console.WriteLine("Storing logs in the repository");
            string logName = testResults.testKey + ".txt";
            StreamWriter sr = null;
            try
            {
                sr = new System.IO.StreamWriter(System.IO.Path.Combine(repoPath_, logName));
                sr.WriteLine(logName);
                foreach (ITestResult test in testResults.testResults)
                {
                    sr.WriteLine("-----------------------------");
                    sr.WriteLine(test.testName);
                    sr.WriteLine(test.testResult);
                    sr.WriteLine(test.testLog);
                }
                sr.WriteLine("-----------------------------");
            }
            catch
            {
                sr.Close();
                return false;
            }
            sr.Close();
            return true;
        }
        //Run tests 
        
        ITestResults runTests(Messages testRequest)
        {
            AppDomain ad = null;
            ILoadAndTest ldandtst = null;
            RequestInfo rqi = null;
            ITestResults tr = null;

            try
            {
                lock (sync_)
                {
                    rqi = processRequestAndLoadFiles(testRequest);
                    ad = createChildAppDomain();
                    ldandtst = installLoader(ad);
                }
                if (ldandtst != null)
                {
                    tr = ldandtst.test(rqi);
                }
                // unloading ChildDomain, and so unloading the library

                saveResultsAndLogs(tr);

                //lock (sync_)  // this lock scope is no longer needed due to use of "thread local storage Dictionary - TLS"
                {
                    Console.WriteLine("\n------------------------Requirement 7 - Line 333 TestHarness.cs--------------------");
                    Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": unloading: \"" + ad.FriendlyName + "\"\n");
                    AppDomain.Unload(ad);
                    try
                    {
                        System.IO.Directory.Delete(rqi.tempDirName, true);
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": removed directory " + rqi.tempDirName);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": could not remove directory " + rqi.tempDirName);
                        Console.Write("\n  TID" + Thread.CurrentThread.ManagedThreadId + ": " + ex.Message);
                    }
                }
                return tr;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n---- {0}\n\n", ex.Message);
                return tr;
            }
        }
        //Make TestResults Message

        Messages makeTestResultsMessage(ITestResults tr, Messages testRequest)
        {
            Messages trMsg = new Messages();
            trMsg.author = "TestHarness";
            trMsg.to = testRequest.from;
            trMsg.from = "TH";
            XDocument doc = new XDocument();
            XElement root = new XElement("testResultsMsg");
            doc.Add(root);
            XElement testKey = new XElement("testKey");
            testKey.Value = tr.testKey;
            root.Add(testKey);
            XElement timeStamp = new XElement("timeStamp");
            timeStamp.Value = tr.dateTime.ToString();
            root.Add(timeStamp);
            XElement testResults = new XElement("testResults");
            root.Add(testResults);
            foreach (ITestResult test in tr.testResults)
            {
                XElement testResult = new XElement("testResult");
                testResults.Add(testResult);
                XElement testName = new XElement("testName");
                testName.Value = test.testName;
                testResult.Add(testName);
                XElement result = new XElement("result");
                result.Value = test.testResult;
                testResult.Add(result);
                XElement log = new XElement("log");
                log.Value = test.testLog;
                testResult.Add(log);
            }
            trMsg.body = doc.ToString();
            return trMsg;
        }

        public Messages MakeErrorMessage(Messages msg)
        {
            Messages errorMsg = new Messages();
            errorMsg.author = msg.author;
            errorMsg.from = "TH";
            errorMsg.to = msg.from;
            errorMsg.body = "File Error";
            errorMsg.time = DateTime.Now;
            errorMsg.type = "Error";
            return errorMsg;
        }
        //Wait for all threads to finish
        
        public void wait()
        {
            foreach (Thread t in threads_)
                t.Join();
        }
        //Main activity of TestHarness

        public void processMessages()
        {
            AppDomain main = AppDomain.CurrentDomain;
            Console.Write("\n  Starting in AppDomain " + main.FriendlyName + "\n");
            HiResTimer hrt1 = new HiResTimer();
            int requestCount = 0;
            ThreadStart doTests = () => {
                while (true)
                {
                    Messages testRequest = inQ_.deQ();
                    hrt1.Start();
                    if (++requestCount == 1)
                        Console.Write("\n\n  First Message body:{0}", testRequest.body);

                    if (testRequest.body == "quit")
                    {
                        inQ_.enQ(testRequest);
                        return;
                    }
                    ITestResults testResults = runTests(testRequest);
                    hrt1.Stop();
                    Console.WriteLine("\n------------------------Requirement 12 - Line 431 TestHarness.cs--------------------");
                    Console.WriteLine("Test Completed in " + hrt1.ElapsedMicroseconds);
                    Console.WriteLine("\n------------------------Requirement 6 and 10- Line 433 TestHarness.cs---------------");
                    Console.Write("\n  Sending Results to the client using WCF---  ");

                    lock (sync_)
                    {
                        
                        comm.sndr.PostMessage(makeTestResultsMessage(testResults, testRequest));
                        //client_.sendResults(makeTestResultsMessage(testResults));
                    }
                }
            };
            Console.WriteLine("\n------------------------Requirement 4 - Line 401 TestHarness.cs---------------");
            Console.Write("\n  Creating AppDomain thread");
            Thread t = new Thread(doTests);
            threads_.Add(t);
            t.Start();
            
        }

        void rcvThreadProc()
        {
            while (true)
            {
                Messages msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:", comm.name);
                msg.showMsg();
                if (msg.body == "quit")
                    break;
                sendTestRequest(msg);
                processMessages();
            }
        }
       

        void showAssemblies(AppDomain ad)
        {
            Assembly[] arrayOfAssems = ad.GetAssemblies();
            foreach (Assembly assem in arrayOfAssems)
                Console.WriteLine("\n  " + assem.ToString());
        }
        //----< create child AppDomain >---------------------------------

        public AppDomain createChildAppDomain()
        {
            try
            {
                //DLog.flush();
                //RLog.write("\n  creating child AppDomain - Req #5");

                AppDomainSetup domaininfo = new AppDomainSetup();
                domaininfo.ApplicationBase
                  = "file:///" + System.Environment.CurrentDirectory;  // defines search path for LoadAndTest library

                //Create evidence for the new AppDomain from evidence of current

                Evidence adevidence = AppDomain.CurrentDomain.Evidence;

                // Create Child AppDomain

                AppDomain ad
                  = AppDomain.CreateDomain("ChildDomain", adevidence, domaininfo);

                //DLog.write("\n  created AppDomain \"" + ad.FriendlyName + "\"");
                return ad;
            }
            catch
            {
                //RLog.write("\n  " + except.Message + "\n\n");
            }
            return null;
        }
        //----< Load and Test is responsible for testing >---------------

        ILoadAndTest installLoader(AppDomain ad)
        {
            ad.Load("LoadAndTest");
            //showAssemblies(ad);
            //Console.WriteLine();

            // create proxy for LoadAndTest object in child AppDomain

            ObjectHandle oh
              = ad.CreateInstance("LoadAndTest", "Remote_TestHarness.LoadAndTest");
            object ob = oh.Unwrap();    // unwrap creates proxy to ChildDomain
                                        // Console.Write("\n  {0}", ob);

            // set reference to LoadAndTest object in child

            ILoadAndTest landt = (ILoadAndTest)ob;

            // create Callback object in parent domain and pass reference
            // to LoadAndTest object in child

            landt.setCallback(cb_);
            landt.loadPath(filePath_);  // send file path to LoadAndTest
            return landt;
        }
        static void Main(string[] args)
        {
            Console.Title = "Test Harness";
            TestHarness testHarness = new TestHarness(null);
            Console.WriteLine(" \n************************************Test Harness**********************************");
            //client = new Client(testHarness as ITestHarness);
            //client.setRepository(repository);
            //testHarness.setClient(client);
            //testHarness.processMessages();
        }
    }
}
