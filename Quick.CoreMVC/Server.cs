using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace Quick.CoreMVC
{
    public class Server
    {
        private string[] urls;
        public Server(params string[] urls)
        {
            this.urls = urls;
        }

        public void Run()
        {
            var host = new WebHostBuilder()
               .UseKestrel()
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseUrls(urls)
               .UseStartup<Startup>()
               .Build();

            host.Run();
        }
    }
}
