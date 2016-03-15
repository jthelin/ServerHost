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
        private readonly string className;

        [Fact]
        [Trait("Category","BVT")]
        public void LoadServerInNewAppDomain()
        {
            string testName = "LoadServerInNewAppDomain"; // TestContext.TestName;

            ServerHostHandle<TestServer.Server> serverHostHandle = ServerHost
                .LoadServerInNewAppDomain<TestServer.Server>(testName);

            serverHostHandle.Should().NotBeNull("Null ServerHostHandle returned.");
            serverHostHandle.ServerName.Should().Be(testName, "ServerHostHandle.ServerName");
            serverHostHandle.AppDomain.Should().NotBeNull("Null ServerHostHandle.AppDomain returned.");
            serverHostHandle.Server.Should().NotBeNull("Null ServerHostHandle.Server returned.");
            serverHostHandle.Server.Should().BeOfType<TestServer.Server>("Server instance type.");
        }

        #region Test Initialization / Cleanup methods

        // TestInitialize
        public ServerHostTests(ITestOutputHelper output, ServerHostTestFixture fixture)
        {
            this.output = output;
            this.className = GetType().Name;
            output.WriteLine("{0} TestInitialize", className);

            output.WriteLine("{0} Fixture = {1}", className, fixture);

            output.WriteLine("{0} Current directory = {1}", className, Environment.CurrentDirectory);
        }

        // TestCleanup
        public void Dispose()
        {
            output.WriteLine("{0} TestCleanup", className);

            output.WriteLine("{0} UnloadAllServers", className);
            ServerHost.UnloadAllServers();
        }
        #endregion
    }

    public class ServerHostTestFixture : IDisposable
    {
        internal static readonly ILog log = LogManager.GetLogger(typeof(ServerHostTestFixture));

        private readonly string className;

        // ClassInitialize
        public ServerHostTestFixture()
        {
            this.className = GetType().Name;

            // Set up the log4net configuration.
            BasicConfigurator.Configure();

            log.InfoFormat("{0} - Initialize", className);
        }

        // ClassCleanup
        public void Dispose()
        {
            log.InfoFormat("{0} - Dispose", className);
        }
    }
}
