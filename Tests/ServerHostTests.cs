// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed under Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using ServerHost.Test.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Server.Host.Tests
{
    public class ServerHostTests : IClassFixture<ServerHostTestFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _className;

        [Fact]
        [Trait("Category","BVT")]
        [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
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
            _output = output;
            _className = GetType().Name;
            output.WriteLine("{0} TestInitialize", _className);

            output.WriteLine("{0} Fixture = {1}", _className, fixture);

            output.WriteLine("{0} Current directory = {1}", _className, Directory.GetCurrentDirectory());
        }

        // TestCleanup
        public void Dispose()
        {
            _output.WriteLine("{0} TestCleanup", _className);

            _output.WriteLine("{0} UnloadAllServers", _className);
            ServerHost.UnloadAllServers();
        }
        #endregion
    }
}
