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

        /// <summary>
        /// 开始运行
        /// </summary>
        public void Run()
        {
            Run<Startup>();
        }

        /// <summary>
        /// 使用指定的Startup类开始运行
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        public void Run<TStartup>()
            where TStartup : class
        {
            var host = new WebHostBuilder()
               .UseKestrel()
               .UseContentRoot(Directory.GetCurrentDirectory())
               .UseUrls(urls)
               .UseStartup<TStartup>()
               .Build();
            host.Run();
        }
    }
}
