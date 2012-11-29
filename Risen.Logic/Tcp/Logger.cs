using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Risen.Server.Tcp
{
    public interface ILogger
    {
        void WriteLine(LogCategory logCategory, string line);
        void WriteData(List<DataHolder> dataHolders, IListenerConfiguration listenerConfiguration);
    }

    public class Logger : ILogger
    {
        private readonly object _mutex = new object();
        private readonly StreamWriter _streamWriter;
        private readonly bool _shouldLogToConsole;

        public Logger(bool shouldLogToConsole, bool isLoggerEnabled)
        {
            SaveFile = GetSaveFileName(); //We create a new log file every time we run the app.
            _streamWriter = new StreamWriter(SaveFile); // create a writer and open the file
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

        public void WriteData(List<DataHolder> dataHolders, IListenerConfiguration listenerConfiguration)
        {
            WriteLine(LogCategory.Info, "\r\n\r\nData from DataHolders in listOfDataHolders follows:\r\n");

                for (int i = 0; i < dataHolders.Count(); i++)
                {
                    DataHolder dataHolder = dataHolders[i];
                    WriteLine(LogCategory.Info, IPAddress.Parse(((IPEndPoint)dataHolder.RemoteEndpoint).Address.ToString()) + ": " + ((IPEndPoint)dataHolder.RemoteEndpoint).Port.ToString() + ", " + dataHolder.ReceivedTransmissionId + ", " + Encoding.ASCII.GetString(dataHolder.DataMessageReceived));
                }

                WriteLine(LogCategory.Info, "\r\nHighest # of simultaneous connections was " + SocketListener.MaxSimultaneousClientsThatWereConnected);
                WriteLine(LogCategory.Info, "# of transmissions received was " + (listenerConfiguration.MainTransmissionId - SocketListener.InitialTransmissionId));
        }
    }

    public enum LogCategory
    {
        Info,
        Warning,
        Error
    }
}