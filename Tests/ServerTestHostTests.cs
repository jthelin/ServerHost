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
    public class ServerTestHostTests : IClassFixture<ServerTestHostFixture>, IDisposable
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(ServerTestHostTests));

        private readonly ITestOutputHelper output;

        [Fact]
        [Trait("Category","BVT")]
        public void LoadServerInNewAppDomain()
        {
            string serverName = "LoadServerInNewAppDomain"; // TestContext.TestName;

            ServerHostHandle<TestServer.Server> serverHostHandle = ServerTestHost
                .LoadServerInNewAppDomain<TestServer.Server>(serverName);

            serverHostHandle.Should().NotBeNull("Null ServerHostHandle returned.");
            serverHostHandle.ServerName.Should().Be(serverName, "ServerHostHandle.ServerName");
            serverHostHandle.AppDomain.Should().NotBeNull("Null ServerHostHandle.AppDomain returned.");
            serverHostHandle.Server.Should().NotBeNull("Null ServerHostHandle.Server returned.");
            serverHostHandle.Server.Should().BeOfType<TestServer.Server>("Server instance type.");
        }

        #region Test Initialization / Cleanup methods

        // TestInitialize
        public ServerTestHostTests(ITestOutputHelper output, ServerTestHostFixture fixture)
        {
            this.output = output;
            output.WriteLine("TestInitialize");

            output.WriteLine("Fixture = {0}", fixture);

            output.WriteLine("Current directory = {0}", Environment.CurrentDirectory);
        }

        // TestCleanup
        public void Dispose()
        {
            output.WriteLine("TestCleanup");

            output.WriteLine("UnloadAllServers");
            ServerTestHost.UnloadAllServers();
        }
        #endregion
    }

    public class ServerTestHostFixture : IDisposable
    {
        internal static readonly ILog log = LogManager.GetLogger(typeof(ServerTestHostFixture));

        // ClassInitialize
        public ServerTestHostFixture()
        {
            // Set up the log4net configuration.
            BasicConfigurator.Configure();

            log.Info("ServerTestHostFixture - Initialize");
        }

        // ClassCleanup
        public void Dispose()
        {
            log.Info("ServerTestHostFixture - Dispose");
        }
    }
}
