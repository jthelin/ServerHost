// Copyright (c) Jorgen Thelin. All rights reserved.

using System;
using FluentAssertions;
using log4net;
using log4net.Config;
using Xunit;
using Xunit.Abstractions;

using ServerHost;

namespace Tests
{
    public class ServerTestHostTests
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServerTestHostTests));

        [Fact]
        public void LoadServerInNewAppDomain()
        {
            string serverName = null; // TestContext.TestName;

            ServerHostHandle<TestServer.Server> serverHostHandle = ServerTestHost
                .LoadServerInNewAppDomain<TestServer.Server>(serverName);

            serverHostHandle.Should().NotBeNull("Null ServerHostHandle returned.");
            serverHostHandle.ServerName.Should().Be(serverName, "ServerHostHandle.ServerName");
            serverHostHandle.AppDomain.Should().NotBeNull("Null ServerHostHandle.AppDomain returned.");
            serverHostHandle.Server.Should().NotBeNull("Null ServerHostHandle.Server returned.");
            serverHostHandle.Server.Should().BeOfType<TestServer.Server>("Server instance type.");
        }

        #region Test Initialization / Cleanup methods

        private readonly ITestOutputHelper output;

        // ClassInitialize
        public ServerTestHostTests(ITestOutputHelper output)
        {
            // Set up the log4net configuration from data in the App.config file.
            XmlConfigurator.Configure();

            this.output = output;

            log.InfoFormat("ClassInitialize - Current directory = {0}", Environment.CurrentDirectory);
        }

        // TestInitialize
        public void TestInitialize()
        {
            string testName = null; // TestContext.TestName;

            output.WriteLine("TestInitialize - {0}", testName);
        }

        // TestCleanup
        public void TestCleanup()
        {
            string testName = null; // TestContext.TestName;

            output.WriteLine("TestCleanup - {0}", testName);

            ServerTestHost.UnloadAllServers();
        }

        #endregion
    }
}
