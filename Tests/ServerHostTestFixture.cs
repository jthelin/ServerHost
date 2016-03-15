// Copyright (c) Jorgen Thelin. All rights reserved.

using System;
using log4net;
using log4net.Config;

namespace Server.Host.Tests
{
    /// <summary>
    /// xUnit test fixture to provide per-test-class usage of hosted server instances.
    /// </summary>
    public class ServerHostTestFixture : IDisposable
    {
        private readonly ILog log;

        private readonly string className;

        // Test-ClassInitialize
        public ServerHostTestFixture()
        {
            this.className = GetType().Name;

            // Set up the log4net configuration.
            BasicConfigurator.Configure();

            this.log = LogManager.GetLogger(className);

            log.InfoFormat("{0} - Initialize", className);
        }

        // Test-ClassCleanup
        public void Dispose()
        {
            log.InfoFormat("{0} - Dispose", className);
        }
    }
}
