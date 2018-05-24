/////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Client prototype GUI for Pluggable Repository  //
// ver 1.0                                                             //
// Language:    C#, Visual Studio 2017                                 //
// Platform:    Lenovo ideapad 500, Windows 10                         //
// Application: Remote Build Server                                    //
//                                                                     //
// Author Name : Nupur Kulkarni                                        //
// Source: Dr. Jim Fawcett                                             //
// CSE681: Software Modeling and Analysis, Fall 2017                   //
/////////////////////////////////////////////////////////////////////////
/*  
 *  Purpose:
 *    Prototype for a client for the Pluggable Repository.This application
 *    doesn't connect to the repository - it has no Communication facility.
 *    It simply explores the kinds of user interface elements needed for that.
 *
 *  Required Files:
 *    MainWindow.xaml, MainWindow.xaml.cs - view into repository and checkin/checkout
 *    Window1.xaml, Window1.xaml.cs       - Code and MetaData view for individual packages
 *  
 *  Interface information:
 *    void initializeFilesListBox() - this method initializes first tab second list box
 *    initializeTestDriverListBox() - initializes second tab list box
 *    void sendButton_Click(object sender, RoutedEventArgs e) - decides what should be done when sender button is clicked
 *    AddTestButton_Click(object sender, RoutedEventArgs e) - decides what to be done when create test request button is clicked
 *    Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) - decides what to be done before closing window
 *    AddTestButton_Click(object sender, RoutedEventArgs e) - decides what to be done when add test button is clicked 
 *    testDriverListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) - decides what to be done when anything in test files list box in first tab is double clicked
 *
 *  Maintenance History:
 *    ver 1.0 : 15 Jun 2017
 *    - first release
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BuildServerMessages;
using Utilities;
using System.Diagnostics;
using CommChannelDemo;
using System.Threading;

namespace PluggableRepoClient
{
    public partial class MainWindow : Window
    {

        List<Window1> popups = new List<Window1>();
        List<TestElement> testElements = new List<TestElement>();
        Random rand = new Random();
        public Comm<MainWindow> comm { get; set; } = new Comm<MainWindow>();
        private Thread rcvThread = null;
        public string endPoint { get; } = Comm<MainWindow>.makeEndPoint("http://localhost", 8085);
        IAsyncResult cbResult;
        private string clientPath = "../../../ClientStorage/"; 

        //this method initializes first tab second list box
        void initializeFilesListBox(string file)
        {
                if (file.Contains(".cs") && !file.Contains("Driver"))
                    filesListBox.Items.Add(file);
        }

        //initializes first tab first list box
        void initializeTestDriverListBox(string file)
        {
                if (file.Contains("Driver"))
                    testDriverListBox.Items.Add(file);
        }
        //initializes second tab list box
        void initializeTestListBox(string file)
        {
            String pattern = ".xml";
            if(file.Contains(pattern))
                testListBox.Items.Add(file);
        }

        public MainWindow()
        {
            comm.rcvr.CreateRecvChannel(endPoint);
            rcvThread = comm.rcvr.start(rcvThreadProc);
            InitializeComponent();
        }
        //receiver procedure 
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
                if (msg.type == "RepoContent")
                {
                    List<string> files = msg.body.Split(',').ToList();
                    foreach (string file in files)
                    {
                        if (Dispatcher.CheckAccess())
                            initialization(file);
                        else
                            Dispatcher.Invoke(
                              new Action<string>(initialization),
                              System.Windows.Threading.DispatcherPriority.Background,
                              new string[] { file }
                            );
                    }
                }
            }
        }
        //for initializing list boxes
        private void initialization(string file)
        {
            initializeFilesListBox(file);
            initializeTestListBox(file);
            initializeTestDriverListBox(file);
            initializeLogListBox(file);
        }
  
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            initializetestListBoxClient();
            initializeLogListBoxClient();
        }
        //initialize list box in fourth tab
        private void initializeLogListBox(string file)
        {
            String pattern = ".txt";
            if(file.Contains(pattern))
                logListBox.Items.Add(file);
            
            statusLabel.Text = "Click on Request Logs from Repository button to request logs from repository";
        }
        //to initialize log list box on logs tab
        private void initializeLogListBoxClient()
        {
            ClientLogs.Items.Clear();
            String pattern = "*.txt";
            string[] files = Directory.GetFiles("../../../ClientStorage/", pattern);
            foreach (string file in files)
            {
                ClientLogs.Items.Add(System.IO.Path.GetFileName(file));
            }
        }

        //decides what should be done when sender button is clicked
        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Text = "Status: Send selected file to Builder";
        }
        //to show logs after double clicking on file name
        private void testListBoxClient_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)testListBoxClient.SelectedItem;

            codePopup.codeLabel.Text = "Source code: " + fileName;

            showFile(fileName, codePopup);
            return;

        }

        //decides what to be done when create test request button is clicked
        private void CreateTestRequestButton_Click(object sender, RoutedEventArgs e)
        {
            BuildRequest tr = new BuildRequest();
            tr.author = "Nupur Kulkarni";
            tr.timeStamp = DateTime.Now;
            tr.tests.AddRange(testElements);
            string trXml = tr.ToXml();
            string filename = "BuildRequest" + rand.Next(1,10000).ToString();
            using (FileStream fs = File.Create(clientPath + "/" + filename + ".xml"))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(trXml);
                fs.Write(info, 0, info.Length);
            }
            testElements.Clear();
            statusLabel.Text = "Status: Build Request Created with name " + filename;
            initializetestListBoxClient();
        }
        //initialize second list box on log tab
        private void initializetestListBoxClient()
        {
            testListBoxClient.Items.Clear();
            String pattern = "*.xml";
            string[] files = Directory.GetFiles("../../../ClientStorage/", pattern);
            foreach (string file in files)
            {
                testListBoxClient.Items.Add(System.IO.Path.GetFileName(file));
            }
        }

        //decides what to be done when add test button is clicked 
        private void AddTestButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)testDriverListBox.SelectedItem == null || filesListBox.SelectedItems.Count == 0)
                statusLabel.Text = "Status: Please select Test Driver and Code to test";
            else
            {
                TestElement te1 = new TestElement();
                te1.testName = "test";
                te1.addDriver((string)testDriverListBox.SelectedItem);
                te1.addTestConfiguration("C#");
                foreach (string selectedItem in filesListBox.SelectedItems)
                    te1.addCode(selectedItem);
                testElements.Add(te1);
                statusLabel.Text = "Status: Added Test";
                TestRequest.IsEnabled = true;
                testDriverListBox.UnselectAll();
                filesListBox.UnselectAll();
            }
        }

        //decides what to be done when show code button is clicked 
        private void showCodeButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);
        }

        
        //Shows file in other window
        private void showFile(string fileName, Window1 popUp)
        {
            string path = System.IO.Path.Combine("../../../ClientStorage/", fileName);
            Paragraph paragraph = new Paragraph();
            string fileText = "";
            try
            {
                fileText = System.IO.File.ReadAllText(path);
            }
            finally
            {
                paragraph.Inlines.Add(new Run(fileText));
            }

            // add code text to code view
            popUp.codeView.Blocks.Clear();
            popUp.codeView.Blocks.Add(paragraph);
        }
        //decides what to do if window close command comes
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var popup in popups)
                popup.Close();
        }
        //decides what to be done when stop builder button is clicked 
        private void StopBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            Message msg = new Message();
            string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8080);
            msg.from = endPoint;
            msg.to = remoteEndPoint;
            msg.type = "quit";
            comm.sndr.PostMessage(msg);
        }
        //decides what to be done when start builder button is clicked 
        private void StartBuilderButton_Click(object sender, RoutedEventArgs e)
        {
            if(noOfProcessesTextBox.Text == "")
                statusLabel.Text = "Status: Please enter value less than 10 and greater than 0 in text box";
            else
            {
                try
                {
                    if (Convert.ToInt32(noOfProcessesTextBox.Text) > 10 || Convert.ToInt32(noOfProcessesTextBox.Text) <= 0)
                        statusLabel.Text = "Status: Please enter value less than 10 and greater than 0 in text box";
                    else
                    {
                        if (createProcess(noOfProcessesTextBox.Text))
                        {
                            Console.Write(" - succeeded");
                        }
                        else
                        {
                            Console.Write(" - failed");
                        }
                    }
                }
                catch(Exception ex)
                {
                    statusLabel.Text = "Invalid number of proceses" + ex;
                }
            }
        }

        //creates mother builder process
        static bool createProcess(string numberOfProcesses)
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\MotherBuilder\\bin\\debug\\MotherBuilder.exe";
            string absFileSpec = System.IO.Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = numberOfProcesses;

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

        //function for viewing the logs
        private void logListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1 codePopup = new Window1();
            codePopup.Show();
            popups.Add(codePopup);

            codePopup.codeView.Blocks.Clear();
            string fileName = (string)ClientLogs.SelectedItem;

            codePopup.codeLabel.Text = "Source code: " + fileName;

            showFile(fileName, codePopup);
            return;
        }
        //refresh button click handler
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            foreach (string selectedItem in logListBox.SelectedItems)
                comm.sndr.download(selectedItem, clientPath);
            initializeLogListBoxClient();
        }

        //handeling get repository content button click
        private void GetRepoContent_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("\n   Requirement 11: Sending request to Repository Server for getting files");
            Message msg = new Message();
            string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8083);
            msg.from = endPoint;
            msg.to = remoteEndPoint;
            msg.type = "GetRepoContent";
            comm.sndr.PostMessage(msg);
            Action proc = this.rcvThreadProc;
            cbResult = proc.BeginInvoke(null, null);
            AddTest.IsEnabled = true;
            Refresh.IsEnabled = true;
        }
        //handle sending the build request to reository 
        private void SendToRepo_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("\n   Requirement 12: Sending build request structures to the repository for storage and transmission to the Build Server.");
            testListBox.Items.Clear();
            testDriverListBox.Items.Clear();
            filesListBox.Items.Clear();
            logListBox.Items.Clear();
            comm.sndr.uploadFile(testListBoxClient.SelectedItem.ToString(), clientPath);
            Message msg = new Message();
            string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8083);
            msg.from = endPoint;
            msg.to = remoteEndPoint;
            msg.type = "GetRepoContent";
            comm.sndr.PostMessage(msg);
            Action proc = this.rcvThreadProc;
            cbResult = proc.BeginInvoke(null, null);
        }
        //handling what should happen after clicking on build button 
        private void Build_Click(object sender, RoutedEventArgs e)
        {
            if(testListBox.SelectedItem != null && testListBoxClient.SelectedItem != null)
            {
                statusLabel.Text = "Please select 1 build request at a time";
                testListBox.UnselectAll();
                testListBoxClient.UnselectAll();
            }
            else
            {
                Message msg = new Message();
                msg.from = endPoint;
                msg.type = "BuildRequest";
                if (testListBoxClient.SelectedItem != null)
                {
                    string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8080);
                    msg.to = remoteEndPoint;
                    string fileName, fileContent;
                    fileName = testListBoxClient.SelectedItem.ToString();
                    using (StreamReader sr = new StreamReader(clientPath + fileName))
                    {
                        // Read the stream to a string, and write the string to the console.
                        fileContent = sr.ReadToEnd();
                    }
                    msg.body = fileContent;
                    comm.sndr.PostMessage(msg);
                }
                else
                {
                    string remoteEndPoint = Comm<MainWindow>.makeEndPoint("http://localhost", 8083);
                    msg.to = remoteEndPoint;
                    msg.body = testListBox.SelectedItem.ToString();
                    comm.sndr.PostMessage(msg);
                }
            }
        }
    }
}
