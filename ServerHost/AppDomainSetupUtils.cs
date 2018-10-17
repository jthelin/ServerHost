using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;

namespace Server.Host
{
    public static class AppDomainSetupUtils
    {
        private static readonly object[] NoArgs = new object[0];

        /// <summary>
        /// Create a new instance of the trace listener in the specified app domain.
        /// </summary>
        /// <typeparam name="T"> The type of object to be created. </typeparam>
        /// <param name="appDomain"> The AppDomain where the "far" tracer object should be created. </param>
        /// <returns> An instance of the remote trace instance. </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T CreateObjectInstanceInAppDomain<T>(
            AppDomain appDomain,
            object[] args = null)
            where T : class
        {
            string appDomainName = appDomain.FriendlyName;
            string className = typeof(T).GetTypeInfo().FullName;
            string assemblyFilePath = typeof(T).GetTypeInfo().Assembly.Location;

            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException("Cannot determine class name for " + typeof(T));
            }
            if (string.IsNullOrEmpty(assemblyFilePath))
            {
                throw new ArgumentNullException("Cannot determine assembly location for " + typeof(T));
            }

            // Create instance of the class in the "far" app domain.

            ObjectHandle remoteObjRef = appDomain.CreateInstanceFrom(
                assemblyFilePath,
                className,
                false,
                BindingFlags.Default,
                null,
                args ?? NoArgs,
                CultureInfo.CurrentCulture,
                NoArgs);

            object remoteObj = remoteObjRef.Unwrap();

            if (remoteObj == null)
            {
                throw new ArgumentNullException(
                    $"Could not create remote object instance {className}"
                    + $" from assembly {assemblyFilePath}"
                    + $" in AppDomain {appDomainName}");
            }

            T remoteTracer = remoteObj as T;

            if (remoteTracer == null)
            {
                Type type1 = remoteObj.GetType();
                Type type2 = typeof(T);
                string codebase1 = type1.GetTypeInfo().Assembly.CodeBase;
                string codebase2 = type2.GetTypeInfo().Assembly.CodeBase;

                throw new InvalidCastException(
                    $"Cannot cast server object {type1} from assembly {codebase1} to type {type2} from assembly {codebase2}");
            }

            return remoteTracer;
        }

        /// <summary>
        /// Construct AppDomain configuration metadata based on the current execution environment.
        /// </summary>
        /// <returns>AppDomainSetup info for creating an new child AppDomain.</returns>
        public static AppDomainSetup GetAppDomainSetupInfo()
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
    }
}
