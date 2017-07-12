using System;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentDir = System.IO.Directory.GetCurrentDirectory();
            currentDir = System.IO.Path.GetDirectoryName(currentDir);
            System.IO.Directory.SetCurrentDirectory(currentDir);

            var server = new Quick.CoreMVC.Server("http://*:3000");
            server.Run();
        }
    }
}