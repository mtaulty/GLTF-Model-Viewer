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

public class NetworkMessageManager : MonoBehaviour
{
    public event EventHandler<NewModelOnNetworkEventArgs> NewModelOnNetwork;

    // Note - this defines a dependency between this script and the IP Address Provider
    // being created before we arrive here.
    void Start()
    {
#if ENABLE_WINMD_SUPPORT

        if (IPAddressProvider.HasIpAddress)
        {
            this.messageRegistrar = new MessageRegistrar();

            this.newModelMessageKey =
                this.messageRegistrar.RegisterMessageFactory<NewModelMessage>(
                    () => new NewModelMessage());

            this.messageRegistrar.RegisterMessageHandler<NewModelMessage>(
                this.OnNewModelOnNetwork);

            this.messageService = new MessageService(this.messageRegistrar,
                IPAddressProvider.IPAddress.ToString());

            this.messageService.Open();
        }
#endif // ENABLE_WINMD_SUPPORT
    }
    public void SendNewModelMessage(Guid identifier)
    {
#if ENABLE_WINMD_SUPPORT
        if (IPAddressProvider.HasIpAddress)
        {
            var message = (NewModelMessage)this.messageRegistrar.CreateMessage(this.newModelMessageKey);
            message.ServerIPAddress = IPAddressProvider.IPAddress;
            message.ModelIdentifier = identifier;
            this.messageService.Send(message);
        }
#endif // ENABLE_WINMD_SUPPORT
    }

#if ENABLE_WINMD_SUPPORT
    void OnNewModelOnNetwork(object obj)
    {
        NewModelMessage message = obj as NewModelMessage;

        this.NewModelOnNetwork?.Invoke(
            this, new NewModelOnNetworkEventArgs(message.ModelIdentifier, message.ServerIPAddress));
    }

    MessageTypeKey newModelMessageKey;
    MessageService messageService;
    MessageRegistrar messageRegistrar;
#endif // ENABLE_WINMD_SUPPORT
}