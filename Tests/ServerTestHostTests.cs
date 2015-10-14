// Copyright (c) Jorgen Thelin. All rights reserved.

using System;
using log4net;
using log4net.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerHost;

namespace Tests
{
    [TestClass]
    public class ServerTestHostTests
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServerTestHostTests));

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void LoadServerInNewAppDomain()
        {
            string serverName = TestContext.TestName;

            ServerHost<TestServer.Server> serverHost = ServerTestHost
                .LoadServerInNewAppDomain<TestServer.Server>(serverName);

            Assert.IsNotNull(serverHost, "Null ServerHost returned.");
            Assert.AreEqual(serverName, serverHost.ServerName, "ServerHost.ServerName");
            Assert.IsNotNull(serverHost.AppDomain, "Null ServerHost.AppDomain returned.");
            Assert.IsNotNull(serverHost.Server, "Null ServerHost.Server returned.");
            Assert.IsInstanceOfType(serverHost.Server, typeof(TestServer.Server), "Server instance type.");
        }

        #region Test Initialization / Cleanup methods

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // Set up the log4net configuration from data in the App.config file.
            XmlConfigurator.Configure();

            log.InfoFormat("ClassInitialize - Current directory = {0}", Environment.CurrentDirectory);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            string testName = TestContext.TestName;

            log.InfoFormat("TestInitialize - {0}", testName);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            string testName = TestContext.TestName;

            log.InfoFormat("TestCleanup - {0}", testName);

            ServerTestHost.UnloadAllServers();
        }

        #endregion
    }
}
