using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API.SSHint
{
    public class PlayerHintManager
    {
        private static readonly Dictionary<ReferenceHub, PlayerHintDisplay> PlayerDisplays =
            new Dictionary<ReferenceHub, PlayerHintDisplay>();
        /// <summary>
        /// 获取玩家的提示显示器
        /// </summary>
        public static PlayerHintDisplay GetPlayerDisplay(ReferenceHub player)
        {
            if (player == null) return null;

            if (!PlayerDisplays.TryGetValue(player, out var display))
            {
                display = new PlayerHintDisplay(player);
                PlayerDisplays[player] = display;
            }

            return display;
        }
        /// <summary>
        /// 向玩家发送提示
        /// </summary>
        public static void SendHint(ReferenceHub player, SSHint hint)
        {
            if (player == null || hint == null) return;

            var display = GetPlayerDisplay(player);
            display?.AddHint(hint);
        }

        /// <summary>
        /// 清除玩家的所有提示
        /// </summary>
        public static void ClearHints(ReferenceHub player)
        {
            if (player == null) return;

            if (PlayerDisplays.TryGetValue(player, out var display))
            {
                display.ClearAllHints();
            }
        }

        /// <summary>
        /// 移除玩家的提示显示器
        /// </summary>
        public static void RemovePlayerDisplay(ReferenceHub player)
        {
            if (player == null) return;

            if (PlayerDisplays.TryGetValue(player, out var display))
            {
                display.Dispose();
                PlayerDisplays.Remove(player);
            }
        }

        /// <summary>
        /// 清理所有提示显示器
        /// </summary>
        public static void CleanupAll()
        {
            foreach (var display in PlayerDisplays.Values)
            {
                display.Dispose();
            }
            PlayerDisplays.Clear();
        }
    }
}
