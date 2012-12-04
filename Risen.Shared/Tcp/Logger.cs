using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Risen.Shared.Tcp
{
    public interface ILogger
    {
        void WriteLine(LogCategory logCategory, string line);
    }

    public class Logger : ILogger
    {
        private readonly object _mutex = new object();
        private readonly StreamWriter _streamWriter;
        private readonly bool _shouldLogToConsole;

        public Logger(bool shouldLogToConsole, bool isLoggerEnabled)
        {
            //SaveFile = GetSaveFileName(); // *** NEED TO REFACTOR TO LOG TO THE DB INSTEAD OF THIS FILE GARBAGE
            //_streamWriter = new StreamWriter(SaveFile); // create a writer and open the file
            _shouldLogToConsole = shouldLogToConsole;
            IsEnabled = isLoggerEnabled;
        }

        public string SaveFile { get; set; }
        public bool IsEnabled { get; set; }

        private string GetSaveFileName()
        {
            var saveDirectory = ConfigurationManager.AppSettings["SaveDirectory"];            

            try
            {
                if (Directory.Exists(saveDirectory) == false)
                    Directory.CreateDirectory(saveDirectory);
            }
            catch
            {
                Console.WriteLine("Could not create save directory for log. See TestFileWriter.cs."); Console.ReadLine();
            }

            string assemblyFullName = Assembly.GetExecutingAssembly().FullName;
            int index = assemblyFullName.IndexOf(',');
            string saveFile = assemblyFullName.Substring(0, index);
            string dt = DateTime.Now.ToString("yyMMddHHmmss");

            saveFile = saveDirectory + saveFile + "-" + dt + ".txt"; //Save directory is created in ConfigFileHandler

            return saveFile;
        }

        public void WriteLine(LogCategory logCategory, string lineToWrite)
        {
            if (!IsEnabled)
                return;

            var formattedLine = string.Format("{0}: {1}", logCategory, lineToWrite);

            if (_shouldLogToConsole)
                Console.WriteLine(formattedLine);

            lock (_mutex)
                _streamWriter.WriteLine(formattedLine);
        }

        public void Close()
        {
            _streamWriter.Close();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("This session was logged to " + SaveFile);
            Console.WriteLine();
            Console.WriteLine();
        }
    }

    public enum LogCategory
    {
        Info,
        Warning,
        Error
    }
}