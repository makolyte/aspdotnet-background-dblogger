using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundDatabaseLogger.Logging
{
    public interface ILogRepository
    {
        public Task Insert(List<LogMessage> logMessages);
    }
}
