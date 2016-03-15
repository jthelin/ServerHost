// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed with Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using log4net;

namespace Server.Host.Tests.TestServer
{
    public class Server : MarshalByRefObject
    {
        private static readonly ILog log = LogManager.GetLogger("TestServer.Server");

        public Server(string serverName)
        {
            log.InfoFormat("Server - {0}", serverName);
        }

        public void InitServer()
        {
            log.Info("InitServer!");
        }

        public int Run()
        {
            log.Info("Run: I am a server!");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            return 0;
        }
    }
}
