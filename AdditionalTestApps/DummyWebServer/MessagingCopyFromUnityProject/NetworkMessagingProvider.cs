using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using MulticastMessaging;
using MulticastMessaging.Messages;

internal static class NetworkMessagingProvider
{
    // Note - this defines a dependency between this script and the IP Address Provider
    // being created before we arrive here.
    internal static void Initialise(
        Action<object> newModelCallback,
        Action<object> deletedModelCallback,
        Action<object> transformCallback)
    {
        if (IPAddressProvider.HasIpAddress)
        {
            messageRegistrar = new MessageRegistrar();

            newModelMessageKey =
                messageRegistrar.RegisterMessageFactory<NewModelMessage>(
                    () => new NewModelMessage());

            messageRegistrar.RegisterMessageHandler<NewModelMessage>(newModelCallback);

            transformMessageKey =
                messageRegistrar.RegisterMessageFactory<TransformChangeMessage>(
                    () => new TransformChangeMessage());

            messageRegistrar.RegisterMessageHandler<TransformChangeMessage>(
                transformCallback);

            deletedMessageKey =
                messageRegistrar.RegisterMessageFactory<DeleteModelMessage>(
                    () => new DeleteModelMessage());

            messageRegistrar.RegisterMessageHandler<DeleteModelMessage>(
                deletedModelCallback);

            messageService = new MessageService(messageRegistrar,
                IPAddressProvider.IPAddress.ToString());

            messageService.Open();
        }
    }
    internal static void SendNewModelMessage(Guid identifier)
    {
        if (IPAddressProvider.HasIpAddress)
        {
            var message = (NewModelMessage)messageRegistrar.CreateMessage(newModelMessageKey);
            message.ServerIPAddress = IPAddressProvider.IPAddress;
            message.ModelIdentifier = identifier;
            messageService.Send(message);
        }
    }
    internal static void SendDeleteModelMessage(Guid identifier)
    {
        if (IPAddressProvider.HasIpAddress)
        {
            var message = (DeleteModelMessage)messageRegistrar.CreateMessage(deletedMessageKey);
            message.ModelIdentifier = identifier;
            messageService.Send(message);
        }
    }
    internal static void SendTransformChangeMessage(Guid identifier,
        Vector3 scale, Quaternion rotation, Vector3 translation)
    {
        if (IPAddressProvider.HasIpAddress)
        {
            var message = (TransformChangeMessage)messageRegistrar.CreateMessage(transformMessageKey);
            message.ModelIdentifier = identifier;
            message.Scale = scale;
            message.Rotation = rotation;
            message.Translation = translation;
            messageService.Send(message);
        }
    }
    static MessageTypeKey newModelMessageKey;
    static MessageTypeKey transformMessageKey;
    static MessageTypeKey deletedMessageKey;
    static MessageService messageService;
    static MessageRegistrar messageRegistrar;
}