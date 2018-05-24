/////////////////////////////////////////////////////////////////////
// RepositoryServer.cs - define basic functionalities of repository//
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
 * This package processes messages sent by client to view logs or process test request.
 *
 *  Public Interface:
 * ================= 
 * public List<FileName> files1(bool showPath = false): return list of files in storage/category
 * public List<FileName> filesWithPattern(String path, String pattern): return list of files in storage/category with given pattern
 * 
 * Build Process:
 * --------------
 * Required Files:  RepositoryServer.cs, CommChannelDemo.cs , Communication.cs, Messages.cs
 * Compiler command: csc RepositoryServer.cs, CommChannelDemo.cs , Communication.cs, Messages.cs
 *
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
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.ServiceModel;

namespace RepositoryServer
{
    using FileName = String;  // filename may have version number at end
    using FileSpec = String;  // c:/.../category/filename
    class RepositoryServer
    {
        public Comm<RepositoryServer> comm { get; set; } = new Comm<RepositoryServer>();
        public string endPoint { get; } = Comm<RepositoryServer>.makeEndPoint("http://localhost", 8083);

        private Thread rcvThread = null;
        private string repoStoragePath = "../../../RepoStorage/";

        //----< initialize receiver >------------------------------------
        public RepositoryServer()
        {
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
        }

        //For receiving messages from other components
        void rcvThreadProc()
        {
            while (true)
            {
                Message msg = comm.rcvr.GetMessage();
                msg.time = DateTime.Now;
                Console.Write("\n  {0} received message:", comm.name);
                msg.showMsg();
                if (msg.body == "quit")
                    break;
                if(msg.type == "GetRepoContent")
                {
                    Message msg1 = new Message();
                    msg1.to = Comm<RepositoryServer>.makeEndPoint("http://localhost", 8085);
                    msg1.from = endPoint;
                    List<FileName> files = files1();
                    msg1.body = String.Join(",",files);
                    msg1.type = "RepoContent";
                    comm.sndr.PostMessage(msg1);
                }
                if(msg.type == "BuildRequest")
                {
                    Console.WriteLine("\n   Requirement 13: request the repository to send a build request in its storage to the Build Server for build processing.");
                    Message msg1 = new Message();
                    msg1.to = Comm<RepositoryServer>.makeEndPoint("http://localhost", 8080);
                    msg1.from = endPoint;
                    msg1.type = "BuildRequest";
                    using (StreamReader sr = new StreamReader(repoStoragePath + msg.body))
                    {
                        // Read the stream to a string.
                        msg1.body = sr.ReadToEnd();
                    }
                    comm.sndr.PostMessage(msg1);
                }
            }
        }

        /*----< return list of files in storage/category >-------------*/

        public List<FileName> files1(bool showPath = false)
        {

            FileSpec[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(repoStoragePath));

            for (int i = 0; i < files.Length; ++i)
            {
                if (showPath)
                    files[i] = System.IO.Path.GetFullPath(files[i]);  // now an absolute FileSpec
                else
                    files[i] = System.IO.Path.GetFileName(files[i]);  // now a FileName
            }
            return files.ToList<FileName>();
        }

        //----< join receive thread >------------------------------------

        public void wait()
        {
            rcvThread.Join();
        }

        /*----< return list of files in storage/category with given pattern >-------------*/

        public List<FileName> filesWithPattern(String path, String pattern)
        {

            FileSpec[] files = Directory.GetFiles(path, pattern);

            for (int i = 0; i < files.Length; ++i)
            {
                files[i] = System.IO.Path.GetFileName(files[i]);  // now a FileName
            }
            return files.ToList<FileName>();
        }
        static void Main(string[] args)
        {

            Console.Title = "Repository";
            Console.Write("\n  Repository");
            Console.Write("\n ===========\n");
            ServiceHost host = CommChannelDemo.Receiver<RepositoryServer>.CreateServiceChannel("http://localhost:8000/StreamService");

            host.Open();
            RepositoryServer repoServer = new RepositoryServer();
            repoServer.wait();
            Console.Write("\n\n");
        }
    }
}
