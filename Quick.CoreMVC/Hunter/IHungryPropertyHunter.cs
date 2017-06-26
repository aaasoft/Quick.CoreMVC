using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quick.CoreMVC.Hunter
{
    public interface IHungryPropertyHunter
    {
        void Hunt(IDictionary<String, String> properties);
    }
}
