using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Quick.CoreMVC.Utils
{
    public class PathUtils
    {
        /// <summary>
        /// 获取程序基础目录
        /// </summary>
        /// <returns></returns>
        public static string GetBaseDirectory()
        {
            var baseFile = System.Reflection.Assembly.GetEntryAssembly().Location;
            return Path.GetDirectoryName(baseFile);
        }

        /// <summary>
        /// 获取插件目录
        /// </summary>
        /// <returns></returns>
        public static string GetPluginDirectory()
        {
            return Path.Combine(GetBaseDirectory(), "Plugins");
        }

        /// <summary>
        /// 获取插件目录下的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathInPluginDirectory(string path)
        {
            return Path.Combine(GetPluginDirectory(), path);
        }
    }
}
