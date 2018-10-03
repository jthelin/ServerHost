// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace Server.Host
{
    /// <summary>
    /// A hosting / test framework class for loading server instances into individual AppDomains in current process.
    /// </summary>
    /// <remarks>Uses <c>log4net</c> for logging. See: http://logging.apache.org/log4net/</remarks>
    public static class ServerHost
    {
        private static readonly ILog Log = LogManager.GetLogger("ServerHost");

        private static readonly ConcurrentDictionary<string, AppDomain> LoadedAppDomains = new ConcurrentDictionary<string, AppDomain>();

        /// <summary>
        /// Create a new AppDomain and load a new instance of a server class into that AppDomain.
        /// </summary>
        /// <typeparam name="TServer">Type of the server.</typeparam>
        /// <param name="serverName">Name of this server instance.</param>
        /// <returns><c>ServerHostHandle</c> object containing metadata about the loaded server instance.</returns>
        /// <remarks>
        /// The <c>serverName</c> will be used for the name of the newly created AppDomain to host this new server instance.
        /// Multiple copies of a server can be created if they are each given a different server name.
        /// </remarks>
        public static ServerHostHandle<TServer> LoadServerInNewAppDomain<TServer>(
            string serverName)
            where TServer : MarshalByRefObject
        {
            if (string.IsNullOrEmpty(serverName)) {
                throw new ArgumentNullException (nameof(serverName), "Server name param cannot be blank.");
            }

            Type serverType = typeof(TServer);
            string assemblyName = serverType.GetTypeInfo().Assembly.GetName().Name;
            string serverAssembly = assemblyName + ".exe";
            string serverTypeName = serverType.FullName ?? serverType.Name;
            if (!File.Exists(serverAssembly))
            {
                serverAssembly = assemblyName + ".dll";
                if (!File.Exists(serverAssembly))
                {
                    throw new FileNotFoundException(
                        $"Cannot find file to load for server class {serverTypeName} from assembly {assemblyName}");
                }
            }

            AppDomainSetup setup = GetAppDomainSetupInfo();

            AppDomain appDomain = AppDomain.CreateDomain(serverName, null, setup);
            LoadedAppDomains[serverName] = appDomain;

            // The server class must have a public constructor which
            // accepts single parameter of server name.
            var args = new object[] { serverName };
            var noActivationAttributes = new object[0];

            object serverObj = appDomain.CreateInstanceFromAndUnwrap(
                serverAssembly, serverTypeName, false,
                BindingFlags.Default, null, args, CultureInfo.CurrentCulture,
                noActivationAttributes);

            TServer server = serverObj as TServer;

            if (server == null)
            {
                Type type1 = serverObj.GetType();
                Type type2 = typeof(TServer);
                string codebase1 = type1.GetTypeInfo().Assembly.CodeBase;
                string codebase2 = serverType.GetTypeInfo().Assembly.CodeBase;

                throw new InvalidCastException(
                    $"Cannot cast server object {type1} from assembly {codebase1} to type {type2} from assembly {codebase2}");
            }

            appDomain.UnhandledException += ReportUnobservedException;

            return new ServerHostHandle<TServer>
            {
                ServerName = serverName,
                Server = server,
                AppDomain = appDomain
            };
        }

        /// <summary>
        /// Unload the server in the specific AppDomain.
        /// </summary>
        /// <param name="appDomain">The AppDomain to be unloaded.</param>
        /// <remarks>
        /// No specific checks are done to confirm that the specified
        /// AppDomain was created by this class and/or contains a hosted server.
        /// </remarks>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void UnloadServerInAppDomain(AppDomain appDomain)
        {
            if (appDomain == null) return;

            string appDomainName = "UNKNOWN";

            try
            {
                appDomainName = appDomain.FriendlyName;

                appDomain.UnhandledException -= ReportUnobservedException;

                AppDomain.Unload(appDomain);
            }
            catch (CannotUnloadAppDomainException exc)
            {
                Log.Warn($"Unable to unload AppDomain {appDomainName} - " + exc.Message, exc);
            }
            catch (Exception exc)
            {
                Log.Warn($"Ignoring error unloading AppDomain {appDomainName} - " + exc.Message, exc);
            }
        }

        /// <summary>
        /// Unload all AppDomains for server instances that we previously created / loaded.
        /// </summary>
        public static void UnloadAllServers()
        {
            string[] serverNames = LoadedAppDomains.Keys.ToArray(); // Take working copy

            foreach (string serverName in serverNames)
            {
                if (LoadedAppDomains.TryRemove(serverName, out AppDomain appDomain))
                {
                    if (appDomain == null) continue;

                    string appDomainName = null;
                    try
                    {
                        // We may get AppDomainUnloadedException thrown here - X-ref: #46.

                        appDomainName = appDomain.FriendlyName;

                        Log.Info($"Unloading Server {serverName} in AppDomain {appDomainName}");

                        UnloadServerInAppDomain(appDomain);
                    }
                    catch (Exception exc)
                    {
                        Log.Warn($"Ignoring error unloading AppDomain {appDomainName ?? serverName}. " + exc.Message, exc);
                    }
                }
            }
        }

        /// <summary>
        /// Construct AppDomain configuration metadata based on the current execution environment.
        /// </summary>
        /// <returns>AppDomainSetup info for creating an new child AppDomain.</returns>
        private static AppDomainSetup GetAppDomainSetupInfo()
        {
            AppDomain currentAppDomain = AppDomain.CurrentDomain;

            return new AppDomainSetup
            {
                ApplicationBase = Directory.GetCurrentDirectory(),
                ConfigurationFile = currentAppDomain.SetupInformation.ConfigurationFile,
                ShadowCopyFiles = currentAppDomain.SetupInformation.ShadowCopyFiles,
                ShadowCopyDirectories = currentAppDomain.SetupInformation.ShadowCopyDirectories,
                CachePath = currentAppDomain.SetupInformation.CachePath
            };
        }

        /// <summary>
        /// Event handle method to report any unhandled exceptions in a newly created AppDomain.
        /// </summary>
        /// <remarks>
        /// This event handler is automatically
        /// hooked up when AppDomain was created in <c>LoadServerInNewAppDomain</c>,
        /// and unhooked when AppDomain is unloaded in <c>UnloadServerInAppDomain</c>.
        /// </remarks>
        /// <remarks>Used <c>log4net</c> framework to report error details.</remarks>
        [ExcludeFromCodeCoverage]
        private static void ReportUnobservedException(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            Exception exception = (Exception) eventArgs.ExceptionObject;
            Log.Warn("Unobserved exception: " + exception);
        }
    }
}
