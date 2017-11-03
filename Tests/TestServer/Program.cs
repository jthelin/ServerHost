// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Reflection;
using log4net;
using log4net.Config;

// Read log4net configs fromApp.config file.
[assembly:XmlConfigurator]

namespace Server.Host.Tests.TestServer
{
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("TestServer");

        public static void Main(string[] args)
        {
            const string serverName = "MyTestServer";

            Server server = new Server(serverName);

            Log.InfoFormat("Initializing Server {0} with args = {1}", serverName, string.Join(", ", args));
            server.InitServer();

            Log.Info("Running Server");
            int rc = server.Run();

            Log.InfoFormat("{0}.exe finished with rc={1}", Assembly.GetEntryAssembly().GetName().Name, rc);
            Environment.Exit(rc);
        }
    }
}
