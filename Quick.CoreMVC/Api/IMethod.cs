﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.CoreMVC.Api
{
    public interface IMethod
    {
        /// <summary>
        /// HTTP方法
        /// </summary>
        HttpMethod Method { get; }
        /// <summary>
        /// 方法名称
        /// </summary>
        String Name { get; }
        /// <summary>
        /// 方法描述
        /// </summary>
        String Description { get; }
        /// <summary>
        /// 标签
        /// </summary>
        String[] Tags { get; }
        /// <summary>
        /// 调用示例
        /// </summary>
        String InvokeExample { get; }
        /// <summary>
        /// 返回值示例
        /// </summary>
        String ReturnValueExample { get; }

        /// <summary>
        /// 输入参数类型
        /// </summary>
        Type InputType { get; }
        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Invoke(HttpContext context);
    }
}
