// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Server.Host.Tracing
{
    // adapted from http://stackoverflow.com/questions/15548364/cross-appdomain-access-to-console-out

    /// <summary>
    /// Utility class for hooking the <c>CrossAppDomainTracer</c> into an AppDomain.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static class CrossAppDomainTrace
    {
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private static CrossAppDomainTracer _tracer;

        public static string AppDomainIdent { get; private set; }

        /// <summary>
        /// Inject a new <c>CrossAppDomainTracer</c> instance into the specified AppDomain,
        /// and connect it to pump trace messages into this current [main] AppDomain.
        /// </summary>
        /// <param name="remoteDomain"></param>
        /// <param name="showAppDomainName"></param>
        [PublicAPI]
        public static void StartListening(AppDomain remoteDomain, bool showAppDomainName = true)
        {
            try
            {
                AppDomainIdent = GetAppDomainIdent();

                _tracer = new CrossAppDomainTracer(remoteDomain)
                {
                    ShowAppDomainName = showAppDomainName
                };
            }
            catch (Exception exc)
            {
                string msg = $"Unable to start trace listener in remote AppDomain {remoteDomain}";
                Trace.TraceError(msg + " - " + exc);
                throw new AggregateException(msg, exc);
            }
        }

        internal static string GetAppDomainIdent()
        {
            string appDomainName = GetAppDomainName();

            int appDomainId = Math.Abs(ExecId.Value.GetHashCode());

            return $"{appDomainName}-{appDomainId}";
        }

        internal static string GetAppDomainName()
        {
            string appDomainName = AppDomain.CurrentDomain.FriendlyName;
            int idx = appDomainName.IndexOf(":", StringComparison.Ordinal);
            if (idx > 0)
            {
                appDomainName = appDomainName.Substring(0, idx);
            }

            return appDomainName;
        }
    }

    internal sealed class CrossAppDomainTracer : MarshalByRefObject, IDisposable
    {
        public bool ShowAppDomainName { get; internal set; }

        private CrossAppDomainTracer _remoteTracer;

        [NonSerialized] private ActionTraceListener _localListener;

        [NonSerialized] private readonly string _appDomainIdent;

        /// <summary>
        /// This constructor is only used to instantiate the version of the object that resides within the AppDomain.
        /// </summary>
        [PublicAPI]
        public CrossAppDomainTracer()
        {
            Debug.WriteLine("CrossAppDomainTracer init() in hosted AppDomain " + Assembly.GetExecutingAssembly());

            _appDomainIdent = CrossAppDomainTrace.GetAppDomainIdent();
        }

        /// <summary>
        /// This constructor is only used to instantiate the local version of the object,
        /// which injects the <c>CrossAppDomainTracer</c> into the specified <c>AppDomain</c> instance.
        /// </summary>
        /// <param name="appDomain"> The app domain to install the <c>CrossAppDomainTracer</c> into. </param>
        internal CrossAppDomainTracer(AppDomain appDomain)
        {
            Debug.WriteLine("CrossAppDomainTracer init(appDomain) in local AppDomain " + Assembly.GetExecutingAssembly());

            _appDomainIdent = CrossAppDomainTrace.GetAppDomainIdent();

            // Create instance of the trace listener in the "far" app domain.
            _remoteTracer = AppDomainSetupUtils
                .CreateObjectInstanceInAppDomain<CrossAppDomainTracer>(appDomain);

            if (_remoteTracer == null)
            {
                throw new NullReferenceException(
                    "Unable to instantiate remote trace object in AppDomain " + appDomain);
            }

            if (Debugger.IsAttached)
            {
                Trace.TraceInformation("Skip starting remote logger if Debugger is attached.");
                return;
            }

            try
            {
                _remoteTracer.StartListening(this);
            }
            catch (Exception exc)
            {
                string msg = $"Unable to start logger in AppDomain '{appDomain.FriendlyName}'";
                Trace.TraceWarning($"{msg} - {exc.GetType().FullName} - {exc}");
#if DEBUG
                throw new AggregateException(msg, exc);
#endif
            }
        }

        [PublicAPI]
        public void StartListening(CrossAppDomainTracer remoteTracer)
        {
            Debug.WriteLine("CrossAppDomainTracer.StartListening");

            _remoteTracer = remoteTracer;

            _localListener = new ActionTraceListener(SendToRemoteLogger);
            Trace.Listeners.Add(_localListener);
        }

        public void SendToRemoteLogger(string message)
        {
            if (_remoteTracer == null) return; // Fast-path return if no remote tracer.

            // Use .NET thread pool to perform remote log write asynchronously.
            Task.Run(() => _remoteTracer.RemoteWrite(_appDomainIdent, message));
        }

        public void RemoteWrite(string sourceDomain, string message, params object[] args)
        {
            string logMsg = string.Format(message, args);
            if (ShowAppDomainName && !_appDomainIdent.Equals(sourceDomain))
            {
                // Add remote domain ident to log message.
                logMsg = sourceDomain + ": " + logMsg;
            }

            Trace.Write(logMsg);
        }

        #region IDisposable Support
        private bool _disposed; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Debug.WriteLine("CrossAppDomainTracer.Dispose");

            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    if (_localListener != null)
                    {
                        _localListener.Dispose();
                        _localListener = null;
                    }

                    _remoteTracer = null;
                }

                _disposed = true;
            }
        }
        #endregion
    }

    [ExcludeFromCodeCoverage]
    internal class ActionTraceListener : TraceListener
    {
        private readonly Action<string> _action;

        public ActionTraceListener(Action<string> action)
        {
            Debug.WriteLine("ActionTraceListener.Constructor");

            _action = action;
        }

        public override void Write(string message)
        {
            _action(message);
        }

        public override void WriteLine(string message)
        {
            _action(message + Environment.NewLine);
        }
    }
}
