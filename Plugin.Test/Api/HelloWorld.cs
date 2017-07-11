﻿using Quick.CoreMVC.Node;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Plugin.Test.Api
{
    public class HelloWorld : AbstractMethod
    {
        public override string Name => "测试API";

        public override object Invoke(HttpContext context)
        {
            return new
            {
                First = "Hello",
                Last = "World!"
            };
        }
    }
}