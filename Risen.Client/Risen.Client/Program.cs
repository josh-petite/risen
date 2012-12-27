using System.Diagnostics;
using Risen.Client.Configuration;
using StructureMap;

namespace Risen.Client
{
    internal static class Program
    {
        internal static TextWriterTraceListener TraceListener = new TextWriterTraceListener(System.IO.File.CreateText("Output.txt"));

        private static void Main(string[] args)
        {
            Debug.Listeners.Add(TraceListener);
            ClientRegistry.Configure();

            using (var game = ObjectFactory.GetInstance<IGameMain>())
                game.Run();
        }
    }
}

