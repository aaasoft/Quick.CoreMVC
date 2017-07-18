using Quick.CoreMVC.Api;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Plugin.Test.Api
{
    public class HelloWorld : AbstractMethod
    {
        public override string Name => "测试API";
        public override HttpMethod Method => HttpMethod.GET;

        public override Task Invoke(HttpContext context, RequestDelegate next)
        {
            return Task.FromResult(new
            {
                First = "Hello",
                Last = "World!"
            });
        }
    }
}
