using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.CoreMVC.Api
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ApiParameterAttribute : Attribute
    {
        public bool ValueToObject { get; set; }
        public String[] IgnoreProperties { get; set; }

        public ApiParameterAttribute(bool valueToObject, params String[] ignoreProperties)
        {
            ValueToObject = valueToObject;
            IgnoreProperties = ignoreProperties;
        }
    }
}
