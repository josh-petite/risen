using Risen.Client.Configuration;
using StructureMap;

namespace Risen.Client
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            ClientRegistry.Configure();

            using (var game = ObjectFactory.GetInstance<IGameMain>())
                game.Run();
        }
    }
}

