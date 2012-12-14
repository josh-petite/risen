﻿using System;

namespace Risen.Shared.Msmq
{
    public class LogMessage
    {
        public LogCategory Category { get; set; }
        public LogSeverity Severity { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} - {2} <{3}>", Category, Severity, Description, CreatedDate.ToString("hh:mm:ss.ff - yyyy-MM-dd"));
        }

        public static LogMessage Create(LogCategory logCategory, LogSeverity logSeverity, string description)
        {
            return new LogMessage
                {
                    Category = logCategory,
                    Severity = logSeverity,
                    Description = description,
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now
                };
        }
    }

    public enum LogSeverity
    {
        Debug,
        Warning,
        Error
    }

    public enum LogCategory
    {
        TcpServer,
        Logic,
        Caching
    }
}
