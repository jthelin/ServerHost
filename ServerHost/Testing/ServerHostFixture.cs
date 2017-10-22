// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Diagnostics.CodeAnalysis;
using log4net;
using log4net.Config;

namespace Server.Host.Testing
{
    /// <summary>
    /// xUnit test fixture to provide per-test-class usage of hosted server instances.
    /// </summary>
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ServerHostFixture : IDisposable
    {
        private readonly ILog _log;

        private readonly string _className;

        /// <summary>
        /// Constructor -- Equivalent to ClassInitialize function.
        /// </summary>
        public ServerHostFixture()
        {
            _className = GetType().Name;

            // Set up the log4net configuration.
            BasicConfigurator.Configure();

            _log = LogManager.GetLogger(_className);

            _log.Info($"{_className} - Initialize");
        }

        /// <summary>
        /// Dispose -- Equivalent to ClassCleanup function.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose processing function.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                _log.Info($"{_className} - Dispose - UnloadAllServers");

                ServerHost.UnloadAllServers();
            }
        }

        /// <summary>
        /// Finalizer function.
        /// </summary>
        ~ServerHostFixture()
        {
            Dispose(false);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReleaseUnmanagedResources()
        {
            _log.Info($"{_className} - ReleaseUnmanagedResources");

            // TODO release unmanaged resources here
        }
    }
}
