/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  Messages.cs                                                            //
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
 *   Messages provides helper code for building and parsing XML messages.
 *    
 *   Public Interface
 *   ----------------
 *   Message msg = new Message();
 *   msg.showMsg();
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   Messages.cs
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
using System.Xml.Linq;

namespace Remote_TestHarness
{
    [Serializable]
    public class Messages
    {
        public string type { get; set; } = "default";
        public string to { get; set; }
        public string from { get; set; }
        public string author { get; set; } = "";
        public DateTime time { get; set; } = DateTime.Now;
        public string body { get; set; } = "";
        public List<string> fileList { get; set; }
        public Messages(string bodyStr = "")
        {
            body = bodyStr;
        }

        public static List<Messages> MakeMessage(string fileName)
        {
            List<XmlParser> newXml = new List<XmlParser>();
            List<Messages> messageList = new List<Messages>();
            //XmlParser xml = new XmlParser();
            newXml = XmlParser.ParseXml(fileName);
            Messages newMessage = new Messages();
            foreach(var j in newXml)
            {
                newMessage.author = j.author;
                newMessage.to = j.toURL;
                newMessage.from = j.fromURL;
                newMessage.time = j.timeStamp;
                newMessage.body = j.testName + '~' + j.testDriver + '~' + j.testCode;
                messageList.Add(newMessage);
            }
            return messageList;
        }

        /*public static Messages BuildMessage(string to, string from, string fileName)
        {

        }*/
        public override string ToString()
        {
            string temp = "type: " + type;
            temp += ", to: " + to;
            temp += ", from: " + from;
            if (author != "")
                temp += ", author: " + author;
            temp += ", time: " + time;
            temp += ", body:\n" + body;
            return temp;
        }
        public Messages copy()
        {
            Messages temp = new Messages();
            temp.type = type;
            temp.to = to;
            temp.from = from;
            temp.author = author;
            temp.time = DateTime.Now;
            temp.body = body;
            return temp;
        }

        static void Main(string[] args)
        {
            
        }
    }

    public static class extMethods
    {
        public static void showMsg(this Messages msg)
        {
            Console.Write("\n  formatted message:");
            string[] lines = msg.ToString().Split(new char[] { ',' });
            foreach (string line in lines)
            {
                if(!line.Contains("body"))
                    Console.Write("\n    {0}", line.Trim());
            }
            try
            {
                XDocument xdoc = XDocument.Parse(msg.body);
                Console.WriteLine(xdoc);
            }
            catch(Exception e)
            {
                Console.WriteLine("In query logs" + e.Message);
            }
            Console.WriteLine();
        }

        public static string shift(this string str, int n = 2)
        {
            string insertString = new string(' ', n);
            string[] lines = str.Split('\n');
            for (int i = 0; i < lines.Count(); ++i)
            {
                lines[i] = insertString + lines[i];
            }
            string temp = "";
            foreach (string line in lines)
                temp += line + "\n";
            return temp;
        }

        public static string showThis(this object msg)
        {
            string showStr = "\n  formatted message:";
            string[] lines = msg.ToString().Split('\n');
            foreach (string line in lines)
                showStr += "\n    " + line.Trim();
            showStr += "\n";
            return showStr;
        }

#if (TEST_MESSAGES)
    static void Main(string[] args)
    {
      Console.Write("\n  Testing Message Class");
      Console.Write("\n =======================\n");

      Message msg = new Message();
      msg.to = "http://localhost:8080/ICommunicator";
      msg.from = "http://localhost:8081/ICommunicator";
      msg.author = "Fawcett";
      msg.type = "TestRequest";
      msg.showMsg();
      Console.Write("\n\n");
    }
#endif
    }
}
