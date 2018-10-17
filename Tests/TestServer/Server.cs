// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Diagnostics;
using log4net;
using log4net.Config;

namespace Server.Host.Tests.TestServer
{
    public class Server : MarshalByRefObject
    {
        private static readonly ILog Log = LogManager.GetLogger("TestServer.Server");

        public Server(string serverName)
        {
            // Initialize log4net logging.
            XmlConfigurator.Configure();

            Log.Info($"Server - {serverName}");
        }

        public void InitServer()
        {
            Log.Info("InitServer - log4net");
            Debug.WriteLine("InitServer - Debug.WriteLine");
            Trace.TraceInformation("InitServer - Trace.TraceInformation");
        }

        public int Run()
        {
            Log.Info("Run: I am a server!");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            return 0;
        }
    }
}
