using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API
{
    public class Log
    {
        public static void AddLog(string msg)
        {
            GameCore.Console.AddLog(msg, UnityEngine.Color.white);
        }
        public static void Debug(string msg)
        {
            GameCore.Console.AddLog(msg, UnityEngine.Color.blue);
        }
        public static void Error(string msg)
        {
            GameCore.Console.AddLog(msg, UnityEngine.Color.red);
        }
        public static void CustomInfo(string msg, UnityEngine.Color color)
        {
            GameCore.Console.AddLog(msg, color);
        }
    }
}
