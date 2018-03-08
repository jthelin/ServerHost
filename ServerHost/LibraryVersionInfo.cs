// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System.Diagnostics;
using System.Reflection;

namespace Server.Host
{
    /// <summary>
    /// Version info for ServerHost library.
    /// </summary>
    /// <remarks>
    /// Based on the <c>Orleans.Runtime.RuntimeVersion</c> class.
    /// https://github.com/dotnet/orleans/blob/master/src/Orleans/Runtime/RuntimeVersion.cs
    /// </remarks>
    public static class LibraryVersionInfo
    {
        private static readonly TypeInfo libraryTypeInfo = typeof(ServerHost).GetTypeInfo();

        /// <summary>
        /// The full version string of the library.
        /// eg: '2012.5.9.51607 Build:12345 Timestamp: 20120509-185359'
        /// </summary>
        public static string Current
        {
            get
            {
                Assembly me = libraryTypeInfo.Assembly;
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(me.Location);
                // NOTE: progVersionInfo.IsDebug; does not work here.
                bool isDebug = IsAssemblyDebugBuild(me);
                string productVersion = versionInfo.ProductVersion + (isDebug ? " (Debug)" : " (Release)");
                return string.IsNullOrEmpty(productVersion) ? ApiVersion : productVersion;
            }
        }

        /// <summary>
        /// The ApiVersion of the library.
        /// eg: '1.0.0.0'
        /// </summary>
        public static string ApiVersion
        {
            get
            {
                Assembly me = libraryTypeInfo.Assembly;
                AssemblyName libraryInfo = me.GetName();
                return libraryInfo.Version.ToString();
            }
        }

        /// <summary>
        /// The FileVersion of the library.
        /// eg: '2012.5.9.51607'
        /// </summary>
        public static string FileVersion
        {
            get
            {
                Assembly me = libraryTypeInfo.Assembly;
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(me.Location);
                string fileVersion = versionInfo.FileVersion;
                return string.IsNullOrEmpty(fileVersion) ? ApiVersion : fileVersion;
            }
        }

        private static bool IsAssemblyDebugBuild(Assembly assembly)
        {
            foreach (object attribute in assembly.GetCustomAttributes(false))
            {
                DebuggableAttribute debuggableAttribute = attribute as DebuggableAttribute;
                if (debuggableAttribute != null)
                {
                    return debuggableAttribute.IsJITTrackingEnabled;
                }
            }
            return false;
        }
    }
}
