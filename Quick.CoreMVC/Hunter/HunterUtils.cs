using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Quick.CoreMVC.Hunter
{
    public class HunterUtils
    {
        public static void TryHunt(Object obj, IDictionary<String, String> properties)
        {
            TryHunt(obj.GetType(), obj, properties);
        }

        public static void TryHunt(Type objType, Object obj, IDictionary<String, String> properties)
        {
            if (obj is IHungryPropertyHunter)
            {
                IHungryPropertyHunter hunter = (IHungryPropertyHunter)obj;
                hunter.Hunt(properties);
            }
            if (obj is IPropertyHunter)
            {
                IPropertyHunter hunter = (IPropertyHunter)obj;
                var prefix = objType.FullName + ".";
                foreach (String key in properties.Keys.Where(t => t.StartsWith(prefix)))
                    hunter.Hunt(key.Substring(prefix.Length), properties[key]);
            }
        }

        public static void TryHunt(IEnumerable objs, IDictionary<String, String> properties)
        {
            foreach (var obj in objs)
                TryHunt(obj.GetType(), obj, properties);
        }
    }
}
