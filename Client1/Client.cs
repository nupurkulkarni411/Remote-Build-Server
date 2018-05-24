/////////////////////////////////////////////////////////////////////
// Client.cs - Demonstrate application use of channel              //
// ver 1.0                                                         //
// Language:    C#, Visual Studio 2017                             //
// Platform:    Lenovo ideapad 500, Windows 10                     //
// Application: Remote Build Server                                //
//                                                                 //
// Author Name : Nupur Kulkarni                                    //
// Source: Dr. Jim Fawcett                                         //
// CSE681: Software Modeling and Analysis, Fall 2017               //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * The Client package defines one class, Client, that uses the Comm<Client>
 * class to pass messages to a remote endpoint.
 * 
 * Required Files:
 * ---------------
 * - Client.cs
 * - ICommunicator.cs, CommServices.cs
 * - Messages.cs, MessageTest, Serialization
 *
 * Maintenance History:
 * --------------------
 * Ver 1.0 : 10 Nov 2016
 * Ver 2.0 : 05 Dec 2017
 * - first release 
 *  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Threading;
using BuildServerMessages;
using System.Diagnostics;
using System.IO;


namespace CommChannelDemo
{
  ///////////////////////////////////////////////////////////////////
  // Client class demonstrates how an application uses Comm
  //
  class Client
  {
        public Comm<Client> comm { get; set; } = new Comm<Client>();
        public string endPoint { get; } = Comm<Client>.makeEndPoint("http://localhost", 8081);

        private Thread rcvThread = null;

        //----< initialize receiver >------------------------------------

        public Client()
        {
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
        }

        //----< join receive thread >------------------------------------

        public void wait()
        {
            rcvThread.Join();
        }


        //Build Request for C#
        public string CreateBuildRequest1()
        {
            
            TestElement te2 = new TestElement();
            te2.testName = "test2";
            te2.addDriver("TestDriver1.cs");
            te2.addTestConfiguration("C#");
            te2.addCode("ITest.cs");
            te2.addCode("CodeToTest1.cs");

            BuildRequest tr = new BuildRequest();
            tr.author = "Nupur Kulkarni";
            tr.timeStamp = DateTime.Now;
            tr.tests.Add(te2);
            string trXml = tr.ToXml();
            Console.Write("\n   Build Request: \n{0}\n", trXml);
            return trXml;
        }
        //second build request
        public string CreateBuildRequest2()
        {
            TestElement te1 = new TestElement();
            te1.testName = "test1";
            te1.addDriver("TestDriver2.cs");
            te1.addTestConfiguration("C#");
            te1.addCode("ITest.cs");
            te1.addCode("CodeToTest2.cs");

            BuildRequest tr = new BuildRequest();
            tr.author = "Nupur Kulkarni";
            tr.timeStamp = DateTime.Now;
            tr.tests.Add(te1);
            string trXml = tr.ToXml();
            Console.Write("\n   Build Request: \n{0}\n", trXml);
            return trXml;
        }

        //third build request
        public string CreateBuildRequest3()
        {
            TestElement te1 = new TestElement();
            te1.testName = "test";
            te1.addDriver("TestDriver3.cs");
            te1.addTestConfiguration("C#");
            te1.addCode("ITest.cs");
            te1.addCode("CodeToTest3.cs");

            BuildRequest tr = new BuildRequest();
            tr.author = "Nupur Kulkarni";
            tr.timeStamp = DateTime.Now;
            tr.tests.Add(te1);
            string trXml = tr.ToXml();
            Console.Write("\n   Build Request: \n{0}\n", trXml);
            return trXml;
        }


        //----< construct a basic message >------------------------------

        public Message makeMessage(string author, string fromEndPoint, string toEndPoint)
        {
            Message msg = new Message();
            msg.author = author;
            msg.from = fromEndPoint;
            msg.to = toEndPoint;
            return msg;
        }

        //----< use private service method to receive a message >--------
     
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
            }
        }

        //----< run client demo >----------------------------------------

        static void Main(string[] args)
        {
            Console.Title = "Client1";
            Console.Write("\n  Client1 ");
            Console.Write("\n =========\n");

            Console.Write("\n  Requirement2: Using a Message-Passing Communication Service built with WCF\n");

            Client client = new Client();
            string remoteEndPoint = Comm<Client>.makeEndPoint("http://localhost", 8080);
            Message msg = new Message();
            msg.from = client.endPoint;
            msg.body = client.CreateBuildRequest1();
            msg.to = remoteEndPoint;
            msg.type = "BuildRequest";

            Message msg1 = new Message();
            msg1.from = client.endPoint;
            msg1.body = client.CreateBuildRequest2();
            msg1.to = remoteEndPoint;
            msg1.type = "BuildRequest";

            Message msg2 = new Message();
            msg2.from = client.endPoint;
            msg2.body = client.CreateBuildRequest3();
            msg2.to = remoteEndPoint;
            msg2.type = "BuildRequest";

            //sending four build requests to mother builder
            Console.Write("\n   Sending message from {0} to {1}", client.comm.name, msg.to);
            msg.showMsg();
            client.comm.sndr.PostMessage(msg);
            Console.Write("\n   Sending message from {0} to {1}", client.comm.name, msg1.to);
            msg1.showMsg();
            client.comm.sndr.PostMessage(msg1);
            Console.Write("\n   Sending message from {0} to {1}", client.comm.name, msg.to);
            msg.showMsg();
            client.comm.sndr.PostMessage(msg);
            Console.Write("\n   Sending message from {0} to {1}", client.comm.name, msg2.to);
            msg2.showMsg();
            client.comm.sndr.PostMessage(msg2);
            client.wait();
            Console.Write("\n\n");
        }
  }
}
