using System;

namespace Server.Host
{
    /// <summary>
    /// Provide a Unique Id for the runtime enviroment in each app domain / process.
    /// </summary>
    public static class ExecId
    {
        private static Guid _ExecId; // Controlled by lock (_ExecIdLock)
        private static readonly object _ExecIdLock = new object();

        /// <summary>
        /// Unique Id will be created in each app domain / process.
        /// </summary>
        public static Guid Value
        {
            get
            {
                Guid execId;
                lock (_ExecIdLock)
                {
                    if (_ExecId == default(Guid))
                    {
                        _ExecId = Guid.NewGuid(); // Unique Id will be created in each app domain / process.
                    }

                    execId = _ExecId;
                }

                return execId;
            }
        }
    }
}
