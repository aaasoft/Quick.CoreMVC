using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Quick.CoreMVC.Api;
using Quick.Plugin;
using Quick.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Quick.CoreMVC.Middleware
{
    public class ApiMiddleware : IHungryPropertyHunter
    {
        private RequestDelegate _next;

        public const string JSONP_CALLBACK = "callback";
        public static ApiMiddleware Instance { get; private set; }

        private Encoding encoding = new UTF8Encoding(false);
        private Dictionary<string, IMethod> apiMethodDict = new Dictionary<string, IMethod>();

        public ApiMiddleware(RequestDelegate next = null)
        {
            _next = next;
            //扫描加载的程序集
            foreach (var pluginInfo in PluginManager.Instance.GetAllPlugins())
            {
                var assembly = Assembly.Load(new AssemblyName(pluginInfo.Id));
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IMethod).IsAssignableFrom(type))
                    {
                        var methodPath = type.FullName;
                        if (methodPath.StartsWith(pluginInfo.Id))
                        {
                            methodPath = methodPath.Substring(pluginInfo.Id.Length + 1);
                            methodPath = methodPath.Replace('.', '/');
                            methodPath = $"/{pluginInfo.Id}/{methodPath}";
                        }
                        else
                        {
                            methodPath = methodPath.Replace('.', '/');
                        }
                        var method = Activator.CreateInstance(type) as IMethod;
                        apiMethodDict[methodPath] = method;
                    }
                }
            }
        }

        public Task Invoke(HttpContext context)
        {
            var req = context.Request;
            var rep = context.Response;

            var path = req.Path.Value;
            HttpMethod currentHttpMethod;
            IMethod apiMethod;
            if (
                //如果路径为空
                string.IsNullOrEmpty(path)
                //如果API方法未找到此路径
                || !apiMethodDict.TryGetValue(path, out apiMethod)
                //如果HTTP方法不能转换为枚举
                || !Enum.TryParse(req.Method, out currentHttpMethod)
                //如果HTTP方法与Api方法注册的HTTP方法不匹配
                || apiMethod.Method != (apiMethod.Method | currentHttpMethod))
                return _next.Invoke(context);



            //调用
            var invokeTime = DateTime.Now;
            var task = apiMethod.Invoke(context);
            if (context.GetRequestHandled())
                return task;

            return task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return Task.FromResult(0);

                var taskType = task.GetType();
                //读取任务结果
                var pi = taskType.GetProperty(nameof(Task<object>.Result));
                Object data = pi.GetValue(t);
                //如果返回值不是ApiResult
                if (!(data is ApiResult))
                {
                    var ret = ApiResult.Success(data);
                    ret.SetMetaInfo("usedTime", (DateTime.Now - invokeTime).ToString());
                    data = ret;
                }

                //要输出的内容
                string result = null;
                //JSON序列化的结果
                var json = JsonConvert.SerializeObject(data/*, NodeManager.Instance.JsonSerializerSettings*/);
                var jsonpCallback = req.Query[JSONP_CALLBACK];

                if (string.IsNullOrEmpty(jsonpCallback))
                {
                    rep.ContentType = "text/json; charset=UTF-8";
                    result = json;
                }
                else
                {
                    rep.ContentType = "application/x-javascript";
                    result = $"{jsonpCallback}({json})";
                }
                rep.Headers["Cache-Control"] = "no-cache";
                return context.Output(encoding.GetBytes(result), true);
            });
        }

        public void Hunt(IDictionary<string, string> properties)
        {
            //NodeManager.Instance.Init(properties);
        }
    }
}
