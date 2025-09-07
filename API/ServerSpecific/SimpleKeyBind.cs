using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.API.ServerSpecific
{
    public class SimpleKeyBind
    {
        public string Id { get; private set; }
        public string Label { get; set; }
        public KeyCode Key { get; set; }
        public KeyCode DefaultKey { get; private set; }
        public string Description { get; set; }
        public bool AllowModification { get; set; } = true;

        public event Action<SimpleKeyBind> OnKeyPressed;
        public event Action<SimpleKeyBind> OnKeyDown;
        public event Action<SimpleKeyBind> OnKeyUp;

        public SimpleKeyBind(string id, string label, KeyCode defaultKey, string description = null)
        {
            Id = id;
            Label = label;
            Key = defaultKey;
            DefaultKey = defaultKey;
            Description = description;
        }

        public bool IsPressed() => UnityEngine.Input.GetKey(Key);
        public bool IsPressedDown() => UnityEngine.Input.GetKeyDown(Key);
        public bool IsPressedUp() => UnityEngine.Input.GetKeyUp(Key);

        public void SetKey(KeyCode newKey)
        {
            if (!AllowModification)
                throw new InvalidOperationException("此按键绑定不允许修改");

            Key = newKey;
        }

        public void ResetToDefault() => Key = DefaultKey;

        internal void Update()
        {
            if (IsPressed()) OnKeyPressed?.Invoke(this);
            if (IsPressedDown()) OnKeyDown?.Invoke(this);
            if (IsPressedUp()) OnKeyUp?.Invoke(this);
        }
    }
}
