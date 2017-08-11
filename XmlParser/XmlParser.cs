/////////////////////////////////////////////////////////////////////////////
//  Remote Test Harness - Project 4                                        //
//  XmlParser.cs                                                           //
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
 *   This module provides functions to parse the XML files.
 *   The function ParseXml parses the input xml test requests and returns the parsed data in a list.
 *   The function GetResult parses the result so as to obtain an efficient way of storing the results.  
 *    
 *   Public Interface
 *   ----------------
 *   XmlParser xml = new XmlParser();
 *   List<XmlParser> newXml = xml.ParseXml(fileName);
 *   XmlParser resultXml = xml.GetResult(fileName); 
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   XmlParser.cs
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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Remote_TestHarness
{
    [Serializable]
    public class XmlParser
    {
        public string toURL { get; set; }
        public string fromURL { get; set; }
        public string testName { get; set; }
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public string testDriver { get; set; }
        //public string result { get; set; }
        //public string resultTime { get; set; }
        public string testCode { get; set; }

        //Function to parse the xml file
        public static List<XmlParser> ParseXml(string fileName)
        {
            string filePath = fileName;
            FileStream xmlFile = new FileStream(filePath, FileMode.Open);
            XDocument xmlDocument = new XDocument();
            xmlDocument = XDocument.Load(filePath);
            List<XmlParser> testList_ = new List<XmlParser>();
            string author = xmlDocument.Descendants("author").First().Value;
            string toURL = xmlDocument.Descendants("toURL").First().Value;
            string fromURL = xmlDocument.Descendants("fromURL").First().Value;
            XElement[] xTests = xmlDocument.Descendants("test").ToArray();
            int numTests = xTests.Count();
            XmlParser parse = null;

            for (int i = 0; i < numTests; ++i)
            {
                parse = new XmlParser();
                parse.author = author;
                parse.toURL = toURL;
                parse.fromURL = fromURL;
                parse.timeStamp = DateTime.Now;
                parse.testName = xTests[i].Attribute("name").Value;
                parse.testDriver = xTests[i].Element("testDriver").Value;
                parse.testCode = xTests[i].Element("testCode").Value;
                /*IEnumerable<XElement> xtestCode = xTests[i].Elements("library");
                foreach (var xlibrary in xtestCode)
                {
                    parse.testCode.Add(xlibrary.Value);
                }*/
                testList_.Add(parse);
            }
            return testList_;
        }

        public static string XmltoString(string filePath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filePath);
                return doc.OuterXml;
            }
            catch(Exception e)
            {
                Console.WriteLine("In XML Parser" + e.Message);
                return null;
            }
        }
#if (TEST_XmlParser)
        static void Main(string[] args)
        {
            XmlParser xml = new XmlParser();
            string fileName = "../../../TestRequests/XML_testRequest.xml";
            string resultFile = args[0];
            List<XmlParser> newList = ParseXml(fileName);
            XmlParser result = GetResult(resultFile);
        }

#endif
    }
}
