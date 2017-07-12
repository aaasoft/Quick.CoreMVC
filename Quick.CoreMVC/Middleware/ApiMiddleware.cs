using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Quick.CoreMVC.Hunter;
using Quick.CoreMVC.Node;
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
            var dllDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var di = new DirectoryInfo(dllDir);
            foreach (var file in di.GetFiles("Plugin.*.dll"))
            {
                var assemblyName = Path.GetFileNameWithoutExtension(file.Name);
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IMethod).IsAssignableFrom(type))
                    {
                        var methodPath = type.FullName;
                        if (methodPath.StartsWith(assemblyName))
                            methodPath = methodPath.Substring(assemblyName.Length + 1);
                        methodPath = methodPath.Replace('.', '/');
                        methodPath = $"/{assemblyName}/{methodPath}";
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


            Object data = null;
            //调用
            var invokeTime = DateTime.Now;
            data = apiMethod.Invoke(context);
            //如果返回值不是ApiResult
            if (!(data is ApiResult))
            {
                var ret = ApiResult.Success(data);
                ret.SetMetaInfo("usedTime", (DateTime.Now - invokeTime).ToString());
                data = ret;
            }
            //try
            //{
            //    //调用方法处理
            //    if (NodeManager.Instance.MethodInvokeHandler != null)
            //        nodeMethod = NodeManager.Instance.MethodInvokeHandler.Invoke(nodeMethod, context);
            //    //调用
            //    var invokeTime = DateTime.Now;
            //    data = nodeMethod?.Invoke(context);
            //    //返回值处理
            //    if (NodeManager.Instance.ReturnValueHandler != null)
            //        data = NodeManager.Instance.ReturnValueHandler.Invoke(nodeMethod, data, invokeTime);
            //    //如果返回值不是ApiResult
            //    if (!(data is ApiResult))
            //    {
            //        var message = $"{nodeMethod.Name}成功";
            //        var ret = ApiResult.Success(message, data);
            //        ret.SetMetaInfo("usedTime", (DateTime.Now - invokeTime).ToString());
            //        data = ret;
            //    }
            //}
            //catch (NodeMethodException ex)
            //{
            //    data = ApiResult.Error(ex.HResult, ex.Message, ex.MethodData);
            //}
            //catch (NodeMethodHandledException)
            //{
            //    return Task.Delay(0);
            //}
            //catch (Exception ex)
            //{
            //    if (NodeManager.Instance.ExceptionHandler == null)
            //        throw ex;
            //    data = NodeManager.Instance.ExceptionHandler.Invoke(ex);
            //}
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
            rep.Headers["Expires"] = DateTime.Now.ToString("R");
            return context.Output(encoding.GetBytes(result), true);
        }

        public void Hunt(IDictionary<string, string> properties)
        {
            //NodeManager.Instance.Init(properties);
        }
    }
}
