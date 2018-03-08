// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;

namespace Server.Host
{
    /// <summary>
    /// Data holder class for information related to a hosted in-process test server instance.
    /// </summary>
    /// <typeparam name="TServer">Type of the server.</typeparam>
    public class ServerHostHandle<TServer>
    {
        internal ServerHostHandle()
        {
            // Prevent the arbitrary creation of ServerHostHandle objects.
        }

        /// <summary> The name of this server. </summary>
        public string ServerName { get; internal set; }
        /// <summary> Reference to the Server instance in the hosted AppDomain.
        /// This should be a <c>MarshalByRefObject</c> to allow cross-domain API calls.</summary>
        public TServer Server { get; internal set; }
        /// <summary> Reference to the AppDomain this server is running in. </summary>
        public AppDomain AppDomain { get; internal set; }
    }
}
