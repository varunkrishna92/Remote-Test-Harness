/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  Repository.cs                                                          //
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
 *   This package implements the repository operations of storing logs and queries.
 *    
 *   Public Interface
 *   ----------------
 *  Messages msg = new Messages();
 *  sendLog(msg);
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   Repository.cs, Messages.cs
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
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Remote_TestHarness
{
    public class Repository : IRepository, IStreamService
    {
        string filename;
        string repoStoragePath = "..\\..\\..\\Repository\\RepositoryStorage\\";
        string savePath = "..\\..\\..\\Repository\\RepositoryStorage\\";
        string ToSendPath = "..\\..\\..\\Repository\\RepositoryStorage\\";
        public Comm<Repository> comm { get; set; } = new Comm<Repository>();
        public string endPoint { get; } = Comm<Repository>.makeEndPoint("http://localhost", 8082);
        //IStreamService channel;
        int BlockSize = 1024;
        byte[] block;
        //HiResTimer hrt = null;
        private Thread rcvThread = null;

        public Repository()
        {
            
            //Console.WriteLine(" * ***********************************Repository**********************************");
        }
        
        public List<string> queryLogs(string queryText)
        {
            List<string> queryResults = new List<string>();
            string path = System.IO.Path.GetFullPath(repoStoragePath);
            string[] files = System.IO.Directory.GetFiles(repoStoragePath, "*.txt");
            foreach (string file in files)
            {
                string contents = File.ReadAllText(file);
                if (contents.Contains(queryText))
                {
                    string name = System.IO.Path.GetFileName(file);
                    queryResults.Add(name);
                }
            }
            queryResults.Sort();
            queryResults.Reverse();
            return queryResults;
        }
        //----< send files with names on fileList >----------------------
        
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
                sendLog(msg);               
            }
        }

        

        public bool getFiles(string path, string fileList)
        {
            string[] files = fileList.Split(new char[] { ',' });
            //string repoStoragePath = "..\\..\\RepositoryStorage\\";

            foreach (string file in files)
            {
                string fqSrcFile = repoStoragePath + file;
                string fqDstFile = "";
                try
                {
                    fqDstFile = path + "\\" + file;
                    File.Copy(fqSrcFile, fqDstFile);
                }
                catch
                {
                    Console.Write("\n  could not copy \"" + fqSrcFile + "\" to \"" + fqDstFile);
                    return false;
                }
            }
            return true;
        }
       
        public void sendLog(Messages msg)
        {
            Console.WriteLine("------------------------Requirement 9 and 10 - Line 128 Repository.cs---------------");
            Console.WriteLine("Responding to client queries");
            try
            {
                string queryText = msg.body;
                List<string> queryResults = queryLogs(queryText);
                Messages logMessage = new Messages();
                logMessage.author = msg.author;
                logMessage.from = "Repo";
                logMessage.to = msg.from;
                logMessage.fileList = queryResults;
                comm.sndr.PostMessage(logMessage);
            }
            catch(Exception e)
            {
                Console.WriteLine("In Repository" + e.Message);
            }
        }

        public void upLoadFile(FileTransferMessage msg)
        {
            int totalBytes = 0;
            block = new byte[BlockSize];
            HiResTimer hrt1 = new HiResTimer();
            hrt1.Start();
            filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    totalBytes += bytesRead;
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            hrt1.Stop();
            Console.Write(
              "\n  Received file \"{0}\" of {1} bytes in {2} microsec.",
              filename, totalBytes, hrt1.ElapsedMicroseconds
            );
        }

        public Stream downLoadFile(string filename)
        {
            HiResTimer hrt = new HiResTimer();
            hrt.Start();
            string sfilename = Path.Combine(ToSendPath, filename);
            FileStream outStream = null;
            if (File.Exists(sfilename))
            {
                outStream = new FileStream(sfilename, FileMode.Open);
            }
            else
                throw new Exception("open failed for \"" + filename + "\"");
            hrt.Stop();
            Console.WriteLine("------------------------Requirement 6 -- Line 176 Repository.cs---------------");
            Console.Write("\n  Sent files \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
            return outStream;
        }
        static ServiceHost CreateServiceChannel(string url)
        {
            
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 50000000;
            Uri baseAddress = new Uri(url);
            Type service = typeof(Repository);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(IStreamService), binding, baseAddress);
            return host;
        }

        static void Main(string[] args)
    {
            try
            {
                Console.Title = "Repository";
                Repository repo = new Repository();
                repo.comm.rcvr.CreateRecvChannel(repo.endPoint);
                repo.rcvThread = repo.comm.rcvr.start(repo.rcvThreadProc);
                ServiceHost host = CreateServiceChannel("http://localhost:8000/StreamService");
                host.Open();
                Console.WriteLine(" \n***********************************Repository**********************************");
                Console.Write("\n  Press key to terminate service:\n");
                Console.ReadKey();
                Console.Write("\n");
                host.Close();
            }
            catch(Exception er)
            {
                Console.WriteLine("In repository" + er.Message);
            }
        }

    }
}
