using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API.SSHint
{
    public class SSHint: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _content;
        private float _duration;
        private float _verticalCoordinate = 700f;
        private string _color = "white";
        private int _fontSize = 20;
        private bool _isActive = true;
        private float _elapsedTime;

        /// <summary>
        /// 提示内容
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        /// <summary>
        /// 显示持续时间（秒）
        /// </summary>
        public float Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged(nameof(Duration));
                }
            }
        }

        /// <summary>
        /// 垂直坐标（屏幕像素位置）
        /// </summary>
        public float VerticalCoordinate
        {
            get => _verticalCoordinate;
            set
            {
                if (_verticalCoordinate != value)
                {
                    _verticalCoordinate = value;
                    OnPropertyChanged(nameof(VerticalCoordinate));
                }
            }
        }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

        /// <summary>
        /// 字体大小
        /// </summary>
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged(nameof(FontSize));
                }
            }
        }

        /// <summary>
        /// 是否激活显示
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        /// <summary>
        /// 已过去的时间
        /// </summary>
        public float ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                if (_elapsedTime != value)
                {
                    _elapsedTime = value;
                    OnPropertyChanged(nameof(ElapsedTime));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 获取格式化后的提示文本
        /// </summary>
        public virtual string GetFormattedText()
        {
            return $"<color={Color}><size={FontSize}>{Content}</size></color>";
        }

        /// <summary>
        /// 更新提示状态
        /// </summary>
        public virtual void Update(float deltaTime)
        {
            if (!IsActive) return;

            ElapsedTime += deltaTime;
            if (ElapsedTime >= Duration)
            {
                IsActive = false;
            }
        }
    }
}
