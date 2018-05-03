// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System.Diagnostics;
using System.Reflection;

namespace Server.Host
{
    /// <summary>
    /// Version info for an assembly.
    /// </summary>
    /// <remarks>
    /// Based on the <c>Orleans.Runtime.RuntimeVersion</c> class.
    /// https://github.com/dotnet/orleans/blob/master/src/Orleans.Core/Runtime/RuntimeVersion.cs
    /// </remarks>
    public class AssemblyVersionInfo
    {
        private readonly FileVersionInfo _versionInfo;
        private readonly AssemblyName _assemblyName;
        private readonly bool _isDebugBuild;

        /// <summary>
        /// Constructor - Record the version info for the specified assembly.
        /// </summary>
        /// <param name="assembly"> The assembly to be inspected. </param>
        public AssemblyVersionInfo(Assembly assembly)
        {
            _assemblyName = assembly.GetName();
            _versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            // NOTE: FileVersionInfo.IsDebug; does not work here.
            _isDebugBuild = IsAssemblyDebugBuild(assembly);
        }

        /// <summary>
        /// The full version string of the library.
        /// eg: '2012.5.9.51607 Build:12345 Timestamp: 20120509-185359'
        /// </summary>
        public string Current
        {
            get
            {
                string productVersion = _versionInfo.ProductVersion + (_isDebugBuild ? " (Debug)" : " (Release)");
                return string.IsNullOrEmpty(productVersion) ? ApiVersion : productVersion;
            }
        }

        /// <summary>
        /// The ApiVersion of the library.
        /// eg: '1.0.0.0'
        /// </summary>
        public string ApiVersion
        {
            get
            {
                return _assemblyName.Version.ToString();
            }
        }

        /// <summary>
        /// The FileVersion of the library.
        /// eg: '2012.5.9.51607'
        /// </summary>
        public string FileVersion
        {
            get
            {
                string fileVersion = _versionInfo.FileVersion;
                return string.IsNullOrEmpty(fileVersion) ? ApiVersion : fileVersion;
            }
        }

        /// <summary>
        /// ToString method.
        /// </summary>
        /// <returns> Returns summary string for this assembly version. </returns>
        public override string ToString()
        {
            return Current;
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
