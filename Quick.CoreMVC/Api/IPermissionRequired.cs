using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.CoreMVC.Api
{
    /// <summary>
    /// 请求权限
    /// </summary>
    public interface IPermissionRequired
    {
        /// <summary>
        /// 权限列表
        /// </summary>
        Enum[] Permissions { get; }
    }
}
