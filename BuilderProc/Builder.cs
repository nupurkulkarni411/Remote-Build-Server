/////////////////////////////////////////////////////////////////////
// Builder.cs -   This package provides core functionality         //
//                of building files for Build Server system        //
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
 * This Package handles the processing of commands sent by other packages 
 * and builds files sent to it by repository the Comm<Builder>
 * class to pass messages to a remote endpoint.
 * 
 * Public Interface:
 * ================= 
 * void ProcessMessage(Message msg) : This enables processing of messages sent by other 
 *                                    packages and takes appropriate actions.
 * void sendLog(string log,string authorName) : Used for sending log file to repository
 * bool receiveFiles(List<string> files) : For pulling required files from repository
 * void Build(string testDriver , List<string> testCodes = null): For building files sent to builder and display output to client
 * rcvThreadPrc1() : Receive thread proc for builder
 * 
 * Build Process:
 * --------------
 * Required Files: BuilderMessages.cs Logger.cs Messages.cs Serialization.cs ChannelDemo.cs ICommunication.cs
   Build Command: csc Builder.cs BuilderMessages.cs Logger.cs Messages.cs Serialization.cs ChannelDemo.cs ICommunication.cs

 * Maintenance History:
    - Ver 1.0 Oct 2017 
 * --------------------*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommChannelDemo;
using System.Threading;
using Logger;
using BuildServerMessages;
using Utilities;
using System.IO;
using System.Diagnostics;

namespace BuilderProc
{
    class Builder
    {
        public Comm<Builder> comm1 { get; set; } = new Comm<Builder>();
        public string endPoint { get; }

        private Thread rcvThread1 = null;

        private string storagepath;

        protected BuildLog bl = new BuildLog();

        private string testDriverToBeTested;

        private List<string> testFiles { get; set; } = new List<string>();

        //Start listening and sending from the assigned port
        public Builder(int port)
        {
            string endp = Comm<Builder>.makeEndPoint("http://localhost", port);
            endPoint = endp;
            storagepath = "..\\..\\..\\BuildStorage" + port.ToString();
            comm1.rcvr.CreateRecvChannel(endPoint);
            rcvThread1 = comm1.rcvr.start(rcvThreadPrc1);
        }

        public void wait()
        {
            rcvThread1.Join();
        }
        //thread proc of the builder
        private void rcvThreadPrc1()
        {
            while (true)
            {
                Message msg = comm1.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message from {1}:", comm1.name,msg.from);
                msg.showMsg();
                if (msg.type == "BuildRequest")
                {
                    ProcessRequest(msg);
                    Message msg2 = new Message();
                    msg2.from = endPoint;
                    string remoteEndPoint = "http://localhost:8080/ICommunicator";
                    msg2.to = remoteEndPoint;
                    msg2.type = "Ready";
                    comm1.sndr.PostMessage(msg2);
                    Console.Write("\n   Sending message from {0} to {1}", comm1.name, msg2.to);
                    msg2.showMsg();
                }
                if (msg.type == "quit")
                {
                    Console.Write("\n  Quitting endpoint {0}", endPoint);
                    break;
                }
            }
        }

        //This enables processing of messages sent by other packages and takes appropriate actions
        public void ProcessRequest(Message msg)
        {
            TestRequest tr = new TestRequest();
            BuildRequest newTrq = msg.body.FromXml<BuildRequest>();
            tr.author = newTrq.author;
            tr.timeStamp = DateTime.Now;
            System.IO.Directory.CreateDirectory(Path.GetFullPath(storagepath));
            bl.createLog(newTrq.author);
            foreach (TestElement tl in newTrq.tests)
            {
                testDriverToBeTested = null;
                TestElement t = new TestElement();
                t.testName = tl.testName;
                t.testConfiguration = tl.testConfiguration;
                if (tl.testConfiguration == "C#")
                    t.testDriver = Path.GetFileNameWithoutExtension(tl.testDriver) + ".dll";
                if (tl.testConfiguration == "Java")
                    t.testDriver = Path.GetFileNameWithoutExtension(tl.testDriver) + ".jar";
                testFiles.Clear();
                testFiles.Add(tl.testDriver);
                testFiles.AddRange(tl.testCodes);
                Console.WriteLine("\n  1. Builder parses the message and gets test request \n  then builder parses the test request and pulls required files from repository storage.");
                if (receiveFiles(testFiles))
                {
                    Console.WriteLine("\n  2. Builder tries to build files send to it. (Requirement 8)");
                    Build(tl.testDriver, tl.testConfiguration, tl.testCodes);
                    if(testDriverToBeTested != null)
                    {
                        tr.tests.Add(t);
                    }
                }
                else
                {
                    Console.WriteLine("\n  Files not received");
                }
            }
            if (tr.tests.Any())
            {
                sendTestRequest(tr);
            }
            sendLog(bl.getLog(), newTrq.author);
        }

        private void sendTestRequest(TestRequest tr)
        {
            Console.WriteLine("\n   Builder sends Test request XML to Test Harness (Requirement 8)");
            Message msg = new Message();
            msg.to = Comm<Builder>.makeEndPoint("http://localhost", 8087);
            msg.from = endPoint;
            msg.body = tr.ToXml();
            msg.type = "TestRequest";
            comm1.sndr.PostMessage(msg);
        }

        private void sendLog(string log, string author)
        {
            Random rand = new Random();
            string filename = "BuildLog" + author + rand.Next(1, 10000).ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            Console.WriteLine("\n  5. Sending created log to repository.");
            using (FileStream fs = File.Create(storagepath + "//" + filename))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(log);
                // Add log information to the file.
                fs.Write(info, 0, info.Length);
            }
            comm1.sndr.uploadFile(filename, storagepath);
        }

        //Builds files sent by repository
        public void Build(string testDriver, string testConfiguration, List<string> testCodes = null)
        {
            try
            {   //Creating background window of command prompt and firing build command
                Console.WriteLine("\n  Start Logging (Requirement 8)");
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe"; p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (testCodes != null)
                {
                    string codes = "";
                    foreach (string test in testCodes)
                    {
                        codes = codes + " " + test;
                    }
                    if (testConfiguration == "C#")
                        p.StartInfo.Arguments = "/Ccsc /warnaserror /target:library " + testDriver + " " + codes;
                    if (testConfiguration == "Java")
                        p.StartInfo.Arguments = "/Cjavac " + testDriver;
                }
                else
                {
                    if (testConfiguration == "C#")
                        p.StartInfo.Arguments = "/Ccsc /warnaserror /target:library " + testDriver;
                    if (testConfiguration == "Java")
                        p.StartInfo.Arguments = "/Cjavac " + testDriver;
                }
                Console.Write("\n  Build Command: {0}", p.StartInfo.Arguments);
                p.StartInfo.WorkingDirectory = storagepath;
                p.StartInfo.RedirectStandardError = true; p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.Start();
                p.WaitForExit();
                string time = p.TotalProcessorTime.ToString(); string errors = p.StandardError.ReadToEnd();
                string output = p.StandardOutput.ReadToEnd(); Console.Write("\n\n"); Console.Write("\n  Output:\n{0}", output + errors);
                if (errors == "" && !output.Contains("error"))
                {
                    Console.Write("\n  Build Successful.");
                    comm1.sndr.uploadFile(Path.GetFileNameWithoutExtension(testDriver) + ".dll", storagepath);
                    testDriverToBeTested = Path.GetFileNameWithoutExtension(testDriver) + ".dll";
                }
                else
                    Console.Write("\n  Build Failure");
                Console.Write("\n  Execution Time: {0}", time);
                bl.startLogging(errors, output, time, testDriver);
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}", ex.Message);
            }
        }

        //this method is used for downloading files in build storage from repository storage using WCF
        public bool receiveFiles(List<string> files)
        {
            Console.WriteLine("\n   Builder receives test code files and test driver from repository storage to the build storage.\n   (Builder already has test request received from repository as a part of message body)");
            foreach (string file in files)
            {
                Console.Write("\n   Receiving \"{0}\" to \"{1}\" using WCF", file, Path.GetFullPath(storagepath));
                try
                {
                    int temp = comm1.sndr.download(file, storagepath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n\n        {0}", ex.ToString());
                    return false;
                }

            }
            return true;
        }

        static void Main(string[] args)
        {

            Console.Title = "ChildBuilderProc";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("\n Child  Builder Process");
            Console.Write("\n ======================");

            if (args.Count() == 0)
            {
                Console.Write("\n  please enter integer value on command line");
                return;
            }
            else
            {
                Builder child = new Builder(Convert.ToInt32(args[0]));
                Console.Write("\n  Hello from child #{0}\n\n", args[0]);
                Message msg1 = new Message();
                msg1.from = child.endPoint;
                msg1.to = "http://localhost:8080/ICommunicator";
                msg1.type = "Ready";
                child.comm1.sndr.PostMessage(msg1);
                Console.Write("\n   Sending message from {0} to {1}", child.comm1.name, msg1.to);
                msg1.showMsg();
            }
            Console.ReadKey();
            Console.Write("\n  ");
        }
    }
}
