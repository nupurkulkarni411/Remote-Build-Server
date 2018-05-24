//////////////////////////////////////////////////////////////////////
// MotherBuilder.cs -   This package provides functionality for     //
//                      creating process pool components on command //
//                      and manage execution of build requests by   //
//                      sending those to ready child processes.     //
// ver 1.0                                                          //
// Language:    C#, Visual Studio 2017                              //
// Platform:    Lenovo ideapad 500, Windows 10                      //
// Application: Remote Build Server                                 //
//                                                                  //
// Author Name : Nupur Kulkarni                                     //
// Source: Dr. Jim Fawcett                                          //
// CSE681: Software Modeling and Analysis, Fall 2017                //
//////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This Package handles the processing of commands sent by client package uses the Comm<MotherBuilder>
 * class to pass messages to a remote endpoint.
 * and manage execution of build requests by sending those to ready child processes.
 * 
 * Public Interface:
 * ================= 
 * MotherBuilder(): Constructor
 * createProcess(int port) : Function for creating new child process  
 * makeMessage(string author, string fromEndPoint, string toEndPoint) : to create a comm message
 * rcvThreadProc(): To check the messages sent by other modules to mother builder
 * 
 * Build Process:
 * --------------
 * Required Files: CS-BlockingQueue.cs Communication.cs Messages.cs Serialization.cs 
   Build Command: csc MotherBuilder.cs CS-BlockingQueue.cs Communication.cs Messages.cs Serialization.cs 

 * Maintenance History:
    - Ver 1.0 Oct 2017
    - Ver 2.0 Dec 2017
 * --------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommChannelDemo;
using System.Threading;
using SWTools;
using System.Diagnostics;
using System.IO;
using ProcessPool;

namespace MotherBuilder
{
    public class MotherBuilder
    {
        public Comm<MotherBuilder> comm { get; set; } = new Comm<MotherBuilder>();

        //readyQ will hold the ready messages sent by child processes to mother builder
        public static SWTools.BlockingQueue<Message> readyQ { get; set; } = null;

        //testRequestQ holds test request messages sent by the client
        public static SWTools.BlockingQueue<Message> testRequestQ { get; set; } = null;

        public string endPoint { get; } = Comm<MotherBuilder>.makeEndPoint("http://localhost", 8080);

        private Thread rcvThread = null;
        static private List<int> portsCreated;

        //Constructor
        public MotherBuilder()
        {
            if (readyQ == null)
                readyQ = new SWTools.BlockingQueue<Message>();
            if (testRequestQ == null)
                testRequestQ = new SWTools.BlockingQueue<Message>();
            comm.rcvr.CreateRecvChannel(endPoint);
            //this thread constantly checks that are there any pending build requests and any ready 
            // child process to which mother builder can assign next pending build request 
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    if (readyQ.size() != 0 && testRequestQ.size() != 0)
                    {
                        Message ready = readyQ.deQ();
                        Message testr = testRequestQ.deQ();
                        testr.to = ready.from;
                        testr.from = endPoint;
                        comm.sndr.PostMessage(testr);
                        Console.Write("\n   Sending message from {0} to {1}", comm.name, testr.to);
                        testr.showMsg();
                    }
                }
            });
            t.Start();

            rcvThread = comm.rcvr.start(rcvThreadProc);
        }

        public void wait()
        {
            rcvThread.Join();
        }
        //to create a comm message
        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }

        //To check the messages sent by other modules to mother builder
        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message from {1} :", comm.name,msg.from);
                msg.showMsg();
                if (msg.type == "Ready")
                    readyQ.enQ(msg);
                if (msg.type == "BuildRequest")
                    testRequestQ.enQ(msg);
                if (msg.type == "quit")
                {
                    Console.Write("\n  Received Quit message from client");
                    Console.Write("\n  Showing Requirement 7 by sending quit messages to all children");
                    foreach(int p in portsCreated)
                    {
                        Message m = new Message();
                        m.to = "http://localhost:" + p + "/ICommunicator";
                        m.from = endPoint;
                        m.type = "quit";
                        comm.sndr.PostMessage(m);
                        Console.Write("\n   Sending message from {0} to {1}", comm.name, m.to);
                        m.showMsg();
                    }
                    Console.Write("\n  Quitting endpoint {0}", endPoint);
                    break;
                }
            }
        }
        static void Main(string[] args)
        {
            Console.Write("\n  Mother Builder");
            Console.Write("\n ================\n");

            Console.Title = "MotherBuilder";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            MotherBuilder Server = new MotherBuilder();
            Console.Write("\n  Requirement2: Using a Message-Passing Communication Service built with WCF\n");

            Console.Write("\n  Requirement 5: This module provides a Process Pool component that creates a specified number of processes on command\n");

            ProcessPool.ProcessPool pp = new ProcessPool.ProcessPool();
            if (args.Count() == 0)
            {
                Console.Write("\n  please enter number of processes to create on command line");
                return;
            }
            else
            {
                int count = Int32.Parse(args[0]);
                if(count > 10 || count < 0)
                {
                    Console.WriteLine("\n  Please enter a value between 1 to 10");
                    return;
                }
                portsCreated = pp.processPool(count);
            }
            
            Message msg = Server.makeMessage("Nupur", Server.endPoint, Server.endPoint);
            Console.ReadKey();
            Console.Write("\n\n");
        }
    }
}
