using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API.SSAudio
{
    public struct AudioFormat
    {
        public int SampleRate;
        public int Channels;
        public float[] AudioData;
        public int Length;

        public float Duration => Length / (float)(SampleRate * Channels);
    }
}
