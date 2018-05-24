/////////////////////////////////////////////////////////////////////
// BuilderMessages.cs - This package provides functionality        //
//                      of building messages required              //
//                      for Build Server System                    //
// ver 1.0                                                         //
// Language:    C#, Visual Studio 2017                             //
// Platform:    Lenovo ideapad 500, Windows 10                     //
// Application: Remote Build Server                                //
//                                                                 //
// Name : Nupur Kulkarni                                           //
// CSE681: Software Modeling and Analysis, Fall 2017               //
// Author: Dr. Jim Fowcett                                         //
/////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * -------------------
 * This Package actually declares structures of test request, build results and test results.
 * 
 * Public Interface:
 * ================= 
 * void addDriver(string name): for adding driver to test element
 * void addCode(string name): for adding test code files to test element
 * string ToString(): for converting build result, test result or test request to string
 * void addLog(string logItem): for adding one element in the build log
 * 
 * Build Process:
 * --------------
 * Required Files: BuilderMessages.cs
   Build Command: csc BuilderMessages.cs

 * Maintenance History:
    - Ver 1.0 Oct 2017
    - Ver 2.0 Dec 2017
 * --------------------*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildServerMessages
{

    ///////////////////////////////////////////////////////////////////
    // TestElement and TestRequest classes
    //
    public class TestElement  /* information about a single test */
    {
        public string testName { get; set; }
        public string testDriver { get; set; }

        public string testConfiguration { get; set; }
        public List<string> testCodes { get; set; } = new List<string>();

        public TestElement() { }
        public TestElement(string name)
        {
            testName = name;
        }
        //for adding driver to test element
        public void addDriver(string name)
        {
            testDriver = name;
        }
        //for adding code to be tested to test element
        public void addCode(string name)
        {
            testCodes.Add(name);
        }

        public void addTestConfiguration(string name)
        {
            testConfiguration = name;
        }

        //converting test element to string
        public override string ToString()
        {
            string temp = "\n    test: " + testName;
            temp += "\n      testDriver: " + testDriver;
            temp += "\n      testConfiguration: " + testConfiguration;
            if (testCodes.Any())
            {
                foreach (string testCode in testCodes)
                    temp += "\n      testCode:   " + testCode;
            }
            return temp;
        }
    }

    public class BuildRequest  /* a container for one or more TestElements */
    {
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public List<TestElement> tests { get; set; } = new List<TestElement>();
        public BuildRequest() { }
        public BuildRequest(string auth, DateTime time)
        {
            author = auth;
            timeStamp = time;
        }

        //for converting test request to string
        public override string ToString()
        {
            string temp = "\n  author: " + author + "\n  Time: " + timeStamp.ToString();
            foreach (TestElement te in tests)
                temp += te.ToString();
            return temp;
        }
    }

    public class TestRequest
    {
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public List<TestElement> tests { get; set; } = new List<TestElement>();
        public TestRequest() { }
        public TestRequest(string auth, DateTime time)
        {
            author = auth;
            timeStamp = time;
        }

        //for converting test request to string
        public override string ToString()
        {
            string temp = "\n  author: " + author + "\n  Time: " + timeStamp.ToString();
            foreach (TestElement te in tests)
                temp += te.ToString();
            return temp;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // TestResult and TestResults classes
    //
    public class BuildResult  /* information about processing of build result of single file */
    {
        public string fileName { get; set; }
        public int numberOfErrors { get; set; }
        public string passed { get; set; }
        public string log { get; set; }
        public string time { get; set; }
        public BuildResult() { }
        public BuildResult(string name, string status)
        {
            fileName = name;
            passed = status;
        }

        //for adding log element to existing log
        public void addLog(string logItem)
        {
            log += logItem;
        }

        //for converting Build result to string
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n    Build: " + fileName + "\n    " + "Build Status: " + passed);
            sb.Append("\n    Log:  " + log);
            return sb.ToString();
        }
    }

    public class BuildResults  /* a container for one or more TestResult instances */
    {
        public string author { get; set; }
        public List<BuildResult> results { get; set; } = new List<BuildResult>();

        public BuildResults() { }
        public BuildResults(string auth)
        {
            author = auth;
        }

        //adding one result to bunch of build results
        public BuildResults add(BuildResult rslt)
        {
            results.Add(rslt);
            return this;
        }

        //converting build results
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n\n    Author: " + author + "\n");
            foreach (BuildResult rslt in results)
            {
                sb.Append(rslt.ToString());
            }
            return sb.ToString();
        }
    }

    public class TestResult  /* information about processing of a single test */
    {
        public string testName { get; set; }
        public bool passed { get; set; }
        public string log { get; set; }

        public TestResult() { }
        public TestResult(string name, bool status)
        {
            testName = name;
            passed = status;
        }

        //adding log item to whole log
        public void addLog(string logItem)
        {
            log += logItem;
        }

        //for converting build results into string
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n    Test: " + testName + " " + passed);
            sb.Append("\n    log:  " + log);
            return sb.ToString();
        }
    }

    public class TestResults  /* a container for one or more builds of many tests */
    {
        public string author { get; set; }
        public DateTime timeStamp { get; set; }
        public List<TestResult> results { get; set; } = new List<TestResult>();

        public TestResults() { }
        public TestResults(string auth, DateTime ts)
        {
            author = auth;
            timeStamp = ts;
        }

        //adding test result to test results
        public TestResults add(TestResult rslt)
        {
            results.Add(rslt);
            return this;
        }

        //Converting test results to string
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n  Author: " + author + " " + timeStamp.ToString());
            foreach (TestResult rslt in results)
            {
                sb.Append(rslt.ToString());
            }
            return sb.ToString();
        }
    }
    class BuilderMessages
    {
#if (Test_BuildServerMessages)
        static void Main(string[] args)
        {
            "Testing THMessage Class".title('=');
            Console.WriteLine();

            ///////////////////////////////////////////////////////////////
            // Serialize and Deserialize TestRequest data structure

            "Testing Serialization of TestRequest data structure".title();

            TestElement te1 = new TestElement();
            te1.testName = "test1";
            te1.addDriver("td1.dll");
            te1.addCode("tc1.dll");
            te1.addCode("tc2.dll");
            TestRequest tr = new TestRequest();
            tr.author = "Jim Fawcett";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            string trXml = tr.ToXml();
            Console.Write("\n  Serialized TestRequest data structure:\n\n  {0}\n", trXml);
            Message msg = new Message();
            msg.to = "TH";
            msg.from = "CL";
            msg.type = "basic";
            msg.author = "Fawcett";

            Console.Write("\n  base message:\n    {0}", msg.ToString());
            msg.show();

            Console.Write("\n  Creating Message using TestRequest data structure\n");
            Message rqstMsg = new Message();
            rqstMsg.author = "Fawcett";
            rqstMsg.to = "localhost:8080";
            rqstMsg.from = "localhost:8091";
            rqstMsg.type = "TestRequest";
            rqstMsg.time = DateTime.Now;
            rqstMsg.body = tr.ToXml();
            rqstMsg.show();
            Console.Write("\n  retrieving testResults object:");
            BuildResults newTr = rltsMsg.body.FromXml<BuildResults>();
            Console.Write("\n{0}", newTr);
            Console.Write("\n\n");
        }
#endif
    }
}
