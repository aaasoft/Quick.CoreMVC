﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using System.Net;
using Quick.CoreMVC.Utils;

namespace Quick.CoreMVC
{
    public static class HttpContextExtension
    {
        private static readonly String FORMDATA_KEY = $"{typeof(HttpContextExtension).FullName}.{nameof(FORMDATA_KEY)}";
        public static readonly String ACCEPT_LANGUAGE_KEY = "Accept-Language";

        ///// <summary>
        ///// 得到Session信息
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static IDictionary<String, Object> GetSession(this HttpContextExtension context)
        //{
        //    return SessionMiddleware.GetSession(context);
        //}

        ///// <summary>
        ///// 获取语言
        ///// </summary>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static String GetLanguage(this HttpContext context)
        //{
        //    var req = context.Request;
        //    String language = String.Empty;
        //    //先尝试从Cookie中读取语言地区
        //    language = context.Request.Cookies[ACCEPT_LANGUAGE_KEY];
        //    if (!String.IsNullOrEmpty(language))
        //        return language;
        //    //然后尝试从Header中读取语言地区
        //    var acceptLanguage = req.Headers.Get(ACCEPT_LANGUAGE_KEY);
        //    if (!String.IsNullOrEmpty(acceptLanguage))
        //        language = acceptLanguage.Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        //    if (String.IsNullOrEmpty(language))
        //        language = TextManager.DefaultLanguage;
        //    return language;
        //}

        //public static TextManager GetTextManager(this HttpContext context)
        //{
        //    return TextManager.GetInstance(context.GetLanguage());
        //}

        ///// <summary>
        ///// 设置语言
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="language"></param>
        //public static void SetLanguage(this HttpContext context, String language)
        //{
        //    var rep = context.Response;
        //    rep.Cookies.Append(ACCEPT_LANGUAGE_KEY, language);
        //}

        ///// <summary>
        ///// 得到当前语言文字
        ///// </summary>
        ///// <param name="context"></param>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public static String GetText(this HttpContext context, Enum key)
        //{
        //    String language = context.GetLanguage();
        //    var textManager = TextManager.GetInstance(language);
        //    return textManager.GetText(key);
        //}

        /// <summary>
        /// 获取POST提交的表单数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IFormCollection GetFormData(this HttpContext context)
        {
            return context.Request.Form;

            //IFormCollection formCollection = context.Get<IFormCollection>(FORMDATA_KEY);
            //if (formCollection != null)
            //    return formCollection;

            //StreamReader reader = new StreamReader(context.Request.Body);
            //var formData = reader.ReadToEnd();
            //IDictionary<String, IList<String>> dict = new Dictionary<String, IList<String>>();
            //foreach (var line in formData.Split('&'))
            //{
            //    var strs = line.Split('=');
            //    if (line.Length < 2)
            //        continue;
            //    var key = strs[0].Trim();
            //    var value = strs[1].Trim();
            //    value = WebUtility.UrlDecode(value);
            //    if (!dict.ContainsKey(key))
            //        dict.Add(key, new List<String>());
            //    dict[key].Add(value);
            //}
            //formCollection = new FormCollection(dict.ToDictionary(t => t.Key, t => t.Value.ToArray()));
            //context.Set(FORMDATA_KEY, formCollection);
            //return formCollection;
        }

        private static JObject getJObject(IEnumerable<KeyValuePair<string, string[]>> data, bool valueToObject, params String[] ignoreProperties)
        {
            JObject jObj = new JObject();
            foreach (var pair in data)
            {
                if (pair.Value.Length > 1)
                {
                    jObj.Add(pair.Key, JToken.FromObject(pair.Value));
                }
                else
                {
                    var text = pair.Value[0];
                    if (String.IsNullOrEmpty(text))
                        continue;
                    //如果要将字符串转换为JSON对象或数组
                    if (valueToObject)
                    {
                        //如果文本是一个JSON对象
                        if (text.StartsWith("{") && text.EndsWith("}"))
                        {
                            try
                            {
                                JObject subObj = JObject.Parse(text);
                                jObj.Add(pair.Key, subObj);
                                continue;
                            }
                            catch { }
                        }
                        //如果文本是一个JSON数组
                        if (text.StartsWith("[") && text.EndsWith("]"))
                        {
                            try
                            {
                                JArray subObj = JArray.Parse(text);
                                jObj.Add(pair.Key, subObj);
                                continue;
                            }
                            catch { }
                        }
                    }
                    jObj.Add(pair.Key, JToken.FromObject(text));
                }
            }
            if (ignoreProperties != null && ignoreProperties.Length > 0)
            {
                HashSet<String> ignoreProperyHashSet = new HashSet<string>(ignoreProperties);
                foreach (var property in jObj.Properties()
                    .Where(t => ignoreProperyHashSet.Contains(t.Name))
                    .ToArray())
                {
                    jObj.Remove(property.Name);
                }
            }
            return jObj;
        }

        /// <summary>
        /// 获取POST提交的表单数据到对象
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Object GetFormData(this HttpContext context, Type type)
        {
            return getJObject(GetFormData(context)
                .Select(t => new KeyValuePair<string, string[]>(t.Key, t.Value)), false)
                .ToObject(type);
        }

        /// <summary>
        /// 获取POST提交的表单数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T GetFormData<T>(this HttpContext context, params String[] ignoreProperties)
            where T : class
        {
            return context.GetFormData<T>(false, ignoreProperties);
        }

        /// <summary>
        /// 获取POST提交的表单数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="valueToObject">是否将值转换为对象</param>
        /// <returns></returns>
        public static T GetFormData<T>(this HttpContext context, bool valueToObject, params String[] ignoreProperties)
            where T : class
        {
            return getJObject(GetFormData(context)
                    .Select(t => new KeyValuePair<string, string[]>(t.Key, t.Value))
                    , valueToObject,
                    ignoreProperties)
                .ToObject<T>();
        }

        /// <summary>
        /// 根据字典得到数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="valueToObject"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static T GetDictData<T>(IEnumerable<KeyValuePair<string, string[]>> data, bool valueToObject, params String[] ignoreProperties)
        {
            return getJObject(data, valueToObject, ignoreProperties).ToObject<T>();
        }

        /// <summary>
        /// 根据字典得到数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="data"></param>
        /// <param name="valueToObject"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns></returns>
        public static T GetDictData<T>(this HttpContext context, IEnumerable<KeyValuePair<string, string[]>> data, bool valueToObject, params String[] ignoreProperties)
        {
            return getJObject(data, valueToObject, ignoreProperties).ToObject<T>();
        }

        /// <summary>
        /// 获取POST提交的表单数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetFormData<T>(this HttpContext context, T obj, params String[] ignoreProperties)
        {
            return context.GetFormData<T>(obj, false, ignoreProperties);
        }

        /// <summary>
        /// 获取POST提交的表单数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="obj"></param>
        /// <param name="valueToObject">是否将值转换为对象</param>
        /// <returns></returns>
        public static T GetFormData<T>(this HttpContext context, T obj, bool valueToObject, params String[] ignoreProperties)
        {
            var jsonString = getJObject(GetFormData(context)
                .Select(t => new KeyValuePair<string, string[]>(t.Key, t.Value)),
                valueToObject, ignoreProperties)
                .ToString();
            Boolean hasCompilerGeneratedAttribute = typeof(T).GetTypeInfo()
                .GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            if (hasCompilerGeneratedAttribute)
                return JsonConvert.DeserializeAnonymousType(jsonString, obj);
            JsonConvert.PopulateObject(jsonString, obj);
            return obj;
        }

        /// <summary>
        /// 获取URL参数数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T GetQueryData<T>(this HttpContext context, params String[] ignoreProperties)
            where T : class
        {
            return context.GetQueryData<T>(false, ignoreProperties);
        }

        /// <summary>
        /// 获取URL参数数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="valueToObject">是否将值转换为对象</param>
        /// <returns></returns>
        public static T GetQueryData<T>(this HttpContext context, bool valueToObject, params String[] ignoreProperties)
            where T : class
        {
            return getJObject(context.Request.Query.Select(t => new KeyValuePair<string, string[]>(t.Key, t.Value))
                .ToArray(), valueToObject, ignoreProperties).ToObject<T>();
        }

        /// <summary>
        /// 获取URL参数数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetQueryData<T>(this HttpContext context, T obj)
            where T : class
        {
            return context.GetQueryData<T>(obj, false);
        }

        /// <summary>
        /// 获取URL参数数据到对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="obj"></param>
        /// <param name="valueToObject">是否将值转换为对象</param>
        /// <returns></returns>
        public static T GetQueryData<T>(this HttpContext context, T obj, bool valueToObject)
            where T : class
        {
            var jsonString = getJObject(context.Request.Query.Select(t => new KeyValuePair<string, string[]>(t.Key, t.Value))
                .ToArray(), valueToObject).ToString();
            Boolean hasCompilerGeneratedAttribute = typeof(T).GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            if (hasCompilerGeneratedAttribute)
                return JsonConvert.DeserializeAnonymousType(jsonString, obj);
            JsonConvert.PopulateObject(jsonString, obj);
            return obj;
        }

        public static Boolean IsAllowCompress(this HttpRequest req)
        {
            if (!req.Headers.ContainsKey("Accept-Encoding"))
                return false;
            var acceptEncoding = req.Headers["Accept-Encoding"].FirstOrDefault();
            if (acceptEncoding == null)
                return false;
            return acceptEncoding.Contains("gzip");
        }

        public static Task Output(this HttpContext context, Stream stream, bool closeStreamWhenFinish, bool enableCompress = true, string resourceName = null, IDictionary<string, string> addonHttpHeaders = null)
        {
            var rep = context.Response;
            Task rtnTask = null;

            //如果要设置MIME
            if (!String.IsNullOrEmpty(resourceName))
            {
                //设置MIME类型
                var mime = MimeUtils.GetMime(resourceName);
                if (mime != null)
                    rep.ContentType = mime;
            }

            //添加额外的HTTP头
            if (addonHttpHeaders != null && addonHttpHeaders.Count > 0)
                foreach (var header in addonHttpHeaders)
                    context.Response.Headers[header.Key] = header.Value;

            //如果启用压缩
            if (enableCompress && IsAllowCompress(context.Request))
            {
                rep.Headers["Content-Encoding"] = "gzip";
                var gzStream = new GZipStream(rep.Body, CompressionMode.Compress);
                rtnTask = stream.CopyToAsync(gzStream)
                    .ContinueWith(t =>
                    {
                        gzStream.Flush();
                        gzStream.Dispose();

                        rep.Body.Flush();
                    });
            }
            else
            {
                rep.ContentLength = stream.Length;
                rtnTask = stream.CopyToAsync(rep.Body);
            }
            return rtnTask.ContinueWith(t =>
            {
                if (closeStreamWhenFinish)
                {
                    stream.Dispose();
                }
            });
        }

        public static Task Output(this HttpContext context, Byte[] content, bool enableCompress = true)
        {
            var rep = context.Response;

            //如果启用压缩
            if (enableCompress && IsAllowCompress(context.Request))
            {
                rep.Headers["Content-Encoding"] = "gzip";
                var gzStream = new GZipStream(rep.Body, CompressionMode.Compress);
                return gzStream.WriteAsync(content, 0, content.Length)
                    .ContinueWith(t =>
                    {
                        gzStream.Dispose();
                    });
            }
            else
            {
                rep.ContentLength = content.Length;
                return rep.Body.WriteAsync(content, 0, content.Length);
            }
        }


        public static void Add(this IDictionary<string, object> dict, Object obj)
        {
            Type type = obj.GetType();
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                dict[propertyInfo.Name] = propertyInfo.GetValue(obj, null);
            }
        }
    }
}
