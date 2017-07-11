using Microsoft.AspNetCore.Http;
using Quick.CoreMVC.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Quick.CoreMVC.Middleware
{
    public class ResourceMiddleware
    {
        public static string Prefix = "/resource/";
        private RequestDelegate _next;
        public ResourceMiddleware(RequestDelegate next = null)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var req = context.Request;
            var rep = context.Response;

            var resourcePath = GetResourcePath(req.Path.Value);
            if (string.IsNullOrEmpty(resourcePath))
                return _next.Invoke(context);

            var pluginDir = PathUtils.GetPluginDirectory();
             
            return null;
        }

        /// <summary>
        /// 获取资源路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetResourcePath(string path)
        {
            if (path.StartsWith(Prefix))
                return path.Substring(Prefix.Length);
            return null;
        }
    }
}
