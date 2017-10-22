// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Diagnostics.CodeAnalysis;
using log4net;
using log4net.Config;

namespace Server.Host
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

            _log.InfoFormat("{0} - Initialize", _className);
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
                _log.InfoFormat("{0} - Dispose", _className);

                // TODO release managed resources here
            }
        }

        ~ServerHostFixture()
        {
            Dispose(false);
        }
        
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
        private void ReleaseUnmanagedResources()
        {
            _log.InfoFormat("{0} - ReleaseUnmanagedResources", _className);

            // TODO release unmanaged resources here
        }
    }
}