using FMOD.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Events.Interfaces
{
    public interface IPlayerEvent
    {
        Player Player { get; set; }
    }
}
