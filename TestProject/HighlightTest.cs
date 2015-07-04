using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AvalonTextBox.Converters;
using AvalonTextBox;
using JayDev.MediaScribe.Common;

namespace TestProject
{
    /// <summary>
    /// Summary description for HighlightTest
    /// </summary>
    [TestClass]
    public class HighlightTest
    {
        public HighlightTest()
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

        [TestMethod]
        public void TestHightlighting1()
        {
            NoteSectionsToInlineListConverter converter = new NoteSectionsToInlineListConverter();
            List<Section> sections = new List<Section>();
            sections.Add(new Section() { Text="the quick ", Weight = NoteWeight.Normal});
            sections.Add(new Section() { Text="brown fox", Weight = NoteWeight.Bold});
            sections.Add(new Section() { Text=" jump", Weight = NoteWeight.Normal});
            sections.Add(new Section() { Text="ed over the lazy dog", Weight = NoteWeight.Normal});
            List<HighlightMatch> highlights = new List<HighlightMatch>() { new HighlightMatch(20, 6) };
            converter.Convert(sections, typeof(List<System.Windows.Documents.Inline>), null, null, highlights);
        }
    }
}
