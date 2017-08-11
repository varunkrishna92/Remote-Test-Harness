/////////////////////////////////////////////////////////////////////////////
//  Remote Test Harness - Project 4                                        //
//  CommChannel.cs                                                         //
//                                                                         //
//  ver 1.0                                                                //
//  Language:     C#, VS 2015                                              //
//  Application:  Demonstration for CSE681 - Software Modeling & Analysis  //
//  Source:       Professor Jim Fawcett                                    //
//  Author:       Varun Krishna Peruru Vanaparthy                          //
//                vperuruv@syr.edu                                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   The ChannelDemo package defines one class, ChannelDemo, that uses 
 * the Comm<Client> and Comm<Server> classes to pass messages to one 
 * another.     
 *    
 *   Public Interface
 *   ----------------
 *   rcvThreadProc();
 * 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   ChannelDemo.cs
 *   - ICommunicator.cs, CommServices.cs
 *   - Messages.cs, MessageTest, Serialization
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
using System.Threading;
using System.Threading.Tasks;

namespace Remote_TestHarness
{
    class CommChannel<T>
    {
        public Comm<T> comm { get; set; } = new Comm<T>();
        public string name { get; set; } = typeof(T).Name;

        public void rcvThreadProc()
        {
            Messages msg = new Messages();
            while (true)
            {
                msg = comm.rcvr.GetMessage();
                Console.Write("\n  getting message on rcvThread {0}", Thread.CurrentThread.ManagedThreadId);
                if (msg.type == "TestRequest")
                {
                    TestRequest tr = msg.body.FromXml<TestRequest>();
                    if (tr != null)
                    {
                        Console.Write(
                          "\n  {0}\n  received message from:  {1}\n{2}\n  deserialized body:\n{3}",
                          msg.to, msg.from, msg.body.shift(), tr.showThis()
                          );
                        if (msg.body == "quit")
                            break;
                    }
                }
                else
                {
                    Console.Write("\n  {0}\n  received message from:  {1}\n{2}", msg.to, msg.from, msg.body.shift());
                    if (msg.body == "quit")
                        break;
                }
            }
            Console.Write("\n  receiver {0} shutting down\n", msg.to);
        }
#if (TEST_COMMCHANNNEL)
         class TestDemoChannel
  { 
    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating Project #4 Channel Prototype");
      Console.Write("\n ============================================\n");

      ChannelDemo<Client_1> demo1 = new ChannelDemo<Client_1>();

      string sndrEndPoint1 = Comm<Client_1>.makeEndPoint("http://localhost", 8080);
      string rcvrEndPoint1 = Comm<TestHarness>.makeEndPoint("http://localhost", 8080);
      demo1.comm.rcvr.CreateRecvChannel(rcvrEndPoint1);
      Thread rcvThread1 = demo1.comm.rcvr.start(demo1.rcvThreadProc);
      Console.Write("\n  rcvr thread id = {0}", rcvThread1.ManagedThreadId);
      Console.WriteLine();      
    }
#endif
    }
}
