/////////////////////////////////////////////////////////////////////
// ProcessPool.cs - Define functionality for creating pool of      //
//                  processes                                      //
// ver 1.0                                                         //
// Language:    C#, Visual Studio 2017                             //
// Platform:    Lenovo ideapad 500, Windows 10                     //
// Application: Remote Build Server                                //
//                                                                 //
// Name : Nupur Kulkarni                                           //
// CSE681: Software Modeling and Analysis, Fall 2017               //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This package generates specified number of child processes.
 *
 *  Public Interface:
 * ================= 
 * static bool createProcess(int port): function for creating new child process
 * public List<int> processPool(int count): function for generating specified number of prcesses
 * 
 * Build Process:
 * --------------
 * Required Files:  ProcessPool.cs
 * Compiler command: csc ProcessPool.cs
 *
 * Maintenance History:
    - Ver 1.0 Oct 2017 
    - Ver 2.0 Dec 2017 
 * --------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessPool
{
    public class ProcessPool
    {
        static private List<int> portsCreated = new List<int>();
        //Function for creating new child process
        static bool createProcess(int port)
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\BuilderProc\\bin\\debug\\BuilderProc.exe";
            string absFileSpec = Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            portsCreated.Add(port);
            string commandline = port.ToString();
            try
            {
                Process.Start(fileName, commandline);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }
        //to create specified count of processes
        public List<int> processPool(int count)
        {
            for (int i = 1; i <= count; ++i)
            {
                if (createProcess(8090 + i))
                {
                    Console.Write(" - succeeded");
                }
                else
                {
                    Console.Write(" - failed");
                }
            }
            return portsCreated;
        }
#if (TEST_POOLPROCESS)
        static void Main(string[] args)
        {
            Console.Write("\n   demo of process pool");
            Console.Write("\n   ==============================");
            ProcessPool repo = new ProcessPool();
            repo.createprocess(2);
            
            Console.Write("\n\n");
        }
#endif
    }
}
