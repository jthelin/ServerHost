// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Reflection;
using JetBrains.Annotations;
using log4net;
using log4net.Config;

// Read log4net configs fromApp.config file.
[assembly:XmlConfigurator]

namespace Server.Host.Tests.TestServer
{
    [PublicAPI]
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("TestServer");

        public static void Main(string[] args)
        {
            const string serverName = "MyTestServer";

            string progName = Assembly.GetEntryAssembly().GetName().Name;

            Server server = new Server(serverName);

            Log.Info($"Initializing Server {serverName} with args = " + string.Join(", ", args));
            server.InitServer();

            Log.Info("Running Server");
            int rc = server.Run();

            Log.Info($"{progName}.exe finished with rc={rc}");
            Environment.Exit(rc);
        }
    }
}
