using Quick.CoreMVC.Node;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Program.Node
{
    public class HelloWorld : AbstractNode
    {
        public HelloWorld(params INode[] nodes) : base(nodes)
        {
            AddGetMethod(new Execute());
        }

        public class Execute : AbstractMethod
        {
            public override string Name => "HelloWorld";

            public override object Invoke(HttpContext context)
            {
                return new
                {
                    First = "Hello",
                    Last = "World"
                };
            }
        }
    }
}
