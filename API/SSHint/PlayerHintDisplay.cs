using MEC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.API.SSHint
{
    public class PlayerHintDisplay : IDisposable
    {
        private readonly ReferenceHub _player;
        private readonly List<SSHint> _activeHints = new List<SSHint>();
        private CoroutineHandle _updateCoroutine;
        private bool _isInitialized;

        /// <summary>
        /// 当前活动的提示数量
        /// </summary>
        public int ActiveHintCount => _activeHints.Count;

        public PlayerHintDisplay(ReferenceHub player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized) return;

            _updateCoroutine = Timing.RunCoroutine(UpdateCoroutine());
            _isInitialized = true;
        }

        /// <summary>
        /// 添加提示
        /// </summary>
        public void AddHint(SSHint hint)
        {
            if (hint == null) return;

            hint.PropertyChanged += OnHintPropertyChanged;
            _activeHints.Add(hint);

            // 立即更新显示
            UpdateDisplay();
        }

        /// <summary>
        /// 移除提示
        /// </summary>
        public void RemoveHint(SSHint hint)
        {
            if (hint == null) return;

            hint.PropertyChanged -= OnHintPropertyChanged;
            _activeHints.Remove(hint);

            UpdateDisplay();
        }

        /// <summary>
        /// 清除所有提示
        /// </summary>
        public void ClearAllHints()
        {
            foreach (var hint in _activeHints)
            {
                hint.PropertyChanged -= OnHintPropertyChanged;
            }
            _activeHints.Clear();

            UpdateDisplay();
        }

        /// <summary>
        /// 提示属性变化时的处理
        /// </summary>
        private void OnHintPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// 更新显示协程
        /// </summary>
        private IEnumerator<float> UpdateCoroutine()
        {
            while (_player != null && _player.isLocalPlayer)
            {
                yield return Timing.WaitForOneFrame;

                try
                {
                    // 更新所有提示的状态
                    UpdateHints(Time.deltaTime);

                    // 移除过期的提示
                    RemoveExpiredHints();

                    // 更新显示
                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"提示更新错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 更新所有提示状态
        /// </summary>
        private void UpdateHints(float deltaTime)
        {
            foreach (var hint in _activeHints)
            {
                hint.Update(deltaTime);
            }
        }

        /// <summary>
        /// 移除过期的提示
        /// </summary>
        private void RemoveExpiredHints()
        {
            for (int i = _activeHints.Count - 1; i >= 0; i--)
            {
                if (!_activeHints[i].IsActive)
                {
                    _activeHints[i].PropertyChanged -= OnHintPropertyChanged;
                    _activeHints.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 更新玩家显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (_player == null || !_player.isLocalPlayer) return;

            // 合并所有活动提示的文本
            string combinedText = CombineHintTexts();

            // 发送给玩家
            SendHintToPlayer(combinedText);
        }

        /// <summary>
        /// 合并所有提示文本
        /// </summary>
        private string CombineHintTexts()
        {
            if (_activeHints.Count == 0) return string.Empty;

            var textLines = new List<string>();
            foreach (var hint in _activeHints)
            {
                if (hint.IsActive)
                {
                    textLines.Add(hint.GetFormattedText());
                }
            }

            return string.Join("\n", textLines);
        }

        /// <summary>
        /// 发送提示给玩家
        /// </summary>
        private void SendHintToPlayer(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Player.Get(_player).SendHint("", 0.1f);
                return;
            }

            Player.Get(_player).SendHint("", 0.1f);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_isInitialized) return;

            Timing.KillCoroutines(_updateCoroutine);
            ClearAllHints();
            _isInitialized = false;
        }
    }
}
