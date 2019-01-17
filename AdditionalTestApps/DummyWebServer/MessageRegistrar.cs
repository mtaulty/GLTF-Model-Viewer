namespace MulticastMessaging
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class MessageRegistrar
    {
        public MessageRegistrar()
        {
            this.factories = new Dictionary<String, Func<Message>>();
            this.handlers = new Dictionary<string, List<Action<object>>>();
        }
        public MessageTypeKey RegisterMessageFactory<T>(Func<Message> factory) where T : Message
        {
            var key = KeyFromMessageType(typeof(T));

            this.factories[key] = factory;

            return (
                new MessageTypeKey()
                {
                    Key = key
                }
            );
        }
        public void UnregisterMessageFactory(MessageTypeKey token)
        {
            this.factories.Remove(token.Key);
        }
        public Message CreateMessage(MessageTypeKey token)
        {
            return (this.CreateMessage(token.Key));
        }
        public void RegisterMessageHandler<T>(MessageTypeKey token,
            Action<object> handler) where T : Message
        {
            if (!this.handlers.ContainsKey(token.Key))
            {
                this.handlers[token.Key] = new List<Action<object>>();
            }
            this.handlers[token.Key].Add(handler);
        }
        public void RegisterMessageHandler<T>(Action<object> handler) where T : Message
        {
            this.RegisterMessageHandler<T>(
                new MessageTypeKey()
                {
                    Key = KeyFromMessageType<T>()
                }, 
                handler);
        }
        public void UnregisterMessageHandler<T>(MessageTypeKey token,
            Action<object> handler) where T : Message
        {
            if (this.handlers.ContainsKey(token.Key))
            {
                this.handlers[token.Key].Remove(handler);
            }
        }
        internal Message CreateMessage(string keyToken)
        {
            var message = this.factories[keyToken]();
            return (message);
        }
        internal void InvokeHandlers(Message message)
        {
            var key = KeyFromMessageType(message.GetType());

            if (this.handlers.ContainsKey(key))
            {
                var entries = this.handlers[key];

                foreach (var entry in entries)
                {
                    entry(message);
                }
            }
        }
        internal static string KeyFromMessageType<T>()
        {
            return (KeyFromMessageType(typeof(T)));
        }
        static string KeyFromMessageType(Type messageType)
        {
            return (messageType.Name);
        }
        Dictionary<string, Func<Message>> factories;
        Dictionary<string, List<Action<object>>> handlers;
    }
}