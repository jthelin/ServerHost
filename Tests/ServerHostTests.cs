// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed under Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using ServerHost.Test.Xunit;

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

        [Fact]
        [Trait("Category", "BVT")]
        public void ServerHost_Version()
        {
            string versionString = LibraryVersionInfo.FileVersion;
            output.WriteLine("ServerHost library version = {0}", versionString);

            versionString.Should().NotBeNullOrEmpty("Version value should be returned");

            versionString.Should().Contain(".", "Version format = Major.Minor");
            versionString.Should().NotStartWith("1.0.0.0", "Version should be specific.");
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

            output.WriteLine("ServerHost library API version = {0}", LibraryVersionInfo.ApiVersion);
            output.WriteLine("ServerHost library file version = {0}", LibraryVersionInfo.FileVersion);
            output.WriteLine("ServerHost library full version info string = {0}", LibraryVersionInfo.Current);
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
}
