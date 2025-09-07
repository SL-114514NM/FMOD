using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceChat;

namespace FMOD.API.Interface
{
    public interface IVoiceRole
    {
        VoiceChatChannel VoiceChatChannel { get; set; }
    }
}
