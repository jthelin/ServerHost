// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed under Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Server.Host.Tests.Net46
{
    public sealed class ServerHostTests : IClassFixture<ServerHostFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _className;

        #region Test Initialization / Cleanup methods

        // TestInitialize
        public ServerHostTests(ITestOutputHelper output, ServerHostFixture fixture)
        {
            _output = output;
            _className = GetType().Name;

            _output.WriteLine("{0} TestInitialize", _className);
            _output.WriteLine("{0} Fixture = {1}", _className, fixture);
        }

        // TestCleanup
        public void Dispose()
        {
            _output.WriteLine("{0} TestCleanup", _className);

            _output.WriteLine("{0} UnloadAllServers", _className);
            ServerHost.UnloadAllServers();
        }
        #endregion

        [Fact]
        [Trait("Category","BVT")]
        [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
        public void LoadServerInNewAppDomain()
        {
            string testName = "LoadServerInNewAppDomain"; // TestContext.TestName;

            ServerHostHandle<TestServer.Server> serverHostHandle =
                ServerHost.LoadServerInNewAppDomain<TestServer.Server>(testName);

            serverHostHandle.Should().NotBeNull("Null ServerHostHandle returned.");
            serverHostHandle.ServerName.Should().Be(testName, "ServerHostHandle.ServerName");
            serverHostHandle.AppDomain.Should().NotBeNull("Null ServerHostHandle.AppDomain returned.");
            serverHostHandle.Server.Should().NotBeNull("Null ServerHostHandle.Server returned.");
            serverHostHandle.Server.Should().BeOfType<TestServer.Server>("Server instance type.");
        }

        [Fact]
        [Trait("Category", "BVT")]
        public async Task UnloadServerInAppDomain()
        {
            ServerHost.LoadServerInNewAppDomain<TestServer.Server>("One");
            ServerHost.LoadServerInNewAppDomain<TestServer.Server>("Two");
            ServerHost.LoadServerInNewAppDomain<TestServer.Server>("Three");

            var unload = Enumerable.Range(1, 4).Select(i =>
            {
                return Task.Run(async () =>
                {
                    await Task.Delay(100 - i);
                    ServerHost.UnloadAllServers();
                });
            });
            await Task.WhenAll(unload);
        }

        [Fact]
        [Trait("Category", "BVT")]
        public void ServerHost_Version()
        {
            _output.WriteLine("ServerHost library API version = {0}", LibraryVersionInfo.ApiVersion);
            _output.WriteLine("ServerHost library file version = {0}", LibraryVersionInfo.FileVersion);
            _output.WriteLine("ServerHost library full version info string = {0}", LibraryVersionInfo.Current);

            string versionString = LibraryVersionInfo.FileVersion;
            _output.WriteLine("ServerHost library version = {0}", versionString);

            versionString.Should().NotBeNullOrEmpty("Version value should be returned");

            versionString.Should().Contain(".", "Version format = Major.Minor");
            versionString.Should().NotStartWith("1.0.0.0", "Version should be specific.");
            versionString.Should().NotStartWith("0.0.0.0", "Version should not be zero.");
            versionString.Should().NotContain("*", "Version should be explicit.");
        }

        [Fact]
        [Trait("Category", "BVT")]
        public void ServerHost_ExecId()
        {
            Guid execId = ExecId.Value;

            _output.WriteLine("ExecId = {0}", execId);

            execId.Should().NotBe(Guid.Empty, "ExecId value should be populated.");
        }
    }
}
