using Microsoft.AspNetCore.Http;
using Quick.CoreMVC.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Test.Api
{
    public class HelloWorld2 : AbstractMethod
    {
        public override string Name => "测试API";
        public override HttpMethod Method => HttpMethod.GET;

        public override async Task Invoke(HttpContext context)
        {
            context.SetRequestHandled(true);

            var rep = context.Response;
            await rep.WriteAsync("HelloWorld2!");
        }
    }
}
