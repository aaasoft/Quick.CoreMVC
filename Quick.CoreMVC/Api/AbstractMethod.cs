using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Quick.CoreMVC.Api
{
    public abstract class AbstractMethod : IMethod
    {

        public abstract string Name { get; }
        public abstract HttpMethod Method { get; }
        public virtual Type InputType { get; } = null;
        public virtual string Description { get; } = String.Empty;
        public virtual string InvokeExample { get; } = String.Empty;
        public virtual string ReturnValueExample { get; } = String.Empty;
        public virtual string[] Tags { get; }

        /// <summary>
        /// 处理文件上传
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="contentDisposition"></param>
        /// <param name="buffer"></param>
        /// <param name="bytes"></param>
        public virtual void HandleFileUpload(HttpContext context, string name, string fileName, string contentType, string contentDisposition, byte[] buffer, int bytes) { }
        /// <summary>
        /// 完成文件上传
        /// </summary>
        public virtual void FinishFileUpload(HttpContext context) { }

        public abstract Task Invoke(HttpContext context);
    }

    public abstract class AbstractMethod<TInput> : AbstractMethod
                where TInput : class
    {
        public override Type InputType { get; } = typeof(TInput);

        /// <summary>
        /// 处理参数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual TInput HandleParameter(HttpContext context, TInput input) => input;

        public override Task Invoke(HttpContext context)
        {
            ApiParameterAttribute attribute = typeof(TInput).GetTypeInfo()
                .GetCustomAttributes(typeof(ApiParameterAttribute), false)
                .FirstOrDefault() as ApiParameterAttribute;

            bool valueToObject = false;
            String[] ignoreProperties = null;

            if (attribute != null)
            {
                valueToObject = attribute.ValueToObject;
                ignoreProperties = attribute.IgnoreProperties;
            }

            TInput input;
            try
            {
                if (typeof(TInput) == typeof(object))
                {
                    input = default(TInput);
                }
                else
                {
                    if (context.Request.Method == "GET")
                        input = context.GetQueryData<TInput>(valueToObject, ignoreProperties);
                    //else if (context.Request.ContentType != null
                    //    && context.Request.ContentType.StartsWith("multipart/form-data;"))
                    //{
                    //    IDictionary<string, string[]> paramDict = new Dictionary<string, string[]>();
                    //    MultipartFormDataUtils.HandleMultipartData(context.Request, part =>
                    //     {
                    //         paramDict[part.Name] = new[] { part.Data };
                    //     }, (string name, string fileName, string contentType, string contentDisposition, byte[] buffer, int bytes) =>
                    //     {
                    //         HandleFileUpload(context, name, fileName, contentType, contentDisposition, buffer, bytes);
                    //     }, () => FinishFileUpload(context));

                    //    input = context.GetDictData<TInput>(paramDict, valueToObject, ignoreProperties);
                    //}
                    else
                        input = context.GetFormData<TInput>(valueToObject, ignoreProperties);
                }
            }
            catch (Exception ex)
            {
                //throw new NodeMethodException(410, "传入参数错误，请检查参数是否正确。");
                throw new Exception();
            }
            //自身参数处理器
            input = HandleParameter(context, input);
            //全局参数处理器
            //if (NodeManager.Instance.ParameterHandler != null)
            //    input = (TInput)NodeManager.Instance.ParameterHandler.Invoke(this, context, input);
            //调用
            return Invoke(context, input);
        }

        public abstract Task Invoke(HttpContext context, TInput input);
    }
}
