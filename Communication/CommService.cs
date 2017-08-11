/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  CommService.cs                                                         //
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
 *   This package defindes a Sender class and Receiver class that
 * manage all of the details to set up a WCF channel.
 *    
 *   Public Interface
 *   ----------------
 *    string endPoint = args[0];
 *    comm.rcvr.CreateRecvChannel(endPoint);
 *    comm.rcvr.start(comm.thrdProc);
 *    comm.sndr = new Sender();
 *    comm.sndr.CreateSendChannel(endPoint);
 *    Message msg1 = new Message();
 *    msg1.body = "Message #1";
 *    comm.sndr.PostMessage(msg1);
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   CommService.cs, ICommunicator, BlockingQueue.cs, Messages.cs
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
using System.ServiceModel;
using System.Threading;

namespace Remote_TestHarness
{
    public class Receiver<T> : ICommunicator
    {
        static BlockingQueue<Messages> rcvBlockingQ = null;
        ServiceHost service = null;

        public string name { get; set; }

        public Receiver()
        {
            if (rcvBlockingQ == null)
                rcvBlockingQ = new BlockingQueue<Messages>();
        }

        public Thread start(ThreadStart rcvThreadProc)
        {
            Thread rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            return rcvThread;
        }

        public void Close()
        {
            service.Close();
        }

        
        //  Create ServiceHost for Communication service

        public void CreateRecvChannel(string address)
        {
            try
            {
                WSHttpBinding binding = new WSHttpBinding();
                Uri baseAddress = new Uri(address);
                service = new ServiceHost(typeof(Receiver<T>), baseAddress);
                service.AddServiceEndpoint(typeof(ICommunicator), binding, baseAddress);
                service.Open();
                Console.Write("\n  Service is open listening on {0}", address);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // Implement service method to receive messages from other Peers

        public void PostMessage(Messages msg)
        {
            //Console.Write("\n  service enQing message: \"{0}\"", msg.body);
            rcvBlockingQ.enQ(msg);
        }

        // Implement service method to extract messages from other Peers.
        // This will often block on empty queue, so user should provide
        // read thread.

        public Messages GetMessage()
        {
            Messages msg = rcvBlockingQ.deQ();
            //Console.Write("\n  {0} dequeuing message from {1}", name, msg.from);
            return msg;
        }
    }

    public class Comm<T>
    {
        public string name { get; set; } = typeof(T).Name;

        public Receiver<T> rcvr { get; set; } = new Receiver<T>();

        public Sender sndr { get; set; } = new Sender();

        public Comm()
        {
            rcvr.name = name;
            sndr.name = name;
        }
        public static string makeEndPoint(string url, int port)
        {
            string endPoint = url + ":" + port.ToString() + "/ICommunicator";
            return endPoint;
        }
        //----< this thrdProc() used only for testing, below >-----------

        public void thrdProc()
        {
            while (true)
            {
                Messages msg = rcvr.GetMessage();
                msg.showMsg();
                if (msg.body == "quit")
                {
                    break;
                }
            }
        }
    }

    public class Sender
    {
        public string name { get; set; }

        ICommunicator channel;
        string lastError = "";
        BlockingQueue<Messages> sndBlockingQ = null;
        Thread sndThrd = null;
        int tryCount = 0, MaxCount = 10;
        string currEndpoint = "";

        //----< processing for send thread >-----------------------------

        void ThreadProc()
        {
            tryCount = 0;
            while (true)
            {
                Messages msg = sndBlockingQ.deQ();
                if (msg.to != currEndpoint)
                {
                    currEndpoint = msg.to;
                    CreateSendChannel(currEndpoint);
                }
                while (true)
                {
                    try
                    {
                        channel.PostMessage(msg);
                        Console.Write("\n  posted message from {0} to {1}", name, msg.to);
                        tryCount = 0;
                        break;
                    }
                    catch
                    {
                        Console.Write("\n  connection failed");
                        if (++tryCount < MaxCount)
                            Thread.Sleep(100);
                        else
                        {
                            Console.Write("\n  {0}", "can't connect\n");
                            currEndpoint = "";
                            tryCount = 0;
                            break;
                        }
                    }
                }
                if (msg.body == "quit")
                    break;
            }
        }

        public Sender()
        {
            sndBlockingQ = new BlockingQueue<Messages>();
            sndThrd = new Thread(ThreadProc);
            sndThrd.IsBackground = true;
            sndThrd.Start();
        }

        public void CreateSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            ChannelFactory<ICommunicator> factory
              = new ChannelFactory<ICommunicator>(binding, address);
            channel = factory.CreateChannel();
            Console.Write("\n  service proxy created for {0}", address);
        }

        public void PostMessage(Messages msg)
        {
            sndBlockingQ.enQ(msg);
        }

        public string GetLastError()
        {
            string temp = lastError;
            lastError = "";
            return temp;
        }
#if (TEST_COMMSERVICE)

  class Cat { }
  class TestComm
  {
    [STAThread]
    static void Main(string[] args)
    {
      Comm<Cat> comm = new Comm<Cat>();
      string endPoint = Comm<Cat>.makeEndPoint("http://localhost", 8080);
      comm.rcvr.CreateRecvChannel(endPoint);
      comm.rcvr.start(comm.thrdProc);
      comm.sndr = new Sender();
      comm.sndr.CreateSendChannel(endPoint);
      Message msg1 = new Message();
      msg1.body = "Message #1";
      comm.sndr.PostMessage(msg1);
      Message msg2 = new Message();
      msg2.body = "quit";
      comm.sndr.PostMessage(msg2);
      Console.Write("\n  Comm Service Running:");
      Console.Write("\n  Press key to quit");
      Console.ReadKey();
      Console.Write("\n\n");
    }
#endif
    }
}
