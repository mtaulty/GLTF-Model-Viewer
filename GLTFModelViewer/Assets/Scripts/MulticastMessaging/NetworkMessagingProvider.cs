using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

#if ENABLE_WINMD_SUPPORT
using MulticastMessaging;
using MulticastMessaging.Messages;

#endif // ENABLE_WINMD_SUPPORT

internal static class NetworkMessagingProvider 
{
    internal static event EventHandler<NewModelOnNetworkEventArgs> NewModelOnNetwork;
    internal static event EventHandler<TransformChangeEventArgs> TransformChange;

    // Note - this defines a dependency between this script and the IP Address Provider
    // being created before we arrive here.
    internal static void Initialise()
    {
#if ENABLE_WINMD_SUPPORT

        if (IPAddressProvider.HasIpAddress)
        {
            messageRegistrar = new MessageRegistrar();

            newModelMessageKey =
                messageRegistrar.RegisterMessageFactory<NewModelMessage>(
                    () => new NewModelMessage());

            transformMessageKey =
                messageRegistrar.RegisterMessageFactory<TransformChangeMessage>(
                    () => new TransformChangeMessage());

            messageRegistrar.RegisterMessageHandler<NewModelMessage>(
                OnNewModelOnNetwork);

            messageRegistrar.RegisterMessageHandler<TransformChangeMessage>(
                OnTransformChangeOnNetwork);

            messageService = new MessageService(messageRegistrar);

            messageService.Open();
        }
#endif // ENABLE_WINMD_SUPPORT
    }
    internal static void SendNewModelMessage(Guid identifier)
    {
#if ENABLE_WINMD_SUPPORT
        if (IPAddressProvider.HasIpAddress)
        {
            var message = (NewModelMessage)messageRegistrar.CreateMessage(newModelMessageKey);
            message.ServerIPAddress = IPAddressProvider.IPAddress;
            message.ModelIdentifier = identifier;
            messageService.Send(message);
        }
#endif // ENABLE_WINMD_SUPPORT
    }
    internal static void SendTransformChangeMessage(Guid identifier,
        Vector3 scale, Quaternion rotation, Vector3 translation)
    {
#if ENABLE_WINMD_SUPPORT

        if (IPAddressProvider.HasIpAddress)
        {
            var message = (TransformChangeMessage)messageRegistrar.CreateMessage(transformMessageKey);
            message.ModelIdentifier = identifier;
            message.Scale = scale;
            message.Rotation = rotation;
            message.Translation = translation;
            messageService.Send(message);
        }

#endif // ENABLE_WINMD_SUPPORT
    }

#if ENABLE_WINMD_SUPPORT
    static void OnNewModelOnNetwork(object obj)
    {
        NewModelMessage message = obj as NewModelMessage;

        NewModelOnNetwork?.Invoke(
            null, new NewModelOnNetworkEventArgs(message.ModelIdentifier, message.ServerIPAddress));
    }
    static void OnTransformChangeOnNetwork(object obj)
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

    static MessageTypeKey newModelMessageKey;
    static MessageTypeKey transformMessageKey;
    static MessageService messageService;
    static MessageRegistrar messageRegistrar;
#endif // ENABLE_WINMD_SUPPORT
}