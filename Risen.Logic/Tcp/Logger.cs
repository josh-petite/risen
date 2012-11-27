using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Risen.Server.Tcp
{
    public interface ILogger
    {
        void WriteLine(string socketListenerConstructorComplete);
    }

    public class Logger : ILogger
    {
        private readonly object _mutex = new object();
        private readonly StreamWriter _streamWriter;
        private readonly bool _shouldLogToConsole;

        public Logger(bool shouldLogToConsole)
        {
            SaveFile = GetSaveFileName(); //We create a new log file every time we run the app.
            _streamWriter = new StreamWriter(SaveFile); // create a writer and open the file
            _shouldLogToConsole = shouldLogToConsole;
        }

        public string SaveFile { get; set; }

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

        public void WriteLine(string lineToWrite)
        {
            if (_shouldLogToConsole)
                Console.WriteLine(lineToWrite);

            lock (_mutex)
                _streamWriter.WriteLine(lineToWrite);
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
}