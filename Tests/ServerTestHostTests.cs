// Copyright (c) Jorgen Thelin. All rights reserved.

using System;
using FluentAssertions;
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

            ServerHostHandle<TestServer.Server> serverHostHandle = ServerTestHost
                .LoadServerInNewAppDomain<TestServer.Server>(serverName);

            serverHostHandle.Should().NotBeNull("Null ServerHostHandle returned.");
            serverHostHandle.ServerName.Should().Be(serverName, "ServerHostHandle.ServerName");
            serverHostHandle.AppDomain.Should().NotBeNull("Null ServerHostHandle.AppDomain returned.");
            serverHostHandle.Server.Should().NotBeNull("Null ServerHostHandle.Server returned.");
            serverHostHandle.Server.Should().BeOfType<TestServer.Server>("Server instance type.");
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
