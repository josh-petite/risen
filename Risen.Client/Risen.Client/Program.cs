using StructureMap;

namespace Risen.Client
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (var game = ObjectFactory.GetInstance<IMainGame>())
                game.Run();
        }
    }
}

