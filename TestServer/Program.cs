// Copyright (c) Jorgen Thelin. All rights reserved.

using System;
using System.Reflection;
using log4net;
using log4net.Config;

// Read log4net configs fromApp.config file.
[assembly:XmlConfigurator]

namespace TestServer
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger("TestServer");

        public static void Main(string[] args)
        {
            string serverName = "MyTestServer";

            Server server = new Server(serverName);

            log.Info("Initializing Server");
            server.InitServer();

            log.Info("Running Server");
            int rc = server.Run();

            log.InfoFormat("{0}.exe finished with rc={1}", Assembly.GetEntryAssembly().GetName().Name, rc);
            Environment.Exit(rc);
        }
    }
}
