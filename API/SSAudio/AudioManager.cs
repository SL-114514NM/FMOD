using FMOD.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API.SSAudio
{
    public static class AudioManager
    {
        private static readonly List<AudioPlayer> ActivePlayers = new List<AudioPlayer>();
        private static readonly Dictionary<ReferenceHub, List<NetworkAudioPlayer>> PlayerAudioPlayers =
            new Dictionary<ReferenceHub, List<NetworkAudioPlayer>>();

        public static AudioPlayer PlayAudio(string filePath, AudioType audioType, bool loop = false, float volume = 1.0f)
        {
            var player = AudioPlayer.LoadFromFile(filePath, audioType);
            if (player != null)
            {
                player.Loop = loop;
                player.Volume = volume;
                player.Play();
                ActivePlayers.Add(player);

                player.PlaybackEnded += () => RemovePlayer(player);
            }
            return player;
        }
        /// <summary>
        /// 播放音频文件给指定玩家
        /// </summary>
        public static NetworkAudioPlayer PlayAudioForPlayer(ReferenceHub targetPlayer, string filePath,
            AudioType audioType, bool loop = false, float volume = 1.0f, bool is3DAudio = false)
        {
            if (targetPlayer == null)
                throw new ArgumentNullException(nameof(targetPlayer));

            var audioData = LoadAudioData(filePath, audioType);
            if (audioData == null) return null;

            var networkPlayer = NetworkAudioPlayer.CreateForPlayer(targetPlayer, is3DAudio);
            networkPlayer.Initialize(audioData.Value.data, audioData.Value.sampleRate, audioData.Value.channels);
            networkPlayer.Loop = loop;
            networkPlayer.Volume = volume;
            networkPlayer.Play();

            // 添加到玩家专属列表
            if (!PlayerAudioPlayers.ContainsKey(targetPlayer))
                PlayerAudioPlayers[targetPlayer] = new List<NetworkAudioPlayer>();

            PlayerAudioPlayers[targetPlayer].Add(networkPlayer);

            networkPlayer.PlaybackEnded += () => RemoveNetworkPlayer(targetPlayer, networkPlayer);
            return networkPlayer;
        }
        private static (float[] data, int sampleRate, int channels)? LoadAudioData(string filePath, AudioType audioType)
        {
            try
            {
                switch (audioType)
                {
                    case AudioType.Ogg:
                        return AudioPlayer.LoadOggFile(filePath);
                    case AudioType.Mp3:
                        return AudioPlayer.LoadMp3File(filePath);
                    case AudioType.Wav:
                        return AudioPlayer.LoadWavFile(filePath);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"加载音频数据失败: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 停止指定玩家的所有音频
        /// </summary>
        public static void StopAudioForPlayer(ReferenceHub targetPlayer)
        {
            if (targetPlayer != null && PlayerAudioPlayers.ContainsKey(targetPlayer))
            {
                foreach (var player in PlayerAudioPlayers[targetPlayer].ToArray())
                {
                    player.Stop();
                    player.Dispose();
                }
                PlayerAudioPlayers[targetPlayer].Clear();
            }
        }

        /// <summary>
        /// 暂停指定玩家的所有音频
        /// </summary>
        public static void PauseAudioForPlayer(ReferenceHub targetPlayer)
        {
            if (targetPlayer != null && PlayerAudioPlayers.ContainsKey(targetPlayer))
            {
                foreach (var player in PlayerAudioPlayers[targetPlayer])
                {
                    player.Pause();
                }
            }
        }

        /// <summary>
        /// 恢复指定玩家的所有音频
        /// </summary>
        public static void ResumeAudioForPlayer(ReferenceHub targetPlayer)
        {
            if (targetPlayer != null && PlayerAudioPlayers.ContainsKey(targetPlayer))
            {
                foreach (var player in PlayerAudioPlayers[targetPlayer])
                {
                    player.Play();
                }
            }
        }

        /// <summary>
        /// 设置指定玩家的所有音频音量
        /// </summary>
        public static void SetVolumeForPlayer(ReferenceHub targetPlayer, float volume)
        {
            if (targetPlayer != null && PlayerAudioPlayers.ContainsKey(targetPlayer))
            {
                foreach (var player in PlayerAudioPlayers[targetPlayer])
                {
                    player.Volume = volume;
                }
            }
        }
        private static void RemoveNetworkPlayer(ReferenceHub targetPlayer, NetworkAudioPlayer player)
        {
            if (targetPlayer != null && PlayerAudioPlayers.ContainsKey(targetPlayer))
            {
                PlayerAudioPlayers[targetPlayer].Remove(player);
            }
            player.Dispose();
        }

        /// <summary>
        /// 停止所有音频
        /// </summary>
        public static void StopAll()
        {
            foreach (var player in ActivePlayers.ToArray())
            {
                player.Stop();
                player.Dispose();
            }
            ActivePlayers.Clear();
        }

        /// <summary>
        /// 暂停所有音频
        /// </summary>
        public static void PauseAll()
        {
            foreach (var player in ActivePlayers)
            {
                player.Pause();
            }
        }

        /// <summary>
        /// 恢复所有音频
        /// </summary>
        public static void ResumeAll()
        {
            foreach (var player in ActivePlayers)
            {
                player.Play();
            }
        }

        private static void RemovePlayer(AudioPlayer player)
        {
            ActivePlayers.Remove(player);
            player.Dispose();
        }
    }
}
