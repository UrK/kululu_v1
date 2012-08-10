using Kululu.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using Kululu.Web.Models.Common;
using Kululu.Entities;
using Dror.Common.Data.Contracts;

namespace Kululu.MSIntegrationTests
{
    
    
    /// <summary>
    ///This is a test class for BaseControllerTest and is intended
    ///to contain all BaseControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BaseControllerTest
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


        internal virtual BaseController_Accessor CreateBaseController_Accessor()
        {
            // TODO: Instantiate an appropriate concrete class.
            BaseController_Accessor target = null;
            return target;
        }

        /// <summary>
        ///A test for GetCurrentUserPrivileges
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Dropbox\\Kululu\\kululu.web", "/")]
        [UrlToTest("http://localhost:11255/")]
        [DeploymentItem("Kululu.Web.dll")]
        public void GetCurrentUserPrivilegesTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BaseController_Accessor target = new BaseController_Accessor(param0); // TODO: Initialize to an appropriate value
            UserPrivileges expected = new UserPrivileges(); // TODO: Initialize to an appropriate value
            UserPrivileges actual;
            actual = target.GetCurrentUserPrivileges();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetCurrentWedding
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Dropbox\\Kululu\\kululu.web", "/")]
        [UrlToTest("http://localhost:11255/")]
        [DeploymentItem("Kululu.Web.dll")]
        public void GetCurrentWeddingTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BaseController_Accessor target = new BaseController_Accessor(param0); // TODO: Initialize to an appropriate value
            Wedding expected = null; // TODO: Initialize to an appropriate value
            Wedding actual;
            actual = target.GetCurrentWedding();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsCurrentUser
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Dropbox\\Kululu\\kululu.web", "/")]
        [UrlToTest("http://localhost:11255/")]
        [DeploymentItem("Kululu.Web.dll")]
        public void IsCurrentUserTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BaseController_Accessor target = new BaseController_Accessor(param0); // TODO: Initialize to an appropriate value
            UserPrivileges userPrivileges = new UserPrivileges(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsCurrentUser(userPrivileges);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsCurrentUserNotSet
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Dropbox\\Kululu\\kululu.web", "/")]
        [UrlToTest("http://localhost:11255/")]
        [DeploymentItem("Kululu.Web.dll")]
        public void IsCurrentUserNotSetTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BaseController_Accessor target = new BaseController_Accessor(param0); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsCurrentUserNotSet();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsCurrentWeddingSet
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Dropbox\\Kululu\\kululu.web", "/")]
        [UrlToTest("http://localhost:11255/")]
        [DeploymentItem("Kululu.Web.dll")]
        public void IsCurrentWeddingSetTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BaseController_Accessor target = new BaseController_Accessor(param0); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsCurrentWeddingSet();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        internal virtual BaseController CreateBaseController()
        {
            // TODO: Instantiate an appropriate concrete class.
            BaseController target = null;
            return target;
        }

        /// <summary>
        ///A test for CurrentUser
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Dropbox\\Kululu\\kululu.web", "/")]
        [UrlToTest("http://localhost:11255/")]
        public void CurrentUserTest()
        {
            BaseController target = CreateBaseController(); // TODO: Initialize to an appropriate value
            FbUser expected = null; // TODO: Initialize to an appropriate value
            FbUser actual;
            target.CurrentUser = expected;
            actual = target.CurrentUser;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Repository
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Dropbox\\Kululu\\kululu.web", "/")]
        [UrlToTest("http://localhost:11255/")]
        [DeploymentItem("Kululu.Web.dll")]
        public void RepositoryTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            BaseController_Accessor target = new BaseController_Accessor(param0); // TODO: Initialize to an appropriate value
            IRepository expected = null; // TODO: Initialize to an appropriate value
            IRepository actual;
            target.Repository = expected;
            actual = target.Repository;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
