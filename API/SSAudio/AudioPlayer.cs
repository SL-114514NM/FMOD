using FMOD.Enums;
using MEC;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AudioType = FMOD.Enums.AudioType;
using PlaybackState = FMOD.Enums.PlaybackState;

namespace FMOD.API.SSAudio
{
    public class AudioPlayer : MonoBehaviour, IDisposable
    {
        #region 核心属性

        private AudioFormat _format;
        private int _position;
        private PlaybackState _state = PlaybackState.Stopped;
        private float[] _playbackBuffer;
        private CoroutineHandle _playbackCoroutine;

        public AudioFormat Format => _format;
        public int Position
        {
            get => _position;
            set => _position = Mathf.Clamp(value, 0, _format.Length);
        }

        public float CurrentTime
        {
            get => _position / (float)(_format.SampleRate * _format.Channels);
            set => _position = (int)(value * _format.SampleRate * _format.Channels);
        }

        public float Duration => _format.Duration;
        public PlaybackState State => _state;
        public bool Loop { get; set; }
        public float Volume { get; set; } = 1.0f;

        public event Action PlaybackEnded;
        public event Action PlaybackStarted;
        public event Action PlaybackPaused;

        #endregion

        #region 音频加载

        public static AudioPlayer LoadFromFile(string filePath, AudioType audioType)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"音频文件不存在: {filePath}");

            try
            {
                float[] audioData;
                int sampleRate;
                int channels;

                switch (audioType)
                {
                    case AudioType.Ogg:
                        (audioData, sampleRate, channels) = LoadOggFile(filePath);
                        break;
                    case AudioType.Mp3:
                        (audioData, sampleRate, channels) = LoadMp3File(filePath);
                        break;
                    case AudioType.Wav:
                        (audioData, sampleRate, channels) = LoadWavFile(filePath);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的音频格式: {audioType}");
                }

                var player = new GameObject("AudioPlayer").AddComponent<AudioPlayer>();
                player.Initialize(audioData, sampleRate, channels);
                return player;
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载音频文件失败: {ex.Message}");
                return null;
            }
        }

        public static AudioPlayer LoadFromBytes(byte[] data, AudioType audioType)
        {
            try
            {
                float[] audioData;
                int sampleRate;
                int channels;

                switch (audioType)
                {
                    case AudioType.Ogg:
                        (audioData, sampleRate, channels) = LoadOggFromBytes(data);
                        break;
                    case AudioType.Mp3:
                        (audioData, sampleRate, channels) = LoadMp3FromBytes(data);
                        break;
                    case AudioType.Wav:
                        (audioData, sampleRate, channels) = LoadWavFromBytes(data);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的音频格式: {audioType}");
                }

                var player = new GameObject("AudioPlayer").AddComponent<AudioPlayer>();
                player.Initialize(audioData, sampleRate, channels);
                return player;
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载音频数据失败: {ex.Message}");
                return null;
            }
        }

        public void Initialize(float[] audioData, int sampleRate, int channels)
        {
            _format = new AudioFormat
            {
                AudioData = audioData,
                SampleRate = sampleRate,
                Channels = channels,
                Length = audioData.Length
            };

            _playbackBuffer = new float[1024];
            _position = 0;
        }

        #endregion

        #region 播放控制

        public void Play()
        {
            if (_state == PlaybackState.Playing) return;

            _state = PlaybackState.Playing;
            PlaybackStarted?.Invoke();

            if (_playbackCoroutine.IsRunning)
                Timing.KillCoroutines(_playbackCoroutine);

            _playbackCoroutine = Timing.RunCoroutine(PlaybackCoroutine());
        }

        public void Pause()
        {
            if (_state != PlaybackState.Playing) return;

            _state = PlaybackState.Paused;
            Timing.KillCoroutines(_playbackCoroutine);
            PlaybackPaused?.Invoke();
        }

        public void Stop()
        {
            _state = PlaybackState.Stopped;
            Timing.KillCoroutines(_playbackCoroutine);
            _position = 0;
            PlaybackEnded?.Invoke();
        }

        public void Seek(float time)
        {
            _position = (int)(time * _format.SampleRate * _format.Channels);
            _position = Mathf.Clamp(_position, 0, _format.Length);
        }

        private IEnumerator<float> PlaybackCoroutine()
        {
            while (_state == PlaybackState.Playing && _position < _format.Length)
            {
                ProcessAudioPacket();
                yield return Timing.WaitForOneFrame;
            }

            if (_position >= _format.Length)
            {
                if (Loop)
                {
                    _position = 0;
                    _playbackCoroutine = Timing.RunCoroutine(PlaybackCoroutine());
                }
                else
                {
                    Stop();
                }
            }
        }

        private void ProcessAudioPacket()
        {
            int samplesToRead = Mathf.Min(_playbackBuffer.Length, _format.Length - _position);

            if (samplesToRead == 0)
                return;

            Array.Copy(_format.AudioData, _position, _playbackBuffer, 0, samplesToRead);

            if (Volume != 1.0f)
            {
                for (int i = 0; i < samplesToRead; i++)
                {
                    _playbackBuffer[i] *= Volume;
                }
            }

            _position += samplesToRead;
            OnAudioDataAvailable(_playbackBuffer, samplesToRead);
        }

        protected virtual void OnAudioDataAvailable(float[] data, int length)
        {
            // 子类实现具体播放逻辑
        }

        #endregion

        #region 音频文件加载实现（使用正确的IMp3FrameDecompressor）

        public static (float[] data, int sampleRate, int channels) LoadOggFile(string filePath)
        {
            using (var vorbisReader = new VorbisWaveReader(filePath))
            {
                return ConvertWaveProviderToFloatArray(vorbisReader);
            }
        }

        public static (float[] data, int sampleRate, int channels) LoadMp3File(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return LoadMp3FromStream(fileStream);
            }
        }

        public static (float[] data, int sampleRate, int channels) LoadMp3FromBytes(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return LoadMp3FromStream(memoryStream);
            }
        }

        private static (float[] data, int sampleRate, int channels) LoadMp3FromStream(Stream stream)
        {
            try
            {
                var decoder = new NLayer.MpegFile(stream);
                var sampleRate = decoder.SampleRate;
                var channels = decoder.Channels;

                // 读取所有样本数据
                var samples = new float[decoder.Length];
                int samplesRead = decoder.ReadSamples(samples, 0, samples.Length);

                // 如果读取的样本数少于预期，调整数组大小
                if (samplesRead < samples.Length)
                {
                    Array.Resize(ref samples, samplesRead);
                }

                return (samples, sampleRate, channels);
            }
            catch (Exception ex)
            {
                Debug.LogError($"MP3 解码失败: {ex.Message}");
                throw;
            }
        }

        public static (float[] data, int sampleRate, int channels) LoadWavFile(string filePath)
        {
            using (var waveReader = new WaveFileReader(filePath))
            {
                return ConvertWaveStreamToFloatArray(waveReader);
            }
        }

        private static (float[] data, int sampleRate, int channels) LoadOggFromBytes(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            using (var vorbisReader = new VorbisWaveReader(memoryStream))
            {
                return ConvertWaveProviderToFloatArray(vorbisReader);
            }
        }

        private static (float[] data, int sampleRate, int channels) LoadWavFromBytes(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            using (var waveReader = new WaveFileReader(memoryStream))
            {
                return ConvertWaveStreamToFloatArray(waveReader);
            }
        }

        private static (float[] data, int sampleRate, int channels) ConvertWaveProviderToFloatArray(IWaveProvider provider)
        {
            var buffer = new byte[provider.WaveFormat.AverageBytesPerSecond * 10];
            using (var memoryStream = new MemoryStream())
            {
                int bytesRead;
                while ((bytesRead = provider.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                byte[] audioBytes = memoryStream.ToArray();
                return ConvertBytesToFloatArray(audioBytes, provider.WaveFormat);
            }
        }

        private static (float[] data, int sampleRate, int channels) ConvertWaveStreamToFloatArray(WaveStream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[stream.WaveFormat.AverageBytesPerSecond * 10];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, bytesRead);
                }

                byte[] audioBytes = memoryStream.ToArray();
                return ConvertBytesToFloatArray(audioBytes, stream.WaveFormat);
            }
        }

        private static (float[] data, int sampleRate, int channels) ConvertBytesToFloatArray(byte[] audioBytes, WaveFormat waveFormat)
        {
            int sampleRate = waveFormat.SampleRate;
            int channels = waveFormat.Channels;
            int bytesPerSample = waveFormat.BitsPerSample / 8;

            int sampleCount = audioBytes.Length / bytesPerSample;
            float[] audioData = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                int byteIndex = i * bytesPerSample;
                short sample = BitConverter.ToInt16(audioBytes, byteIndex);
                audioData[i] = sample / 32768f;
            }

            return (audioData, sampleRate, channels);
        }

        #endregion

        #region 清理资源

        public void Dispose()
        {
            Stop();
            if (gameObject != null)
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Stop();
            PlaybackEnded = null;
            PlaybackStarted = null;
            PlaybackPaused = null;
        }

        #endregion
    }
}
