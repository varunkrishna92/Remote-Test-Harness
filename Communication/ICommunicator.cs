/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  ICommunicator.cs                                                       //
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
 *   This package defindes the ICommunicator interface
 *      
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   CommService.cs, ICommunicator
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
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Remote_TestHarness
{
    [ServiceContract] 
    public interface ICommunicator
    {
        [OperationContract(IsOneWay = true)]
        void PostMessage(Messages msg);

        // used only locally so not exposed as service method

        Messages GetMessage();
    }
}
