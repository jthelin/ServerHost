// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
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

        private static readonly List<AppDomain> LoadedAppDomains = new List<AppDomain>();

        /// <summary>
        /// Create a new AppDomain and load a new instance of a server class into that AppDomain.
        /// </summary>
        /// <typeparam name="TServer">Type of the server.</typeparam>
        /// <param name="serverName">Name of this server instance.</param>
        /// <returns><c>ServerHostHandle</c> object containing metadata about the loaded server instance.</returns>
        /// <remarks>
        /// The <c>serverName</c> will be used for the name of the newly created AppDomain to host this new server instance.
        /// Multiple copies of a server can be created if they arer each given a different server name.
        /// </remarks>
        public static ServerHostHandle<TServer> LoadServerInNewAppDomain<TServer>(
            string serverName) 
            where TServer : MarshalByRefObject
        {
            if (string.IsNullOrEmpty(serverName)) {
                throw new ArgumentNullException ("serverName", "Server name param cannot be blank.");
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
                    throw new FileNotFoundException(string.Format(
                        "Cannot find file to load for server class {0} from assembly {1}",
                        serverTypeName, assemblyName));
                }
            }

            AppDomainSetup setup = GetAppDomainSetupInfo();

            AppDomain appDomain = AppDomain.CreateDomain(serverName, null, setup);
            LoadedAppDomains.Add(appDomain);

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
                throw new InvalidCastException(string.Format(
                    "Cannot cast server object {0} from assembly {1} to type {2} from assembly {3}",
                    serverObj.GetType(), serverObj.GetType().GetTypeInfo().Assembly.CodeBase,
                    serverType, serverType.GetTypeInfo().Assembly.CodeBase));
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
            
            try
            {
                LoadedAppDomains.Remove(appDomain);
                appDomain.UnhandledException -= ReportUnobservedException;

                AppDomain.Unload(appDomain);
            }
            catch (Exception exc)
            {
                Log.WarnFormat("Ignoring error unloading AppDomain {0} - {1}",
                    appDomain.FriendlyName, exc);
            }
        }

        /// <summary>
        /// Unload all AppDomains for server instances that we previously created / loaded.
        /// </summary>
        public static void UnloadAllServers()
        {
            foreach (AppDomain appDomain in LoadedAppDomains.ToArray()) // Take working copy
            {
                if (appDomain == null) continue;
                
                Log.InfoFormat("Unloading AppDomain {0}", appDomain.FriendlyName);
                UnloadServerInAppDomain(appDomain);
            }
            LoadedAppDomains.Clear();
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
            Log.WarnFormat("Unobserved exception: {0}", exception);
        }
    }
}
