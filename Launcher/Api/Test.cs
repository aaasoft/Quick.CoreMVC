using Microsoft.AspNetCore.Http;
using Quick.CoreMVC.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Api
{
    public class Test : AbstractMethod
    {
        public override string Name => "获取测试消息";
        public override HttpMethod Method => HttpMethod.GET;

        public override async Task Invoke(HttpContext context)
        {
            var outstr = "Hello .NET Core!";
            var rep = context.Response;
            rep.Headers["Content-Type"] = "text/plain; charset=UTF-8";
            await rep.WriteAsync(outstr);
            context.SetRequestHandled(true);
        }
    }
}
