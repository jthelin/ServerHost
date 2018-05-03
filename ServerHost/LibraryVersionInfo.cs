// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System.Reflection;

namespace Server.Host
{
    /// <summary>
    /// Version info for ServerHost library.
    /// </summary>
    public static class LibraryVersionInfo
    {
        private static readonly AssemblyVersionInfo libraryVersionInfo;

        static LibraryVersionInfo() {
            TypeInfo libraryTypeInfo = typeof(ServerHost).GetTypeInfo();
            Assembly me = libraryTypeInfo.Assembly;
            libraryVersionInfo = new AssemblyVersionInfo(me);
        }

        /// <summary>
        /// The full version string of the library.
        /// eg: '2012.5.9.51607 Build:12345 Timestamp: 20120509-185359'
        /// </summary>
        public static string Current => libraryVersionInfo.Current;

        /// <summary>
        /// The ApiVersion of the library.
        /// eg: '1.0.0.0'
        /// </summary>
        public static string ApiVersion => libraryVersionInfo.ApiVersion;

        /// <summary>
        /// The FileVersion of the library.
        /// eg: '2012.5.9.51607'
        /// </summary>
        public static string FileVersion => libraryVersionInfo.FileVersion;
    }
}
