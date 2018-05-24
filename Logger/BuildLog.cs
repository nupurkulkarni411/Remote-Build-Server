/////////////////////////////////////////////////////////////////////
// BuildLog.cs -   This package provides core functionality        //
//                 for logging builds and tests                    //
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
 * This Package handles the creation of logger and logging 
 * all the actions of builds and test.
 * 
 * Public Interface:
 * ================= 
 * void createLog(string author): creating build log and test log of perticuler author.
 * void startLogging(string errors,string output, string time,string fileName) : logs all actions of builder and test harness
 * string getLog(): for getting logs which are created
 * 
 * Build Process:
 * --------------
 * Required Files: BuilderMassages.cs 
   Build Command: csc BuildLog.cs BuilderMassages.cs

 * Maintenance History:
    - Ver 1.0 Oct 2017 
    - Ver 2.0 Dec 2017
 * --------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BuildServerMessages;
using System.IO;

namespace Logger
{
    public class BuildLog
    {
        BuildResults brs;
        //creation of build log
        public void createLog(string author)
        {
            brs = new BuildResults();
            brs.author = author;
        }
        //catching every action taken by builder
        public void startLogging(string errors,string output, string time,string fileName) 
        {
            BuildResult br = new BuildResult();
            br.fileName = fileName;
            //need to go through the output and error provided by command fired 
            // for build to decide build success or failure.
            if(errors == "" && !output.Contains("error"))
            {
                br.numberOfErrors = 0;
                br.passed = "Build Successful";
            }
            else
            {
                br.numberOfErrors = errors.Split('\n').Length;
                br.passed = "Build Failure";
            }
            br.time = time;
            br.log = "\n    Output:\n       " + output + errors;
            br.log = br.log + "\n    Number of errors: " + br.numberOfErrors;
            br.log = br.log + "\n    Time of Execution: " + br.time;
            brs.add(br);
        }
        //for returning the whole log
        public string getLog()
        {
            return brs.ToString();
        }
    }

    public class TestLog
    {
        private TestResults trs;

        //for creating test log
        public void createLog(string author)
        {
            trs = new TestResults();
            trs.author = author;
            trs.timeStamp = DateTime.Now;
        }

        //fr catching every action taken by test harness
        public void startLogging(string name,bool result,string log) 
        {
            TestResult tr = new TestResult();
            tr.testName = name;
            tr.passed = result;
            tr.log = log;
            trs.add(tr);
        }

        //returning test log
        public string getLog()
        {
            return trs.ToString();
        }

    }

    class TestBuildLog
    {
#if (TEST_LOGGER)
        static void Main(string[] args)
        {
            BuildLog bl = new BuildLog();
            bl.createLog("Nupur Kulkarni");
            Builder br = new Builder();
            br.Build(fm.getFile(fm.StoragePath),".cs");
            string log = getLog();
        }
#endif
    }
}
