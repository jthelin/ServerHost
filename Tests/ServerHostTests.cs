// Copyright (c) Jorgen Thelin. All rights reserved.

using System;
using FluentAssertions;
using log4net;
using log4net.Config;
using Xunit;
using Xunit.Abstractions;

namespace Server.Host.Tests
{
    public class ServerHostTests : IClassFixture<ServerHostTestFixture>, IDisposable
    {
        private readonly ITestOutputHelper output;

        [Fact]
        [Trait("Category","BVT")]
        public void LoadServerInNewAppDomain()
        {
            string serverName = "LoadServerInNewAppDomain"; // TestContext.TestName;

            ServerHostHandle<TestServer.Server> serverHostHandle = ServerHost
                .LoadServerInNewAppDomain<TestServer.Server>(serverName);

            serverHostHandle.Should().NotBeNull("Null ServerHostHandle returned.");
            serverHostHandle.ServerName.Should().Be(serverName, "ServerHostHandle.ServerName");
            serverHostHandle.AppDomain.Should().NotBeNull("Null ServerHostHandle.AppDomain returned.");
            serverHostHandle.Server.Should().NotBeNull("Null ServerHostHandle.Server returned.");
            serverHostHandle.Server.Should().BeOfType<TestServer.Server>("Server instance type.");
        }

        #region Test Initialization / Cleanup methods

        // TestInitialize
        public ServerHostTests(ITestOutputHelper output, ServerHostTestFixture fixture)
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
            ServerHost.UnloadAllServers();
        }
        #endregion
    }

    public class ServerHostTestFixture : IDisposable
    {
        internal static readonly ILog log = LogManager.GetLogger(typeof(ServerHostTestFixture));

        // ClassInitialize
        public ServerHostTestFixture()
        {
            // Set up the log4net configuration.
            BasicConfigurator.Configure();

            log.Info("ServerHostTestFixture - Initialize");
        }

        // ClassCleanup
        public void Dispose()
        {
            log.Info("ServerHostTestFixture - Dispose");
        }
    }
}
