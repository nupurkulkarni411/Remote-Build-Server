/////////////////////////////////////////////////////////////////////
// TestHarness.cs - Runs tests by loading dlls and invoking test() //
// ver 2                                                           //
// Language:    C#, Visual Studio 2017                             //
// Platform:    Lenovo ideapad 500, Windows 10                     //
// Application: Remote Build Server                                //
//                                                                 //
// Name : Nupur Kulkarni                                           //
// CSE681: Software Modeling and Analysis, Fall 2017               //
// Author Dr. Jim Fawcett                                          //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This package will load the test drivers and execute tests.
 *
 *  Public Interface:
 * ================= 
 * public void sendLog(string log, string authorName): for sending log to repository
 * public void ProcessRequest(Message msg) : for processing test request
 * receiveFiles(List<string> files): for pulling required drivers from repository
 * public bool LoadTests(string path) : for loading and executing tests
 * public void run() : run all the tests on list made in LoadTests
 * 
 * Build Process:
 * --------------
 * Required Files:  TestHarness.cs BuildMessages.cs Logger.cs ITest.cs Serialization.cs ChannelDemo.cs
 * Compiler command: csc TestHarness.cs BuildMessages.cs Logger.cs ITest.cs Serialization.cs ChannelDemo.cs
 *
 * Maintenance History:
 * Ver 2 
 * - added more error handling in run()
 * - made load error handling more specific, so it will continue
 *   to load tests after a load or creation error
 *   
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommChannelDemo;
using System.Threading;
using Logger;
using BuildServerMessages;
using System.IO;
using System.Reflection;
using Utilities;
#pragma warning disable


namespace LoadingTests
{
    class TestHarness
    {
        private struct TestData
        {
            private string Name;
            private ITest testDriver;
        }
        public Comm<TestHarness> comm2 { get; set; } = new Comm<TestHarness>();
        public string endPoint { get; } = Comm<TestHarness>.makeEndPoint("http://localhost", 8087);

        private Thread rcvThread2 = null;
        private List<Type> testTypes = new List<Type>();
        protected TestLog tl = new TestLog();
        private string log = "";
        private string testPath = "../../../TestStorage";
        public List<string> dllFiles { get; set; } = new List<string>();

        //----< initialize receiver >------------------------------------
        public TestHarness()
        {
            comm2.rcvr.CreateRecvChannel(endPoint);
            rcvThread2 = comm2.rcvr.start(rcvThreadProc);
        }


        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm2.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:", comm2.name);
                msg.showMsg();
                if (msg.body == "quit")
                    break;
                if (msg.type == "TestRequest")
                {
                    ProcessRequest(msg);
                }
            }
        }

        //For sending logs to repository
        public void sendLog(string log, string authorName)
        {
            Random rand = new Random();
            string filename = "TestLog" + authorName + rand.Next(1, 10000).ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            Console.WriteLine("\n   9. Sending test log to repository");
            using (FileStream fs = File.Create(testPath + "//" + filename))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(log);
                // Add log information to the file.
                fs.Write(info, 0, info.Length);
            }
            comm2.sndr.uploadFile(filename, testPath);
        }

        //processing messages sent by other packages to test harness
        public void ProcessRequest(Message msg)
        {
            System.IO.Directory.CreateDirectory(Path.GetFullPath(testPath));
            TestRequest tr = msg.body.FromXml<TestRequest>();
            dllFiles.Clear();
            tl.createLog(tr.author);
            foreach (TestElement te in tr.tests)
            {
                dllFiles.Add(te.testDriver);
            }
            Console.WriteLine("\n  7. Test Harness parses test request and pulls required files from build storage.(Requirement 7)");
            if (receiveFiles(dllFiles))
            {
                if (LoadTests(testPath))
                    run();
                else
                    Console.Write("\n  couldn't load tests");
                sendLog(tl.getLog(), tr.author);
            }
            else
            {
                Console.WriteLine("\n  Files Not received");
            }
        }

        //----< join receive thread >------------------------------------

        public void wait()
        {
            rcvThread2.Join();
        }

        //to pull required drivers from repository 
        public bool receiveFiles(List<string> files)
        {
            CommChannelDemo.Sender clnt = new CommChannelDemo.Sender();
            CommChannelDemo.Sender.channel = CommChannelDemo.Sender.CreateServiceChannel("http://localhost:8000/StreamService");
            foreach (string file in files)
            {
                Console.Write("\n   Receiving \"{0}\" to \"{1}\" using WCF", file, Path.GetFullPath(testPath));
                try
                {
                    int temp = clnt.download(file, testPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n\n        {0}", ex.ToString());
                    return false;
                }
            }
            return true;
        }
        //----< load test dlls to invoke >-------------------------------
        public bool LoadTests(string path)
        {
            testTypes.Clear();
            string[] files = System.IO.Directory.GetFiles(testPath, "*.dll");
            Console.WriteLine("\n  8. Test harness loads all the dlls for testing and runs the tests.(Requirement 9)");
            foreach (string file in files)
            {
                Console.Write("\n  loading: \"{0}\"", file);
                try
                {
                    Assembly assem = Assembly.LoadFrom(file);
                    Type[] types = assem.GetExportedTypes();

                    foreach (Type t in types)
                    {
                        MethodInfo tM = t.GetMethod("test");

                        if (t.IsClass && t.GetInterface("ITest") != null && tM != null && tM.ReturnType == typeof(bool))//typeof(ITest).IsAssignableFrom(t))  // does this type derive from ITest ?
                        {
                            testTypes.Add(t);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // here, in the real testharness you log the error
                    Console.Write("\n  {0}", ex.Message);
                }

            }
            Console.Write("\n");
            return testTypes.Count > 0;   // if we have items in list then Load succeeded
        }

        //----< run all the tests on list made in LoadTests >------------

        public void run()
        {
            Console.WriteLine("\n  Starting logger");
            foreach (Type test in testTypes)
            {
                MethodInfo testMethod = test.GetMethod("test");
                Console.Write("\n  testing {0}", test.Name);
                bool result = (bool)testMethod.Invoke(Activator.CreateInstance(test), null);
                if (result)
                {
                    Console.Write("\n  test passed");
                    log += ("Test Passed");
                }
                else
                {
                    Console.Write("\n  test failed");
                    log += ("Test Failed");
                }
                tl.startLogging(test.Name, result, log);
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Test Harness";
            Console.Write("\n  Test Harness");
            Console.Write("\n =============\n");
            TestHarness testharness = new TestHarness();
            testharness.wait();
            Console.Write("\n\n");
        }
    }
}
