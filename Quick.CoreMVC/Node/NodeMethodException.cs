using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.CoreMVC.Node
{
    /// <summary>
    /// Node方法异常，用于返回错误信息
    /// </summary>
    public class NodeMethodException : Exception
    {
        public object MethodData { get; protected set; }

        public NodeMethodException(string message)
            : this(-1, message)
        { }

        public NodeMethodException(int code, string message)
            : this(code, message, null)
        { }

        public NodeMethodException(int code, string message, object methodData)
            : base(message)
        {
            HResult = code;
            MethodData = methodData;
        }
    }
}
