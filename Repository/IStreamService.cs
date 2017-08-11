/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  IStreamService.cs                                                      //
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
 *   WCF StreamService in Self Hosted Configuratio
 *    
 *    
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   IStreamService.cs
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
using System.Threading.Tasks;

namespace Remote_TestHarness
{
    [ServiceContract(Namespace = "http://Remote_TestHarness")]
    public interface IStreamService
    {
        [OperationContract(IsOneWay = true)]
        void upLoadFile(FileTransferMessage msg);
        [OperationContract]
        Stream downLoadFile(string filename);
    }

    [MessageContract]
    public class FileTransferMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
}
