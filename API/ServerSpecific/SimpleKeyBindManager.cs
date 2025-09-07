using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.API.ServerSpecific
{
    public static class SimpleKeyBindManager
    {
        private static readonly Dictionary<string, SimpleKeyBind> KeyBinds = new Dictionary<string, SimpleKeyBind>();
        public static SimpleKeyBind RegisterKeyBind(string id, string label, KeyCode defaultKey, string description = null)
        {
            if (KeyBinds.ContainsKey(id))
                throw new ArgumentException($"按键绑定 ID '{id}' 已存在");

            var keyBind = new SimpleKeyBind(id, label, defaultKey, description);
            KeyBinds.Add(id, keyBind);
            return keyBind;
        }

        public static SimpleKeyBind GetKeyBind(string id) => KeyBinds.TryGetValue(id, out var keyBind) ? keyBind : null;
        public static bool HasKeyBind(string id) => KeyBinds.ContainsKey(id);

        public static bool UnregisterKeyBind(string id) => KeyBinds.Remove(id);
        public static IEnumerable<SimpleKeyBind> GetAllKeyBinds() => KeyBinds.Values;

        public static void Update()
        {
            foreach (var keyBind in KeyBinds.Values)
            {
                keyBind.Update();
            }
        }
    }
}
