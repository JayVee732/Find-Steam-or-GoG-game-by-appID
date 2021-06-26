using System;

namespace FindSteamOrGOGGameByAppID
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GOGGamePath.FindGameByAppID("1423049311"));
            Console.WriteLine(SteamGamePath.FindGameByAppID("730"));
            Console.ReadLine();
        }
    }
}
