using Quick.Plugin;
using Quick.Properties.Utils;
using System;
using System.IO;
using System.Reflection;

namespace Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var startupCurrentDir = System.IO.Directory.GetCurrentDirectory();
#if DEBUG
            startupCurrentDir = System.IO.Path.GetDirectoryName(startupCurrentDir);
            Directory.SetCurrentDirectory(startupCurrentDir);
#endif
            //加载配置
#if DEBUG
            var properties = PropertyUtils.Load("include=Plugin.*/*.properties", startupCurrentDir);
#else
            var properties = PropertyUtils.Load("include=*.properties", startupCurrentDir);
#endif
            //初始化插件
#if DEBUG
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
#endif
            PluginManager.Instance.Init();
            foreach (var pluginInfo in PluginManager.Instance.GetAllPlugins())
            {
                if (pluginInfo.Activator == null)
                    continue;
                HunterUtils.TryHunt(pluginInfo.Activator, properties);
            }
            //启动插件
            PluginManager.Instance.Start();

#if DEBUG
            Directory.SetCurrentDirectory(startupCurrentDir);
#endif

            var server = new Quick.CoreMVC.Server("http://*:3000");
            server.Run<Startup>();
        }
    }
}