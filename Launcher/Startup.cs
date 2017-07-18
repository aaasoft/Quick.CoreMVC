using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Quick.CoreMVC.Middleware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Launcher
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.CookieName = "test.sid";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.CookieHttpOnly = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
#if DEBUG
            loggerFactory.AddConsole();
            app.UseDeveloperExceptionPage();
#endif
            //支持Session
            app.UseSession();

            //支持登录控制
            app.UseMiddleware<Launcher.Middleware.LoginMiddleware>();

            //支持静态文件
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory())
            });
            //支持API中间件
            app.UseMiddleware<Quick.CoreMVC.Middleware.ApiMiddleware>(new object[]
            {
                new Dictionary<string,string>()
                {
                    ["Hello"] = "World!"
                }
            });
            app.Run(async (context) =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("404 NOT FOUND");
            });
        }
    }
}
