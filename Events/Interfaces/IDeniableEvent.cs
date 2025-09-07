using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Events.Interfaces
{
    public interface IDeniableEvent
    {
        /// <summary>
        /// 获取或设置是否允许执行操作
        /// </summary>
        bool IsAllowed { get; set; }
    }
}
