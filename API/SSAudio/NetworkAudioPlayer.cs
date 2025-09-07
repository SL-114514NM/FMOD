using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoiceChat.Networking;

namespace FMOD.API.SSAudio
{
    public class NetworkAudioPlayer : AudioPlayer
    {
        private ReferenceHub _targetPlayer;
        private bool _is3DAudio;
        private float _maxDistance = 50f;
        private AudioMessage _audioMessage;
        private byte _controllerId = 1;

        /// <summary>
        /// 为目标玩家创建网络音频播放器
        /// </summary>
        public static NetworkAudioPlayer CreateForPlayer(ReferenceHub targetPlayer, bool is3DAudio = false)
        {
            var player = new GameObject("NetworkAudioPlayer").AddComponent<NetworkAudioPlayer>();
            player._targetPlayer = targetPlayer;
            player._is3DAudio = is3DAudio;
            player._audioMessage = new AudioMessage(player._controllerId, new byte[1024], 0);
            return player;
        }

        /// <summary>
        /// 设置3D音频参数
        /// </summary>
        public void Set3DSettings(float maxDistance)
        {
            _is3DAudio = true;
            _maxDistance = maxDistance;
        }

        /// <summary>
        /// 设置控制器ID（用于音频消息识别）
        /// </summary>
        public void SetControllerId(byte controllerId)
        {
            _controllerId = controllerId;
            _audioMessage.ControllerId = controllerId;
        }

        protected override void OnAudioDataAvailable(float[] data, int length)
        {
            base.OnAudioDataAvailable(data, length);

            if (_targetPlayer != null && _targetPlayer.isLocalPlayer)
            {
                SendAudioToPlayer(data, length);
            }
        }

        private void SendAudioToPlayer(float[] data, int length)
        {
            if (_is3DAudio)
            {
                // 计算3D音频参数
                float distance = Vector3.Distance(transform.position, _targetPlayer.transform.position);
                if (distance > _maxDistance) return;

                // 应用距离衰减
                float volume = 1.0f - (distance / _maxDistance);
                volume = Mathf.Clamp01(volume);

                // 发送带3D参数的音频数据
                Send3DAudioData(data, length, volume, distance);
            }
            else
            {
                // 发送2D音频数据
                Send2DAudioData(data, length);
            }
        }

        private void Send2DAudioData(float[] data, int length)
        {
            try
            {
                // 将浮点数据转换为字节数据（简化实现）
                byte[] audioBytes = ConvertFloatToByte(data, length);

                // 创建音频消息
                _audioMessage.DataLength = audioBytes.Length;
                Array.Copy(audioBytes, _audioMessage.Data, audioBytes.Length);

                // 发送给指定玩家
                if (_targetPlayer != null && _targetPlayer.connectionToClient != null)
                {
                    _targetPlayer.connectionToClient.Send(_audioMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"发送2D音频数据失败: {ex.Message}");
            }
        }

        private void Send3DAudioData(float[] data, int length, float volume, float distance)
        {
            try
            {
                // 应用距离衰减
                float[] processedData = new float[length];
                Array.Copy(data, processedData, length);

                for (int i = 0; i < length; i++)
                {
                    processedData[i] *= volume;
                }

                // 转换为字节数据
                byte[] audioBytes = ConvertFloatToByte(processedData, length);

                // 创建音频消息（可以添加3D音频参数）
                _audioMessage.DataLength = audioBytes.Length;
                Array.Copy(audioBytes, _audioMessage.Data, audioBytes.Length);

                // 发送给指定玩家
                if (_targetPlayer != null && _targetPlayer.connectionToClient != null)
                {
                    _targetPlayer.connectionToClient.Send(_audioMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"发送3D音频数据失败: {ex.Message}");
            }
        }

        private byte[] ConvertFloatToByte(float[] data, int length)
        {
            byte[] byteData = new byte[length * 2]; // 16-bit audio
            for (int i = 0; i < length; i++)
            {
                short sample = (short)(data[i] * 32767f);
                byteData[i * 2] = (byte)(sample & 0xFF);
                byteData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
            }
            return byteData;
        }

        private void Update()
        {
            if (_is3DAudio && _targetPlayer != null)
            {
                // 更新3D音频位置（跟随目标玩家）
                transform.position = _targetPlayer.transform.position;
            }
        }
    }
}
