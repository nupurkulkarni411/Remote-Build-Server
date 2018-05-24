/////////////////////////////////////////////////////////////////////
// ICommunicator.cs - Peer-To-Peer Communicator Service Contract   //
// ver 2.0                                                         //
// Language:    C#, Visual Studio 2017                             //
// Platform:    Lenovo ideapad 500, Windows 10                     //
// Application: Remote Build Server                                //
//                                                                 //
// Author Name : Nupur Kulkarni                                    //
// Source: Dr. Jim Fawcett                                         //
// CSE681: Software Modeling and Analysis, Fall 2017               //
/////////////////////////////////////////////////////////////////////
/*
 * Maintenance History:
 * ====================
 * ver 2.0 : 10 Oct 11
 * - removed [OperationContract] from GetMessage() so only local client
 *   can dequeue messages
 * ver 1.0 : 14 Jul 07
 * - first release
 */

using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;


namespace CommChannelDemo
{
  [ServiceContract]
  public interface ICommunicator
  {
    [OperationContract(IsOneWay = true)]
    void PostMessage(Message msg);

    [OperationContract(IsOneWay = true)]
    void upLoadFile(FileTransferMessage msg);
    [OperationContract]
    Stream downLoadFile(string filename);


        // used only locally so not exposed as service method

        Message GetMessage();
  }

    // The class Message is defined in CommChannelDemo.Messages as [Serializable]
    // and that appears to be equivalent to defining a similar [DataContract]

    [MessageContract]
    public class FileTransferMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }

}
