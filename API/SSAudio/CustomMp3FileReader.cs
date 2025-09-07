using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API.SSAudio
{
    public class CustomMp3FileReader : WaveStream
    {
        private readonly Stream _stream;
        private readonly IMp3FrameDecompressor _decompressor;
        private readonly WaveFormat _waveFormat;
        private long _position;
        private readonly object _lockObject = new object();

        public CustomMp3FileReader(Stream stream, Func<WaveFormat, IMp3FrameDecompressor> decompressorFactory)
        {
            _stream = stream;

            // 读取第一个帧来获取格式信息
            var frame = Mp3Frame.LoadFromStream(stream);
            if (frame == null)
                throw new InvalidDataException("无效的MP3文件");

            _waveFormat = new WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2);
            _decompressor = decompressorFactory(_waveFormat);

            // 重置流位置
            _stream.Position = 0;
        }

        public override WaveFormat WaveFormat => _waveFormat;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_lockObject)
            {
                int totalBytesRead = 0;
                int bytesRequired = count;

                while (bytesRequired > 0)
                {
                    var frame = Mp3Frame.LoadFromStream(_stream);
                    if (frame == null)
                        break;

                    // DecompressFrame 应该返回实际写入的字节数，而不是字节数组
                    int bytesRead = _decompressor.DecompressFrame(frame, buffer, offset);

                    totalBytesRead += bytesRead;
                    offset += bytesRead;
                    bytesRequired -= bytesRead;
                    _position += bytesRead;
                }

                return totalBytesRead;
            }
        }

        public virtual void Dispose()
        {
            _decompressor?.Dispose();
            _stream?.Dispose();
        }
    }

}
