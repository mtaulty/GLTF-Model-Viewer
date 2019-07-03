using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using MulticastMessaging;
using MulticastMessaging.Messages;
using System;
using UnityEngine;

namespace MulticastMessaging
{
    [MixedRealityExtensionService(SupportedPlatforms.WindowsUniversal)]
    public class NetworkMessagingProvider : BaseExtensionService, INetworkMessagingProvider
    {
        public event EventHandler<NewModelOnNetworkEventArgs> NewModelOnNetwork;
        public event EventHandler<TransformChangeEventArgs> TransformChange;
        public event EventHandler<DeletedModelOnNetworkEventArgs> DeletedModelOnNetwork;

        public NetworkMessagingProvider(
              IMixedRealityServiceRegistrar registrar,
              string name,
              uint priority,
              BaseMixedRealityProfile profile) : base(registrar, name, priority, profile)
        {

        }
        // Note - this defines a dependency between this script and the IP Address Provider
        // being created before we arrive here.
        public void Initialise()
        {
            if (IPAddressProvider.HasIpAddress)
            {
                messageRegistrar = new MessageRegistrar();

                newModelMessageKey =
                    messageRegistrar.RegisterMessageFactory<NewModelMessage>(
                        () => new NewModelMessage());

                transformMessageKey =
                    messageRegistrar.RegisterMessageFactory<TransformChangeMessage>(
                        () => new TransformChangeMessage());

                deleteMessageKey =
                    messageRegistrar.RegisterMessageFactory<DeleteModelMessage>(
                        () => new DeleteModelMessage());

                messageRegistrar.RegisterMessageHandler<NewModelMessage>(
                    OnNewModelOnNetwork);

                messageRegistrar.RegisterMessageHandler<TransformChangeMessage>(
                    OnTransformChangeOnNetwork);

                messageRegistrar.RegisterMessageHandler<DeleteModelMessage>(
                    OnDeletedModelOnNetwork);

                var messageService = MixedRealityToolkit.Instance.GetService<IMessageService>();
                messageService.MessageRegistrar = messageRegistrar;

                messageService.Open();
            }
        }
        public void SendDeletedModelMessage(Guid identifier)
        {
            if (IPAddressProvider.HasIpAddress)
            {
                var message = (DeleteModelMessage)messageRegistrar.CreateMessage(deleteMessageKey);
                message.ModelIdentifier = identifier;

                var messageService = MixedRealityToolkit.Instance.GetService<IMessageService>();
                messageService.Send(message);
            }
        }
        public void SendNewModelMessage(Guid identifier)
        {
            if (IPAddressProvider.HasIpAddress)
            {
                var message = (NewModelMessage)messageRegistrar.CreateMessage(newModelMessageKey);
                message.ServerIPAddress = IPAddressProvider.IPAddress;
                message.ModelIdentifier = identifier;
                var messageService = MixedRealityToolkit.Instance.GetService<IMessageService>();
                messageService.Send(message);
            }
        }
        public void SendTransformChangeMessage(Guid identifier,
            Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            if (IPAddressProvider.HasIpAddress)
            {
                var message = (TransformChangeMessage)messageRegistrar.CreateMessage(transformMessageKey);
                message.ModelIdentifier = identifier;
                message.Scale = scale;
                message.Rotation = rotation;
                message.Translation = translation;

                var messageService = MixedRealityToolkit.Instance.GetService<IMessageService>();
                messageService.Send(message);
            }
        }
        void OnNewModelOnNetwork(object obj)
        {
            NewModelMessage message = obj as NewModelMessage;

            NewModelOnNetwork?.Invoke(
                null, new NewModelOnNetworkEventArgs(message.ModelIdentifier, message.ServerIPAddress));
        }
        void OnTransformChangeOnNetwork(object obj)
        {
            TransformChangeMessage message = obj as TransformChangeMessage;

            // TODO: maybe stop allocating so many of these if we're going to do this at 60fps.
            TransformChange?.Invoke(
                null,
                new TransformChangeEventArgs(
                    message.ModelIdentifier,
                    message.Scale,
                    message.Rotation,
                    message.Translation
                )
            );
        }
        void OnDeletedModelOnNetwork(object obj)
        {
            DeleteModelMessage message = obj as DeleteModelMessage;

            DeletedModelOnNetwork?.Invoke(
                null, new DeletedModelOnNetworkEventArgs(message.ModelIdentifier));
        }
        MessageTypeKey newModelMessageKey;
        MessageTypeKey transformMessageKey;
        MessageTypeKey deleteMessageKey;
        MessageService messageService;
        MessageRegistrar messageRegistrar;
    }
}