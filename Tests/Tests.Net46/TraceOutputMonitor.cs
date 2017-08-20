// Copyright (c) Jorgen Thelin. All rights reserved.
// Licensed under Apache 2.0 https://github.com/jthelin/ServerHost/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Server.Host.Tests.Net46
{
    internal class TraceOutputMonitor : TraceListener
    {
        private readonly List<string> _logMessages = new List<string>();

        public int Count
        {
            get
            {
                lock (_logMessages)
                {
                    return _logMessages.Count;
                }
            }
        }

        public IEnumerable<string> Messages
        {
            get
            {
                lock (_logMessages)
                {
                    return _logMessages.AsReadOnly();
                }
            }
        }

        public void Clear()
        {
            lock (_logMessages)
            {
                _logMessages.Clear();
            }
            Console.Write("TraceOutputMonitor.Clear");
        }

        #region Overrides of TraceListener

        /// <inheritdoc />
        public override void Write(string message)
        {
            lock (_logMessages)
            {
                _logMessages.Add(message);
            }
            Console.Write("TraceOutputMonitor.Write: {0}", message);
        }

        /// <inheritdoc />
        public override void WriteLine(string message)
        {
            lock(_logMessages)
            {
                _logMessages.Add(message);
            }
            Console.WriteLine("TraceOutputMonitor.WriteLine: {0}", message);
        }

        #endregion
    }
}
