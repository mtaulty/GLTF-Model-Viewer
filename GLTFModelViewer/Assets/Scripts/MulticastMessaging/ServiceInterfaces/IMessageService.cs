using Microsoft.MixedReality.Toolkit;
using System;

namespace MulticastMessaging
{
    public interface IMessageService : IMixedRealityExtensionService
    {
        MessageRegistrar MessageRegistrar { get; set; }
        void Close();
        void Open();
        void Send<T>(T message, Action<bool> callback = null) where T : Message;
    }
}