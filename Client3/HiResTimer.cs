/////////////////////////////////////////////////////////////////////////////
//  Test Harness - Project 4                                               //
//  HiResTimer.cs                                                          //
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
 *   This package implements High Resolution Timer using Win32, Performance Counters and .Net Interop 
 *    
 *   Public Interface
 *   ----------------
 *   HiResTimer timer = new HiResTimer();
 *   timer.Start();
 *   timer.Stop();
 *   ulong time = timer.ElapsedMicroseconds
 */
/*
 *   Build Process
 *   -------------
 *   - Required files:   BlockingQueue.cs
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Remote_TestHarness
{
    public class HiResTimer
    {
        protected ulong a, b, f;

        public HiResTimer()
        {
            a = b = 0UL;
            if (QueryPerformanceFrequency(out f) == 0)
                throw new Win32Exception();
        }

        public ulong ElapsedTicks
        {
            get
            { return (b - a); }
        }

        public ulong ElapsedMicroseconds
        {
            get
            {
                ulong d = (b - a);
                if (d < 0x10c6f7a0b5edUL) // 2^64 / 1e6
                    return (d * 1000000UL) / f;
                else
                    return (d / f) * 1000000UL;
            }
        }

        public TimeSpan ElapsedTimeSpan
        {
            get
            {
                ulong t = 10UL * ElapsedMicroseconds;
                if ((t & 0x8000000000000000UL) == 0UL)
                    return new TimeSpan((long)t);
                else
                    return TimeSpan.MaxValue;
            }
        }

        public ulong Frequency
        {
            get
            { return f; }
        }

        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out a);
        }

        public ulong Stop()
        {
            QueryPerformanceCounter(out b);
            return ElapsedTicks;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern
           int QueryPerformanceFrequency(out ulong x);

        [DllImport("kernel32.dll")]
        protected static extern
           int QueryPerformanceCounter(out ulong x);
    }
#if (TEST_TIMER)
    static void main(string args[])
    {
        HiResTimer timer = new HiResTimer();
        timer.Start();
        Console.WriteLine("Testing timer");
        timer.Stop();
        Console.WriteLine("Elapsed Time =" + timer.ElapsedMicroseconds)
    }
#endif
}
