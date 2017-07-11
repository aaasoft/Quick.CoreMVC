using System;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Quick.CoreMVC.Server("http://*:3000");
            server.Run();
        }
    }
}