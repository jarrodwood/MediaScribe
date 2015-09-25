using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NamedPipeWrapper;
using System.Diagnostics;
using System.Timers;

namespace TestProject
{
    /// <summary>
    /// Summary description for NamedPipesTest
    /// </summary>
    [TestClass]
    public class NamedPipesTest
    {
        public NamedPipesTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        NamedPipeServer<string> server;
        NamedPipeClient<string> client;
        int index = 0;

        [TestMethod]
        public void NamedPipesTest1()
        {
            Trace.WriteLine("Welcome!");
            server = new NamedPipeServer<string>("jarrodtest1");
            server.Start();
            server.ClientMessage += server_ClientMessage;


            client = new NamedPipeClient<string>("jarrodtest1");
            client.Start();
            client.ServerMessage += client_ServerMessage;

            Trace.WriteLine("Preparing for first message...");
            Timer timer = new Timer(5000);
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
            Console.ReadKey();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            server.PushMessage("file" + index++);
        }

        Timer clientTimer = null;
        int clientIndex = 0;
        string currentFile = null;
        void client_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
            Trace.WriteLine("received message from server: " + message);
            currentFile = message;
            if (clientTimer != null)
            {
                clientTimer.Stop();
            }
            clientTimer = new Timer(1500);
            clientTimer.AutoReset = true;
            clientTimer.Elapsed += clientTimer_Elapsed;
            clientTimer.Start();
        }

        void clientTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            client.PushMessage(string.Format("File {0}, generated thumbnail {1}", currentFile, clientIndex++));
        }

        void server_ClientMessage(NamedPipeConnection<string, string> connection, string message)
        {
            Trace.WriteLine("received message from client: " + message);
        }
    }
}
