using Microsoft.MixedReality.Toolkit;
using System;
using UnityEngine;

namespace MulticastMessaging
{
    public interface INetworkMessagingProvider : IMixedRealityExtensionService
    {
        event EventHandler<NewModelOnNetworkEventArgs> NewModelOnNetwork;
        event EventHandler<TransformChangeEventArgs> TransformChange;
        event EventHandler<DeletedModelOnNetworkEventArgs> DeletedModelOnNetwork;

        void Initialise();
        
        void SendDeletedModelMessage(Guid identifier);

        void SendNewModelMessage(Guid identifier);

        void SendTransformChangeMessage(Guid identifier,
            Vector3 scale, Quaternion rotation, Vector3 translation);
    }
}