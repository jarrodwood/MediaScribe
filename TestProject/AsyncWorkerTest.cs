using JayDev.MediaScribe.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace TestProject
{
    
    
    /// <summary>
    ///This is a test class for AsyncWorkerTest and is intended
    ///to contain all AsyncWorkerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AsyncWorkerTest
    {


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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for QueueWorkItem
        ///</summary>
        [TestMethod()]
        public void QueueWorkItemTest()
        {
            AsyncWorker target = new AsyncWorker(); // TODO: Initialize to an appropriate value

            for (int i = 0; i < 10; i++)
            {
                int blah = i;
                target.QueueWorkItem((x) =>
                {
                    Debug.WriteLine("Start Action a-{0}", blah);
                    for (int q = 0; q < 20; q++)
                    {
                        Thread.Sleep(50);
                        if (x.Cancel == true)
                        {
                            Debug.WriteLine("got the cancel notification at {0}/20", q);
                            break;
                        }
                    }
                    Debug.WriteLine("End Action a-{0}", blah);
                });
            }

            Thread.Sleep(4500);

            target.AbortAllWork();

            for (int i = 0; i < 10; i++)
            {
                int blah = i;
                target.QueueWorkItem((x) =>
                {
                    Debug.WriteLine("Start Action a-{0}", blah);
                    for (int q = 0; q < 20; q++)
                    {
                        Thread.Sleep(50);
                        if (x.Cancel == true)
                        {
                            Debug.WriteLine("got the cancel notification at {0}/20", q);
                            break;
                        }
                    }
                    Debug.WriteLine("End Action a-{0}", blah);
                });
            }

            Thread.Sleep(2500);

            target.AbortAllWork();

            for (int i = 0; i < 10; i++)
            {
                int blah = i;
                target.QueueWorkItem((x) =>
                {
                    Debug.WriteLine("Start Action b-{0}", blah);
                    for (int q = 0; q < 20; q++)
                    {
                        Thread.Sleep(50);
                    }
                    Debug.WriteLine("End Action b-{0}", blah);
                });
            }


            Thread.Sleep(44000);
        }

    }
}
