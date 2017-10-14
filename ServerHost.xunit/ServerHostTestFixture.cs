// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Diagnostics.CodeAnalysis;
using log4net;
using log4net.Config;

namespace ServerHost.Test.Xunit
{
    /// <summary>
    /// xUnit test fixture to provide per-test-class usage of hosted server instances.
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ServerHostTestFixture : IDisposable
    {
        private readonly ILog _log;

        private readonly string _className;

        // Test-ClassInitialize
        public ServerHostTestFixture()
        {
            _className = GetType().Name;

            // Set up the log4net configuration.
            BasicConfigurator.Configure();

            _log = LogManager.GetLogger(_className);

            _log.InfoFormat("{0} - Initialize", _className);
        }

        // Test-ClassCleanup
        public void Dispose()
        {
            _log.InfoFormat("{0} - Dispose", _className);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                // TODO release managed resources here
            }
        }

        ~ServerHostTestFixture()
        {
            Dispose(false);
        }
        
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }
    }
}
