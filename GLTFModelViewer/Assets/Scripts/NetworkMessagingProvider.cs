using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;

#if ENABLE_WINMD_SUPPORT
using MulticastMessaging;
using MulticastMessaging.Messages;
#endif // ENABLE_WINMD_SUPPORT

public class NewModelOnNetworkEventArgs : EventArgs
{
    public NewModelOnNetworkEventArgs(Guid modelIdentifier,
        IPAddress ipAddress)
    {
        this.ModelIdentifier = modelIdentifier;
        this.IPAddress = ipAddress;
    }
    public IPAddress IPAddress { get; private set; }
    public Guid ModelIdentifier { get; private set; }
}

internal static class NetworkMessagingProvider 
{
    internal static event EventHandler<NewModelOnNetworkEventArgs> NewModelOnNetwork;

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

            messageRegistrar.RegisterMessageHandler<NewModelMessage>(
                OnNewModelOnNetwork);

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

#if ENABLE_WINMD_SUPPORT
    static void OnNewModelOnNetwork(object obj)
    {
        NewModelMessage message = obj as NewModelMessage;

        NewModelOnNetwork?.Invoke(
            null, new NewModelOnNetworkEventArgs(message.ModelIdentifier, message.ServerIPAddress));
    }

    static MessageTypeKey newModelMessageKey;
    static MessageService messageService;
    static MessageRegistrar messageRegistrar;
#endif // ENABLE_WINMD_SUPPORT
}