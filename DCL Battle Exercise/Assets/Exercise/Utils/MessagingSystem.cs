using System.Collections.Generic;
using System;

namespace Utils
{
    public static class MessagingSystem<T>
    {
        private static readonly List<ISubscriber<T>> _subscribers = new List<ISubscriber<T>>();

        public static void Publish(T data)
        {
            for (int i = 0; i < _subscribers.Count; i++)
            {
                if (_subscribers[i] != null)
                {
                    _subscribers[i].OnEvent(data);
                }
                else
                {
                    _subscribers.RemoveAt(i);
                    throw new NullReferenceException($"A Subscriber to the event type {data.GetType()} has been found null.");
                }
            }
        }

        public static void Subscribe(ISubscriber<T> subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public static void Unsubscribe(ISubscriber<T> subscriber)
        {
            _subscribers.Remove(subscriber);
        }
    }

    public interface ISubscriber<T>
    {
        void OnEvent(T evt);
    }
}