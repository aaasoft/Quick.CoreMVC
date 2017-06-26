using System;
using System.Threading;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            Quick.CoreMVC.Node.NodeManager.Instance.Register(new Node.HelloWorld());
            var server = new Quick.CoreMVC.Server("http://*:3000");
            server.Run();
        }
    }
}