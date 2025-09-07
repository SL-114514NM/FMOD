using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Events.Interfaces
{
    public delegate void EventHandler<TEvent>(TEvent eventData);
    public class Event<TEvent>
    {
        private readonly List<EventHandler<TEvent>> _handlers = new List<EventHandler<TEvent>>();
        private readonly object _lock = new object();

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="handler">事件处理器</param>
        public void Subscribe(EventHandler<TEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                if (!_handlers.Contains(handler))
                {
                    _handlers.Add(handler);
                }
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="handler">事件处理器</param>
        public void Unsubscribe(EventHandler<TEvent> handler)
        {
            if (handler == null)
                return;

            lock (_lock)
            {
                _handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="eventData">事件数据</param>
        public void Invoke(TEvent eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            EventHandler<TEvent>[] handlersCopy;
            lock (_lock)
            {
                handlersCopy = _handlers.ToArray();
            }

            foreach (var handler in handlersCopy)
            {
                try
                {
                    handler(eventData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"事件处理器执行失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 清除所有订阅者
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }

        /// <summary>
        /// 获取订阅者数量
        /// </summary>
        public int SubscriberCount
        {
            get
            {
                lock (_lock)
                {
                    return _handlers.Count;
                }
            }
        }

        /// <summary>
        /// 检查是否包含指定的订阅者
        /// </summary>
        public bool Contains(EventHandler<TEvent> handler)
        {
            if (handler == null)
                return false;

            lock (_lock)
            {
                return _handlers.Contains(handler);
            }
        }

        /// <summary>
        /// 使用 += 运算符订阅事件
        /// </summary>
        public static Event<TEvent> operator +(Event<TEvent> eventSource, EventHandler<TEvent> handler)
        {
            eventSource?.Subscribe(handler);
            return eventSource;
        }

        /// <summary>
        /// 使用 -= 运算符取消订阅事件
        /// </summary>
        public static Event<TEvent> operator -(Event<TEvent> eventSource, EventHandler<TEvent> handler)
        {
            eventSource?.Unsubscribe(handler);
            return eventSource;
        }
    }
}
