/////////////////////////////////////////////////////////////////////////////
//  Remote Test Harness - Project 4                                        //
//  MainWindow.xaml.cs - demonstrates that the project implements a WPF    //
//                       client and that it meets all the requirements     //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Demonstration for CSE681 - Software Modeling & Analysis  //
//  Author:       Varun Krishna Peruru Vanaparthy                          //
//                vperuruv@syr.edu                                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This package implements a WPF client that demonstrates in part the various 
 *   requirements of the test harness.
 *     
 *    
 *   Public Interface
 *   ----------------
 *   MainWindow mw = new MainWindow();
 *   mw.displayMessage();
 *   string filePath = args[0];
 *   mw.SendFiles(filePath)
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   MainWindow.xaml.cs, MainWindow.xaml
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
using System.Windows;
using System.Xml.Linq;

namespace Remote_TestHarness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string requestFileName { get; set; }
        public string srcPath { get; set; }//Path for test driver and test code dll files
        public Comm<MainWindow> comm { get; set; } = new Comm<MainWindow>();
        public string endPoint { get; set; }// = Comm<MainWindow>.makeEndPoint("http://localhost", 8091);
        private Thread rcvThread = null;
        public BlockingQueue<string> inQ_ { get; set; }
        private ITestHarness th_ = null;
        private IRepository repo_ = null;
        IStreamService channel;
        HiResTimer hrt = null;
        delegate void NewMessage(Messages msg);
        event NewMessage OnNewMessage;
        int MaxMsgCount = 50;
        int fileChannelCreated = 0;
        int sel = 0;

        public MainWindow()
        {
            Console.WriteLine("***************************************CLIENT 1***********************************");
            InitializeComponent();
            OnNewMessage += new NewMessage(OnNewMessageHandler);
            LoadDropdownMenu();
            Console.Title = "Client 1";
        }

        public void LoadDropdownMenu()
        {
            string filePath = "..\\..\\..\\Client3\\TestRequests\\";
            DirectoryInfo newDir = new DirectoryInfo(filePath);
            FileInfo[] files = newDir.GetFiles();
            Request1.Header = files[0].Name;
            Request2.Header = files[1].Name;
        }

        public void Request1_Click(object sender, RoutedEventArgs e)
        {
            //listBox1.Items.Clear();
            //listBox2.Items.Clear();
            SendButton.IsEnabled = false;
            RequestMenu.Header = Request1.Header;
            requestFileName = "..\\..\\..\\Client3\\TestRequests\\" + Request1.Header.ToString();
            srcPath = "..\\..\\..\\TestDriver3\\bin\\Debug\\";
            sel = 3;
            ListenButton.IsEnabled = true;
        }

        public void Request2_Click(object sender, RoutedEventArgs e)
        {
            //listBox1.Items.Clear();
            //listBox2.Items.Clear();
            SendButton.IsEnabled = false;
            RequestMenu.Header = Request2.Header;
            requestFileName = "..\\..\\..\\Client3\\TestRequests\\" + Request2.Header.ToString();
            srcPath = "..\\..\\..\\TestDriver4\\bin\\Debug\\";
            sel = 4;
            ListenButton.IsEnabled = true;
        }

        public void ListenButton_Click(object sender, RoutedEventArgs e)
        {
            LocalPortTextBox.IsEnabled = false;
            string localPort = LocalPortTextBox.Text;
            endPoint = "http://localhost:" + localPort + "/ICommunicator";
            RequestMenu.IsEnabled = false;
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
            SendButton.IsEnabled = true;
            QueryButton.IsEnabled = true;
        }

        public void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            ListenButton.IsEnabled = false;
            //Send Files to Repository
            HiResTimer hrt = new HiResTimer();
            hrt.Start();
            SendFiles();
            hrt.Stop();
            Console.Write(
              "\n\n  Total elapsed time for uploading = {0} microsec.\n",
              hrt.ElapsedMicroseconds
            );
            Messages msg = MakeMessage(endPoint, endPoint, requestFileName);
            string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8080);
            msg = msg.copy();
            msg.to = remoteEndPoint;
            Console.WriteLine("\n------------------------Requirement 10 - Line 143 MainWindow.xaml.cs---------------");
            Console.WriteLine("Sending message using WCF");
            comm.sndr.PostMessage(msg);
            displayMessage(msg);
            Thread.Sleep(2000);
            RequestMenu.IsEnabled = true;

        }

        public void displayMessage(Messages msg)
        {
            try
            {
                int i = 0;
                listBox1.Items.Insert(i, "Displaying New message");
                i++;
                listBox1.Items.Insert(i, "-----------------------");
                string[] lines = msg.ToString().Split(new char[] { ',' });
                foreach (string line in lines)
                {
                    i++;
                    if (!line.Contains("body"))
                    {
                        listBox1.Items.Insert(i, line.Trim());
                        Console.WriteLine(line.Trim());
                        //Console.WriteLine("\n");
                    }

                }
                XDocument xdoc = XDocument.Parse(msg.body);
                listBox1.Items.Insert(i, xdoc);
                Console.WriteLine(xdoc);
                //Console.WriteLine("\n");
                if (listBox1.Items.Count > MaxMsgCount)
                    listBox1.Items.RemoveAt(listBox1.Items.Count - 1);

            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 100;
                temp.Width = 500;
            }
        }

        public Messages MakeMessage(string fromEndPoint, string toEndPoint, string filePath)
        {
            Messages msg = new Messages();
            msg.author = "Varun Krishna";
            msg.type = "TestRequest";
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            msg.body = XmlParser.XmltoString(filePath);
            return msg;
        }

        public Messages MakeRepoMessage(string author, string fromEndPoint, string toEndPoint, string testName)
        {
            Messages msg = new Messages();
            msg.author = author;
            msg.type = "Query";
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            msg.body = testName;
            return msg;
        }

        public void SendFiles()
        {
            try
            {
                fileChannelCreated = 1;
                //string destPath = "..\\..\\..\\Repository\\RepositoryStorage";
                //string srcPath1 = "..\\..\\..\\TestDriver\\TestDriver.dll";
                int BlockSize = 1024;
                byte[] block;
                HiResTimer hrt = new HiResTimer();
                block = new byte[BlockSize];
                channel = CreateServiceChannel("http://localhost:8000/StreamService");
                hrt.Start();
                if (sel == 3)
                {
                    UploadFile("TestDriver3.dll");
                    UploadFile("TestCode3.dll");
                }
                if (sel == 4)
                {
                    UploadFile("TestDriver4.dll");
                    UploadFile("TestCode4.dll");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("In Client3" + e.Message);
            }
            //hrt = new HRTimer.HiResTimer();
        }

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

        void UploadFile(string filename)
        {
            //string srcPath = "..\\..\\..\\TestDriver2\\bin\\Debug\\";
            string fqname = System.IO.Path.Combine(srcPath, filename);
            HiResTimer hrt = new HiResTimer();
            try
            {
                hrt.Start();
                using (var inputStream = new FileStream(fqname, FileMode.Open))
                {
                    FileTransferMessage msg = new FileTransferMessage();
                    msg.filename = filename;
                    msg.transferStream = inputStream;
                    channel.upLoadFile(msg);
                }
                hrt.Stop();
                Console.WriteLine("\n------------------------Requirement 2 and 6 -- Line 257 MainWindow.cs---------------");
                Console.Write(" Uploaded file to Repository \"{0}\" in {1} microsec.", filename, hrt.ElapsedMicroseconds);
                
            }
            catch (Exception e)
            {
                Console.Write("\n  can't find \"{0}\"", fqname);
            }
        }
        public void rcvThreadProc()
        {
            while (true)
            {
                Messages msg = comm.rcvr.GetMessage();
                this.Dispatcher.BeginInvoke(
         System.Windows.Threading.DispatcherPriority.Normal,
         OnNewMessage,
         msg);
               
            }
        }

        void OnNewMessageHandler(Messages msg)
        {
            msg.time = DateTime.Now;
            Console.Write("\n  {0} received message:", comm.name);
            if (msg.from == "TH")
            {
                if (msg.body.Contains("File Error"))
                {
                    Console.WriteLine("\nFile download error - Requirement 3 - Line 303 MainWindow.xaml.cs");
                    listBox2.Items.Insert(0, "Displaying New message");
                    string[] lines = msg.ToString().Split(new char[] { ',' });
                    foreach (string line in lines)
                    {
                        listBox2.Items.Insert(1, line.Trim());
                        Console.WriteLine(line.Trim());
                        //Console.WriteLine("\n");
                    }
                }
                else
                {
                    int i = 0;
                    listBox2.Items.Insert(i, "Displaying New message");
                    i++;
                    listBox2.Items.Insert(i, "-----------------------");
                    string[] lines = msg.ToString().Split(new char[] { ',' });
                    foreach (string line in lines)
                    {
                        i++;
                        if (!line.Contains("body"))
                        {
                            listBox2.Items.Insert(i, line.Trim());
                            Console.WriteLine(line.Trim());
                            //Console.WriteLine("\n");
                        }
                    }
                    XDocument xdoc = XDocument.Parse(msg.body);
                    listBox2.Items.Insert(i, xdoc);
                    Console.WriteLine(xdoc);
                    //Console.WriteLine("\n");
                    if (listBox2.Items.Count > MaxMsgCount)
                        listBox2.Items.RemoveAt(listBox2.Items.Count - 1);

                    //listBox2.Items.Insert(0, "Displaying New message");
                    //listBox2.Items.Insert(1, msg);
                    //if (listBox2.Items.Count > MaxMsgCount)
                    //listBox2.Items.RemoveAt(listBox2.Items.Count - 1);
                }
            }
            if (msg.from == "Repo")
            {
                listBox2.Items.Insert(0, "Files in Repository");
                List<string> files = msg.fileList;
                Console.Write("\n  first 5 reponses to query \"Test 1\"");
                for (int i = 0; i < 5; ++i)
                {
                    if (i == files.Count())
                        break;
                    Console.Write("\n  " + files[i]);
                    listBox2.Items.Insert(1, files[i]);
                    if (listBox2.Items.Count > MaxMsgCount)
                        listBox2.Items.RemoveAt(listBox2.Items.Count - 1);
                }

                Console.WriteLine();
                if (files.Count != 0)
                {
                    download(files[0]);
                    Console.WriteLine("Downloading the latest file to ../../../Client3/Logs");
                    //download(files[1]);
                }
            }
            //if (msg.body == "quit")
                //break;
            
        }

        void download(string filename)
        {
            int BlockSize = 1024;
            byte[] block = new byte[BlockSize];
            string SavePath = "..\\..\\..\\Client3\\Logs";
            int totalBytes = 0;
            HiResTimer hrt = new HiResTimer();
            try
            {
                hrt.Start();
                Stream strm = channel.downLoadFile(filename);
                string rfilename = System.IO.Path.Combine(SavePath, filename);
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);
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
                hrt.Stop();
                ulong time = hrt.ElapsedMicroseconds;
                Console.Write("\n  Received file \"{0}\" of {1} bytes in {2} microsec.", filename, totalBytes, time);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
            }
        }

        private void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            if(fileChannelCreated == 0)
                channel = CreateServiceChannel("http://localhost:8000/StreamService");
            //listBox1.Items.Clear();
            //listBox2.Items.Clear();
            string localPort = LocalPortTextBox.Text;
            endPoint = "http://localhost:" + localPort + "/ICommunicator";
            Messages repoMsg = MakeRepoMessage("Varun Krishna", endPoint, endPoint, "Third Test");
            string repoEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8082);
            repoMsg = repoMsg.copy();
            repoMsg.to = repoEndPoint;
            Console.WriteLine("\n------------------------Requirement 10 - Line 419 MainWindow.xaml.cs---------------");
            Console.WriteLine("Querying repository using WCF");
            comm.sndr.PostMessage(repoMsg);
            try
            {
                listBox1.Items.Insert(0, repoMsg);
                if (listBox1.Items.Count > MaxMsgCount)
                    listBox1.Items.RemoveAt(listBox1.Items.Count - 1);

            }
            catch (Exception ex)
            {
                Window temp = new Window();
                temp.Content = ex.Message;
                temp.Height = 100;
                temp.Width = 500;
            }
        }
    }
}
