// Copyright (c) Jorgen Thelin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using log4net;

namespace ServerHost
{
    /// <summary>
    /// Data holder class for information related to a hosted in-process test server instance.
    /// </summary>
    /// <typeparam name="TServer">Type of the server.</typeparam>
    public class ServerHostHandle<TServer> 
        where TServer : MarshalByRefObject
    {
        public string ServerName { get; internal set; }
        public TServer Server { get; internal set; }
        public AppDomain AppDomain { get; internal set; }
    }

    /// <summary>
    /// A test framework class for loading server instances into individual AppDomains in current process.
    /// </summary>
    /// <remarks>Uses <c>log4net</c> for logging.</remarks>
    /// <see cref="http://logging.apache.org/log4net/"/>
    public static class ServerTestHost
    {
        private static readonly ILog log = LogManager.GetLogger("ServerTestHost");

        private static readonly List<AppDomain> loadedAppDomains = new List<AppDomain>();

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
            Type serverType = typeof(TServer);
            string assemblyName = serverType.Assembly.GetName().Name;
            string serverAssembly = assemblyName + ".exe";
            if (!File.Exists(serverAssembly))
            {
                serverAssembly = assemblyName + ".dll";
                if (!File.Exists(serverAssembly))
                {
                    throw new FileNotFoundException(string.Format(
                        "Cannot find file to load for server class {0} from assembly {1}",
                        serverType.FullName, assemblyName));
                }
            }

            AppDomainSetup setup = GetAppDomainSetupInfo();

            AppDomain appDomain = AppDomain.CreateDomain(serverName, null, setup);
            loadedAppDomains.Add(appDomain);

            // The server class must have a public constructor which 
            // accepts single parameter of server name.
            var args = new object[] { serverName };
            var noActivationAttributes = new object[0];

            object serverObj = appDomain.CreateInstanceFromAndUnwrap(
                serverAssembly, serverType.FullName, false,
                BindingFlags.Default, null, args, CultureInfo.CurrentCulture,
                noActivationAttributes);

            appDomain.UnhandledException += ReportUnobservedException;

            return new ServerHostHandle<TServer>
            {
                ServerName = serverName,
                Server = (TServer) serverObj,
                AppDomain = appDomain,
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
        public static void UnloadServerInAppDomain(AppDomain appDomain)
        {
            if (appDomain != null)
            {
                try
                {
                    loadedAppDomains.Remove(appDomain);
                    appDomain.UnhandledException -= ReportUnobservedException;

                    AppDomain.Unload(appDomain);
                }
                catch (Exception exc)
                {
                    log.WarnFormat("Ignoring error unloading AppDomain {0} - {1}",
                        appDomain.FriendlyName, exc);
                }
            }
        }

        /// <summary>
        /// Unload all AppDomains for server instances that we previously created / loaded.
        /// </summary>
        public static void UnloadAllServers()
        {
            foreach (AppDomain appDomain in loadedAppDomains.ToArray()) // Take working copy
            {
                if (appDomain != null)
                {
                    log.InfoFormat("Unloading AppDomain {0}", appDomain.FriendlyName);
                    UnloadServerInAppDomain(appDomain);
                }
            }
            loadedAppDomains.Clear();
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
                ApplicationBase = Environment.CurrentDirectory,
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
            log.WarnFormat("Unobserved exception: {0}", exception);
        }
    }
}
